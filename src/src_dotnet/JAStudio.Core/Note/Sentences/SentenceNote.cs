using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Storage.Converters;
using JAStudio.Core.Storage.Media;

namespace JAStudio.Core.Note.Sentences;

public class SentenceNote : JPNote, IHasTextToSpeechAudio
{
   public CachingSentenceConfigurationField Configuration { get; private set; }

   /// <summary>The corpus's own question for the sentence. While it's empty the source's question is shown instead (<see cref="Question"/>).</summary>
   public WritableStringValue CorpusQuestion { get; }

   /// <summary>The corpus's own answer for the sentence. While it's empty the source's answer is shown instead (<see cref="Answer"/>).</summary>
   public WritableStringValue CorpusAnswer { get; }

   /// <summary>The corpus's comments on the sentence, shown beside its source's comments.</summary>
   public WritableStringValue Comments { get; }

   /// <summary>What the sentence's source gave it when it was imported.</summary>
   public SentenceSourceData SourceData { get; }

   /// <summary>The name of the work or deck the sentence came from, as its source gave it, such as <c>Shirokuma Cafe</c>: see <see cref="SentenceDto.SourceName"/>.</summary>
   public WritableStringValue SourceName { get; }

   public WritableStringValue Reading { get; }

   /// <summary>
   /// The corpus's own audio for the sentence, added since its source: its TTS audio. The audio its source gave it is its <see cref="SentenceSourceData.Audio"/>,<br/>
   /// and <see cref="AllAudio"/> is both.
   /// </summary>
   public AudioReferenceList Audio { get; }

   /// <summary>All the sentence's audio, its <see cref="SentenceSourceData.Audio"/> and its <see cref="Audio"/>, in the order it's preferred in.</summary>
   public AllAudioOfANote AllAudio { get; }

   /// <summary>The corpus's own screenshots for the sentence, added since its source, such as one pasted in. Its card shows them after its source's.</summary>
   public MediaReferenceList<ImageReference> Screenshots { get; }

   ParsingResult _parsingResult;

   /// <summary>The question shown: the <see cref="CorpusQuestion"/>, or the source's question while the corpus has none.</summary>
   public SentenceQuestionField Question => new(CorpusQuestion, SourceData.Question);

   /// <summary>The answer shown: the <see cref="CorpusAnswer"/>, or the source's answer while the corpus has none, without its HTML.</summary>
   public StripHtmlOnReadFallbackStringField Answer => new(CorpusAnswer, SourceData.Answer);

   public SentenceNote(NoteServices services, SentenceDto? data = null) : base(services, data != null ? new SentenceId(data.Id) : SentenceId.New(), data?.Tags)
   {
      SourceName = new WritableStringValue(data?.SourceName ?? string.Empty, Guard);
      Reading = new WritableStringValue(data?.Reading ?? string.Empty, Guard);
      CorpusQuestion = new WritableStringValue(data?.CorpusQuestion ?? string.Empty, Guard);
      CorpusAnswer = new WritableStringValue(data?.CorpusAnswer ?? string.Empty, Guard);
      Comments = new WritableStringValue(data?.Comments ?? string.Empty, Guard);
      //wip: slice 9's step 3 reads the media lists from data itself, rather than from the media fields of today (TodaysMediaFields).
      SourceData = new SentenceSourceData(data?.SourceData, TodaysMediaFields.SentenceSourceAudioIn(data), TodaysMediaFields.SentenceSourceScreenshotsIn(data), Guard);
      Audio = new AudioReferenceList(TodaysMediaFields.SentenceAudioIn(data), Guard);
      Screenshots = new MediaReferenceList<ImageReference>([], Guard);
      AllAudio = new AllAudioOfANote(SourceData.Audio, Audio);

      Configuration = new CachingSentenceConfigurationField(this, data?.Configuration, Guard);
      _parsingResult = SentenceDto.CreateParsingResult(data?.ParsingResult);
   }

   public override void UpdateInCache()
   {
      Services.Collection.Sentences.Cache.JpNoteUpdated(this);
   }

   public override CorpusNoteDto ToDto() => SentenceNoteConverter.ToDto(this);

   protected override Dictionary<string, string> StudyNoteFieldsBesidesTheId() => new()
   {
      [AnkiFieldNames.Question] = GetQuestion(),
      [AnkiFieldNames.Sentence.Audio] = AnkiMediaMarkup.SoundTagsFor(AllAudio.InTheOrderPreferred.Take(1))
   };

   /// <summary>The file of the sentence's <see cref="AllAudioOfANote.Preferred"/> audio in the media layer, or null when it has no audio, or the media layer hasn't its file.</summary>
   public AudioAttachment? PreferredAudio => AllAudio.PreferredFileIn(Services.MediaFileIndex);

   /// <summary>All the sentence's screenshots, its source's, then the corpus's own (<see cref="Screenshots"/>), as its card shows them.</summary>
   public IReadOnlyList<ImageReference> AllScreenshots => [..SourceData.Screenshots.All, ..Screenshots.All];

   public override IEnumerable<MediaReference> MediaReferences => [..SourceData.Audio.All, ..SourceData.Screenshots.All, ..Audio.All, ..Screenshots.All];

   public override string GetQuestion() => Question.WithoutInvisibleSpace();

   public string TextToSpeak => IHasTextToSpeechAudio.SpeakableTextOf(GetQuestion());

   public override string GetAnswer() => Answer.Get();

   public AnalysisServices AnalysisServices => new(Services.Collection.Vocab, Services.DictLookup, Services.Settings);

   /// <summary>The analysis the sentence's stored parsing result is made from.</summary>
   public TextAnalysis CreateAnalysis() => new(AnalysisServices, Question.WithInvisibleSpace(), Configuration.Configuration);

   /// <summary>The analysis the sentence breakdown shows under <paramref name="breakdownParsingOptions"/>.</summary>
   public TextAnalysis CreateBreakdownAnalysis(BreakdownParsingOptions breakdownParsingOptions) =>
      new(AnalysisServices, Question.WithInvisibleSpace(), Configuration.Configuration, breakdownParsingOptions);

   public ParsingResult GetParsingResult() => _parsingResult;

   public void SetParsingResult(ParsingResult value) => Guard.Update(() => _parsingResult = value);

   public List<string> GetWords()
   {
      var parsedWords = GetParsingResult().ParsedWordsStrings();
      var highlightedWords = Configuration.HighlightedWords;
      var incorrectWords = Configuration.IncorrectMatches.Words();

      var allWords = new HashSet<string>(parsedWords);
      allWords.UnionWith(highlightedWords);
      allWords.ExceptWith(incorrectWords);

      return [.. allWords.Distinct()];
   }

   public override void UpdateGeneratedData()
   {
      base.UpdateGeneratedData();

      UpdateParsedWords();
   }

   public void UpdateParsedWords(bool force = false)
   {
      var parsingResult = GetParsingResult();
      var questionText = Question.WithoutInvisibleSpace();

      if(!force && parsingResult.Sentence == questionText && parsingResult.ParserVersion == TextAnalysis.Version)
         return;

      var analysis = CreateAnalysis();
      SetParsingResult(ParsingResult.FromAnalysis(analysis));
   }

   public List<string> ExtractKanji()
   {
      var clean = StringExtensions.StripHtmlMarkup(Question.WithoutInvisibleSpace());
      return
      [
         .. clean.Where(KanaUtils.CharacterIsKanji)
                 .Select(c => c.ToString())
      ];
   }

   public static SentenceNote CreateTestNote(NoteServices services, string question, string answer)
   {
      var note = new SentenceNote(services);
      note.SourceData.Question.Set(question);
      note.CorpusAnswer.Set(answer);
      note.UpdateGeneratedData();
      services.Collection.Sentences.Add(note);
      return note;
   }

   public static SentenceNote Create(NoteServices services, string question)
   {
      var note = new SentenceNote(services);
      note.SourceData.Question.Set(question);
      note.UpdateGeneratedData();
      services.Collection.Sentences.Add(note);
      return note;
   }
}
