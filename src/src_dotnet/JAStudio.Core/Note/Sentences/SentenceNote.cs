using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Compze.Teventive;
using Compze.Teventive.Taggregates;
using Compze.Teventive.TaggregateStores;
using JAStudio.Core.Anki;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Sentences.Tevents;
using JAStudio.Core.Storage.Converters;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Storage.Media;

namespace JAStudio.Core.Note.Sentences;

public partial class SentenceNote : JPNote, IHasTextToSpeechAudio, ISnapshotable<SentenceNote, SentenceDto>, ITeventRouted
{
   /// <remarks>The created tevent is also the source question's, which the source data's applier applies (<see cref="SentenceSourceData"/>).</remarks>
   static readonly TeventRouting<SentenceNote> Appliers = NoteAppliers.Extend<SentenceNote>(it => it
     .For<ISentenceTevent.PropertyUpdated.CorpusQuestion>(static (sentence, tevent) => sentence.CorpusQuestion = tevent.CorpusQuestion)
     .For<ISentenceTevent.PropertyUpdated.CorpusAnswer>(static (sentence, tevent) => sentence.CorpusAnswer = tevent.CorpusAnswer)
     .For<ISentenceTevent.PropertyUpdated.Comments>(static (sentence, tevent) => sentence.Comments = tevent.Comments)
     .For<ISentenceTevent.PropertyUpdated.SourceName>(static (sentence, tevent) => sentence.SourceName = tevent.SourceName)
     .For<ISentenceTevent.PropertyUpdated.Reading>(static (sentence, tevent) => sentence.Reading = tevent.Reading)
     .For<ISentenceTevent.PropertyUpdated.ParsingResult>(static (sentence, tevent) => sentence._parsingResult = tevent.ParsingResult)
     .For<ISentenceTevent.PropertyUpdated.ParsingCase>(static (sentence, tevent) => sentence.ParsingCase = tevent.ParsingCase)
     .For<ISentenceTevent.PropertyUpdated.Audio>(static (sentence, tevent) => sentence.Audio.Apply(tevent.Audio))
     .For<ISentenceTevent.PropertyUpdated.Screenshots>(static (sentence, tevent) => sentence.Screenshots.Apply(tevent.Screenshots))
     .Component(static sentence => sentence.SourceData)
     .Component(static sentence => sentence.Configuration));

   TeventRouting ITeventRouted.TeventAppliers => Appliers;

   public SentenceNoteConfiguration Configuration { get; private set; }

   /// <summary>The corpus's own question for the sentence. While it's empty the source's question is shown instead (<see cref="Question"/>).</summary>
   public string CorpusQuestion { get; private set; }

   /// <summary>The corpus's own answer for the sentence. While it's empty the source's answer is shown instead (<see cref="Answer"/>).</summary>
   public string CorpusAnswer { get; private set; }

   /// <summary>The corpus's comments on the sentence, shown beside its source's comments.</summary>
   public string Comments { get; private set; }

   /// <summary>Gives the sentence <paramref name="question"/>, trimmed, as its <see cref="CorpusQuestion"/>.</summary>
   public void SetCorpusQuestion(string question) => PublishIfChanged(CorpusQuestion, question.Trim(), static it => new SentenceTevent.CorpusQuestionUpdated(it));

   /// <summary>Gives the sentence <paramref name="answer"/>, trimmed, as its <see cref="CorpusAnswer"/>.</summary>
   public void SetCorpusAnswer(string answer) => PublishIfChanged(CorpusAnswer, answer.Trim(), static it => new SentenceTevent.CorpusAnswerUpdated(it));

   /// <summary>Gives the sentence <paramref name="comments"/>, trimmed, as its <see cref="Comments"/>.</summary>
   public void SetComments(string comments) => PublishIfChanged(Comments, comments.Trim(), static it => new SentenceTevent.CommentsUpdated(it));

   /// <summary>What the sentence's source gave it when it was imported.</summary>
   public SentenceSourceData SourceData { get; private set; }

   /// <summary>The name of the work or deck the sentence came from, as its source gave it, such as <c>Shirokuma Cafe</c>: see <see cref="SentenceDto.SourceName"/>.</summary>
   public string SourceName { get; private set; }

   /// <summary>Gives the sentence <paramref name="sourceName"/>, trimmed, as its <see cref="SourceName"/>.</summary>
   public void SetSourceName(string sourceName) => PublishIfChanged(SourceName, sourceName.Trim(), static it => new SentenceTevent.SourceNameUpdated(it));

   /// <summary>The sentence's reading, in kana.</summary>
   public string Reading { get; private set; }

   /// <summary>Gives the sentence <paramref name="reading"/>, trimmed, as its <see cref="Reading"/>.</summary>
   public void SetReading(string reading) => PublishIfChanged(Reading, reading.Trim(), static it => new SentenceTevent.ReadingUpdated(it));

   /// <summary>
   /// The corpus's own audio for the sentence, added since its source: its TTS audio. The audio its source gave it is its <see cref="SentenceSourceData.Audio"/>,<br/>
   /// and <see cref="AllAudio"/> is both.
   /// </summary>
   public AudioReferenceList Audio { get; private set; }

   /// <summary>All the sentence's audio, its <see cref="SentenceSourceData.Audio"/> and its <see cref="Audio"/>, in the order it's preferred in.</summary>
   public AllAudioOfANote AllAudio { get; private set; }

   /// <summary>The corpus's own screenshots for the sentence, added since its source, such as one pasted in. Its card shows them after its source's.</summary>
   public MediaReferenceList<ImageReference> Screenshots { get; private set; }

   ParsingResult _parsingResult;

   /// <summary>The question shown: the <see cref="CorpusQuestion"/>, or the source's question while the corpus has none.</summary>
   public SentenceQuestionField Question => new(this);

   /// <summary>The answer shown: the <see cref="CorpusAnswer"/>, or the source's answer while the corpus has none, without its HTML.</summary>
   public StripHtmlOnReadFallbackStringField Answer => new(CorpusAnswer, SourceData.Answer);

   /// <summary>An empty sentence, which is no sentence yet: <see cref="RestoreFrom"/> makes it a stored one, and <see cref="SentenceTevent.Created"/> a new one.</summary>
   internal SentenceNote(NoteServices services) : base(services) => BuildThePartsFrom(null);

   /// <summary>A new sentence, whose source's question is <paramref name="question"/>.</summary>
   public SentenceNote(NoteServices services, string question) : this(services)
   {
      Publish(new SentenceTevent.Created(SentenceId.New(), question.Trim()));
   }

   /// <summary>The sentence <paramref name="data"/> holds, as it was stored.</summary>
   public SentenceNote(NoteServices services, SentenceDto data) : this(services) => RestoreFrom(data);

   public SentenceDto TakeSnapshot() => SentenceNoteConverter.ToDto(this);

   public void RestoreFrom(SentenceDto snapshot)
   {
      RestoreTheNoteFrom(new SentenceId(snapshot.Id), snapshot);
      BuildThePartsFrom(snapshot);
   }

   /// <summary>Builds the parts that hold the sentence's stored data, from <paramref name="data"/>, or empty when it's null.</summary>
   [MemberNotNull(nameof(SourceName), nameof(Reading), nameof(CorpusQuestion), nameof(CorpusAnswer), nameof(Comments), nameof(SourceData), nameof(Audio),
                  nameof(Screenshots), nameof(AllAudio), nameof(Configuration), nameof(_parsingResult))]
   void BuildThePartsFrom(SentenceDto? data)
   {
      SourceName = data?.SourceName ?? string.Empty;
      Reading = data?.Reading ?? string.Empty;
      CorpusQuestion = data?.CorpusQuestion ?? string.Empty;
      CorpusAnswer = data?.CorpusAnswer ?? string.Empty;
      Comments = data?.Comments ?? string.Empty;
      SourceData = new SentenceSourceData(this, data?.SourceData);
      Audio = new AudioReferenceList(this, data?.Audio.Select(it => it.ToReference()) ?? [], static it => new SentenceTevent.AudioUpdated(it));
      Screenshots = new MediaReferenceList<ImageReference>(this, data?.Screenshots.Select(it => it.ToReference()) ?? [], static it => new SentenceTevent.ScreenshotsUpdated(it));
      AllAudio = new AllAudioOfANote(SourceData.Audio, Audio);

      Configuration = new SentenceNoteConfiguration(this, data?.Configuration);
      _parsingResult = SentenceDto.CreateParsingResult(data?.ParsingResult);
      ParsingCase = data?.ParsingCase?.ToParsingCase();
   }

   public override CorpusNoteDto ToDto() => TakeSnapshot();

   internal override void Delete() => Publish(new SentenceTevent.Deleted());

   protected override Type WrapperTeventImplementation => typeof(SentenceTevent<ISentenceTevent>);

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

   void IHasTextToSpeechAudio.Edit(Action<IHasTextToSpeechAudio> edit) => this.Edit(it => edit(it));
   ITaggregateReference<IHasTextToSpeechAudio> IHasTextToSpeechAudio.Reference() => this.Reference();

   public override string GetAnswer() => Answer.Get();

   public AnalysisServices AnalysisServices => new(Services.Collection.Vocab, Services.DictLookup, Services.Settings);

   /// <summary>The analysis the sentence's stored parsing result is made from.</summary>
   public TextAnalysis CreateAnalysis() => new(AnalysisServices, Question.WithInvisibleSpace(), Configuration.Configuration);

   /// <summary>The analysis the sentence breakdown shows under <paramref name="breakdownParsingOptions"/>.</summary>
   public TextAnalysis CreateBreakdownAnalysis(BreakdownParsingOptions breakdownParsingOptions) =>
      new(AnalysisServices, Question.WithInvisibleSpace(), Configuration.Configuration, breakdownParsingOptions);

   public ParsingResult GetParsingResult() => _parsingResult;

   /// <summary>Gives the sentence <paramref name="value"/> as its parsing result: a reparse that comes to the result it has changes nothing.</summary>
   public void SetParsingResult(ParsingResult value) => PublishIfChanged(_parsingResult, value, static it => new SentenceTevent.ParsingResultUpdated(it));

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
      var note = new SentenceNote(services, question);
      note.SetCorpusAnswer(answer);
      note.UpdateGeneratedData();
      services.Collection.Sentences.Add(note);
      return note;
   }

   public static SentenceNote Create(NoteServices services, string question)
   {
      var note = new SentenceNote(services, question);
      note.UpdateGeneratedData();
      services.Collection.Sentences.Add(note);
      return note;
   }
}
