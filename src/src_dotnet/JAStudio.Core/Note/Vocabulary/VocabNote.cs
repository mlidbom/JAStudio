using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Anki;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Storage.Converters;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.SysUtils;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNote : JPNote, IHasTextToSpeechAudio
{
   public VocabNoteQuestion Question { get; private set; }

   /// <summary>
   /// The corpus's own answer for the vocab: the canonical one. While it's empty the source's answer is shown instead (<see cref="GetAnswer"/>).
   /// </summary>
   public WritableStringValue CorpusAnswer { get; }

   /// <summary>The corpus's explanation of the vocab, shown on its card.</summary>
   public WritableStringValue Explanation { get; }

   /// <summary>The corpus's longer explanation of the vocab, shown on its card.</summary>
   public WritableStringValue ExplanationLong { get; }

   /// <summary>The corpus's own mnemonic for the vocab, shown on its card.</summary>
   public WritableStringValue Mnemonic { get; }

   /// <summary>What the vocab's source gave it when it was imported.</summary>
   public VocabSourceData SourceData { get; }

   public VocabNoteForms Forms { get; }
   public VocabNoteKanji Kanji { get; }
   public VocabNotePartsOfSpeech PartsOfSpeech { get; }
   public VocabNoteConjugator Conjugator { get; }
   public VocabNoteSentences Sentences { get; }
   public VocabNoteCompoundParts CompoundParts { get; }
   public Vocabulary.RelatedVocab.RelatedVocab RelatedNotes { get; }
   public VocabNoteMetaData MetaData { get; }
   public VocabNoteRegister Register { get; }
   public VocabNoteUsuallyWrittenInKana UsuallyWrittenInKana { get; }
   public VocabNoteAudio Audio { get; }
   public VocabNoteMatchingConfiguration MatchingConfiguration { get; }
   public VocabCloner Cloner { get; }

   public WritableStringValue TechnicalNotes { get; }
   public WritableStringValue References { get; }
   public WritableImageValue Image { get; }
   public WritableImageValue UserImage { get; }

   readonly List<string> _readings;

   public VocabNote(NoteServices services, VocabDto? data = null) : base(services, data != null ? new VocabId(data.Id) : VocabId.New(), data?.Tags)
   {
      CorpusAnswer = new WritableStringValue(data?.CorpusAnswer ?? string.Empty, Guard);
      Explanation = new WritableStringValue(data?.Explanation ?? string.Empty, Guard);
      ExplanationLong = new WritableStringValue(data?.ExplanationLong ?? string.Empty, Guard);
      Mnemonic = new WritableStringValue(data?.Mnemonic ?? string.Empty, Guard);
      SourceData = new VocabSourceData(data?.SourceData, Guard);
      TechnicalNotes = new WritableStringValue(data?.TechnicalNotes ?? string.Empty, Guard);
      References = new WritableStringValue(data?.References ?? string.Empty, Guard);
      Image = new WritableImageValue(data?.Image ?? string.Empty, Guard);
      UserImage = new WritableImageValue(data?.UserImage ?? string.Empty, Guard);
      _readings = data?.Readings != null ? [..data.Readings] : [];

      Question = new VocabNoteQuestion(this, data, Guard);
      Forms = new VocabNoteForms(this, data, Guard);
      Kanji = new VocabNoteKanji(this);
      PartsOfSpeech = new VocabNotePartsOfSpeech(this, data, Guard);
      Conjugator = new VocabNoteConjugator(this);
      Sentences = new VocabNoteSentences(this);
      CompoundParts = new VocabNoteCompoundParts(this, data, Guard);
      RelatedNotes = new Vocabulary.RelatedVocab.RelatedVocab(this, data?.RelatedVocab, Guard);
      MetaData = new VocabNoteMetaData(this, data, Guard);
      Register = new VocabNoteRegister(data?.Register ?? [], Guard);
      UsuallyWrittenInKana = new VocabNoteUsuallyWrittenInKana(data?.UsuallyWrittenInKana, Guard);
      Audio = new VocabNoteAudio(data, Guard);
      MatchingConfiguration = new VocabNoteMatchingConfiguration(PartsOfSpeech, data?.Matching, new NoteGuard(FlushAfterTheMatchingConfigurationChanged));
      Cloner = new VocabCloner(this);
   }

   public override void UpdateInCache()
   {
      Services.Collection.Vocab.Cache.JpNoteUpdated(this);
   }

   public override CorpusNoteDto ToDto() => VocabNoteConverter.ToDto(this);

   protected override Dictionary<string, string> StudyNoteFields() => new()
   {
      [AnkiFieldNames.Question] = Question.DisambiguationName,
      [AnkiFieldNames.Vocab.AudioB] = Audio.First.RawValue(),
      [AnkiFieldNames.Vocab.AudioG] = Audio.Second.RawValue(),
      [AnkiFieldNames.Vocab.AudioTTS] = Audio.Tts.RawValue()
   };

   public override string GetQuestion() => Question.Text;

   public string TextToSpeak => IHasTextToSpeechAudio.SpeakableTextOf(GetQuestion());

   public WritableAudioValue TextToSpeechAudio => Audio.Tts;

   public IReadOnlyList<AudioAttachment> FirstAudio   => Services.MediaFileIndex.GetAudioAttachments(Audio.First.RawValue());
   public IReadOnlyList<AudioAttachment> SecondAudio  => Services.MediaFileIndex.GetAudioAttachments(Audio.Second.RawValue());
   public IReadOnlyList<AudioAttachment> TtsAudio     => Services.MediaFileIndex.GetAudioAttachments(Audio.Tts.RawValue());
   public AudioAttachment?               PreferredAudio => FirstAudio.FirstOrDefault() ?? SecondAudio.FirstOrDefault() ?? TtsAudio.FirstOrDefault();
   public IReadOnlyList<ImageAttachment> Images       => Services.MediaFileIndex.GetImageAttachments(Image.RawValue());
   public IReadOnlyList<ImageAttachment> UserImages   => Services.MediaFileIndex.GetImageAttachments(UserImage.RawValue());

   public override List<MediaReference> MediaReferences
   {
      get
      {
         var refs = Audio.First.GetMediaReferences();
         refs.AddRange(Audio.Second.GetMediaReferences());
         refs.AddRange(Audio.Tts.GetMediaReferences());
         refs.AddRange(Image.GetMediaReferences());
         refs.AddRange(UserImage.GetMediaReferences());
         return refs;
      }
   }

   /// <summary>The answer shown: the <see cref="CorpusAnswer"/>, or the source's answer while the corpus has none.</summary>
   public override string GetAnswer() => CorpusAnswer.HasValue() ? CorpusAnswer.Value : SourceData.Answer.Value;

   /// <summary>
   /// Makes the source's answer the corpus's own (<see cref="CorpusAnswer"/>), written as the corpus writes a vocab's answers: in lower case, without<br/>
   /// markup, the meanings separated by slashes and the words of each joined by dashes. Only while the vocab has no answer of its own.
   /// </summary>
   public void AcceptMeaning()
   {
      Contract.State.Assert(!CorpusAnswer.HasValue(), () => $"The vocab {GetQuestion()} has an answer of its own already: {CorpusAnswer.Value}");
      CorpusAnswer.Set(WrittenAsTheCorpusWritesAVocabsAnswers(SourceData.Answer.Value));

      static string WrittenAsTheCorpusWritesAVocabsAnswers(string sourceAnswer) =>
         sourceAnswer.Replace(" SOURCE", "")
                     .Replace(", ", "/")
                     .Replace(" ", "-")
                     .ToLower()
                     .StripHtmlAndBracketMarkupAndNoiseCharacters();
   }

   /// <summary>The question and answer, the readings in kana and in romaji, and the forms.</summary>
   public override IEnumerable<string> SearchableTexts() =>
   [
      ..base.SearchableTexts(),
      .._readings,
      .._readings.Select(KanaUtils.Romanize),
      ..Forms.MatchedFormsWithoutNoiseCharacters()
   ];

   /// <summary>
   /// Flushes the vocab after a change to its <see cref="MatchingConfiguration"/>, once the <see cref="Forms"/> have dropped what they derived from it:<br/>
   /// the snapshot the flush takes must reflect the change.
   /// </summary>
   void FlushAfterTheMatchingConfigurationChanged()
   {
      Forms.OnMatchingConfigurationChanged();
      Flush();
   }

   public override void UpdateGeneratedData()
   {
      base.UpdateGeneratedData();

      Services.VocabNoteGeneratedData.UpdateGeneratedData(this);
   }

   public List<string> GetReadings() => [.. _readings];

   public void SetReadings(List<string> readings) => Guard.Update(() =>
   {
      _readings.Clear();
      _readings.AddRange(readings);
   });

   public void GenerateAndSetAnswer()
   {
      var dictLookup = Services.DictLookup.LookupVocabWordOrName(this);
      if(dictLookup.FoundWords())
      {
         var generated = dictLookup.FormatAnswer();
         SourceData.Answer.Set(generated);
      }

      UpdateGeneratedData();
   }

   public static VocabNote Create(NoteServices services, string question, string answer, List<string> readings, List<string> forms)
   {
      var note = new VocabNote(services);
      note.Question.SetText(question);
      note.CorpusAnswer.Set(answer);
      note.SetReadings(readings);

      if(forms.Any())
      {
         note.Forms.SetEnteredForms(forms.Select(EnteredForm.NotOwned));
      }

      note.UpdateGeneratedData();
      services.Collection.Vocab.Add(note);
      return note;
   }

   public static VocabNote Create(NoteServices services, string question, string answer, params string[] readings) =>
      Create(services, question, answer, [.. readings], []);
}
