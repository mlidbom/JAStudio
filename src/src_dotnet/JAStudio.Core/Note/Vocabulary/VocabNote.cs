using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Compze.Contracts;
using Compze.Teventive.Taggregates;
using JAStudio.Core.Anki;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Core.Note.Vocabulary.Tevents;
using JAStudio.Core.Storage.Converters;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.SysUtils;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNote : JPNote, IHasTextToSpeechAudio, ISnapshotable<VocabNote, VocabDto>
{
   public VocabNoteQuestion Question { get; private set; }

   /// <summary>
   /// The corpus's own answer for the vocab: the canonical one. While it's empty the source's answer is shown instead (<see cref="GetAnswer"/>).
   /// </summary>
   public WritableStringValue CorpusAnswer { get; private set; }

   /// <summary>The corpus's explanation of the vocab, shown on its card.</summary>
   public WritableStringValue Explanation { get; private set; }

   /// <summary>The corpus's longer explanation of the vocab, shown on its card.</summary>
   public WritableStringValue ExplanationLong { get; private set; }

   /// <summary>The corpus's own mnemonic for the vocab, shown on its card.</summary>
   public WritableStringValue Mnemonic { get; private set; }

   /// <summary>What the vocab's source gave it when it was imported.</summary>
   public VocabSourceData SourceData { get; private set; }

   public VocabNoteForms Forms { get; private set; }
   public VocabNoteKanji Kanji { get; }
   public VocabNotePartsOfSpeech PartsOfSpeech { get; private set; }
   public VocabNoteConjugator Conjugator { get; }
   public VocabNoteSentences Sentences { get; }
   public VocabNoteCompoundParts CompoundParts { get; private set; }
   public Vocabulary.RelatedVocab.RelatedVocab RelatedNotes { get; private set; }
   public VocabNoteMetaData MetaData { get; private set; }
   public VocabNoteRegister Register { get; private set; }
   public VocabNoteUsuallyWrittenInKana UsuallyWrittenInKana { get; private set; }

   /// <summary>
   /// The corpus's own audio for the vocab, added since its source: its TTS audio, and recordings from elsewhere, such as Yomitan's or Forvo's.<br/>
   /// The audio its source gave it is its <see cref="VocabSourceData.Audio"/>, and <see cref="AllAudio"/> is both.
   /// </summary>
   public AudioReferenceList Audio { get; private set; }

   /// <summary>All the vocab's audio, its <see cref="VocabSourceData.Audio"/> and its <see cref="Audio"/>, in the order it's preferred in.</summary>
   public AllAudioOfANote AllAudio { get; private set; }

   /// <summary>The corpus's own images for the vocab, added since its source, such as images from the web. Shown nowhere yet.</summary>
   public MediaReferenceList<ImageReference> Images { get; private set; }

   public VocabNoteMatchingConfiguration MatchingConfiguration { get; private set; }

   /// <summary>
   /// The texts a sentence must contain one of for the vocab to possibly match in it: the forms it matches by, and unless it must match a<br/>
   /// sentence's surface as written, the stems of all its forms, which its inflections start with. A sentence with none can't hold it, so<br/>
   /// only those with one need analyzing when the vocab changes.
   /// </summary>
   public List<string> TextsASentenceItCanMatchInContainsOneOf() =>
      MatchingConfiguration.MatchConditionRequires(MatchingRules.Surface)
         ? Forms.MatchedFormsList()
         : [.. Conjugator.GetStemsForAllForms().Concat(Forms.MatchedFormsList())];

   public VocabCloner Cloner { get; }

   public WritableStringValue TechnicalNotes { get; private set; }
   public WritableStringValue References { get; private set; }

   List<string> _readings;

   /// <summary>An empty vocab, which is no vocab yet: <see cref="RestoreFrom"/> makes it a stored one, and <see cref="VocabTevent.Created"/> a new one.</summary>
   internal VocabNote(NoteServices services) : base(services)
   {
      Kanji = new VocabNoteKanji(this);
      Conjugator = new VocabNoteConjugator(this);
      Sentences = new VocabNoteSentences(this);
      Cloner = new VocabCloner(this);
      BuildThePartsFrom(null);

      RegisterTeventAppliers()
        .For<IVocabTevent.Question.PropertyUpdated.Text>(it => Question.Apply(it));
   }

   /// <summary>A new vocab, whose question is <paramref name="question"/>. No other vocab may have it.</summary>
   public VocabNote(NoteServices services, string question) : this(services)
   {
      Question.AssertCanBeTheText(question);
      Publish(new VocabTevent.Created(VocabId.New(), question));
   }

   /// <summary>The vocab <paramref name="data"/> holds, as it was stored.</summary>
   public VocabNote(NoteServices services, VocabDto data) : this(services) => RestoreFrom(data);

   public VocabDto TakeSnapshot() => VocabNoteConverter.ToDto(this);

   public void RestoreFrom(VocabDto snapshot)
   {
      RestoreTheNoteFrom(new VocabId(snapshot.Id), snapshot);
      BuildThePartsFrom(snapshot);
   }

   /// <summary>Builds the parts that hold the vocab's stored data, from <paramref name="data"/>, or empty when it's null.</summary>
   [MemberNotNull(nameof(CorpusAnswer), nameof(Explanation), nameof(ExplanationLong), nameof(Mnemonic), nameof(SourceData), nameof(Audio), nameof(Images),
                  nameof(AllAudio), nameof(TechnicalNotes), nameof(References), nameof(_readings), nameof(Question), nameof(Forms), nameof(PartsOfSpeech),
                  nameof(CompoundParts), nameof(RelatedNotes), nameof(MetaData), nameof(Register), nameof(UsuallyWrittenInKana), nameof(MatchingConfiguration))]
   void BuildThePartsFrom(VocabDto? data)
   {
      CorpusAnswer = new WritableStringValue(data?.CorpusAnswer ?? string.Empty, Guard);
      Explanation = new WritableStringValue(data?.Explanation ?? string.Empty, Guard);
      ExplanationLong = new WritableStringValue(data?.ExplanationLong ?? string.Empty, Guard);
      Mnemonic = new WritableStringValue(data?.Mnemonic ?? string.Empty, Guard);
      SourceData = new VocabSourceData(data?.SourceData, Guard);
      Audio = new AudioReferenceList(data?.Audio.Select(it => it.ToReference()) ?? [], Guard);
      Images = new MediaReferenceList<ImageReference>(data?.Images.Select(it => it.ToReference()) ?? [], Guard);
      AllAudio = new AllAudioOfANote(SourceData.Audio, Audio);
      TechnicalNotes = new WritableStringValue(data?.TechnicalNotes ?? string.Empty, Guard);
      References = new WritableStringValue(data?.References ?? string.Empty, Guard);
      _readings = data?.Readings != null ? [..data.Readings] : [];

      Question = new VocabNoteQuestion(this, data, Guard);
      Forms = new VocabNoteForms(this, data, Guard);
      PartsOfSpeech = new VocabNotePartsOfSpeech(this, data, Guard);
      CompoundParts = new VocabNoteCompoundParts(this, data, Guard);
      RelatedNotes = new Vocabulary.RelatedVocab.RelatedVocab(this, data?.RelatedVocab, Guard);
      MetaData = new VocabNoteMetaData(this, data, Guard);
      Register = new VocabNoteRegister(data?.Register ?? [], Guard);
      UsuallyWrittenInKana = new VocabNoteUsuallyWrittenInKana(data?.UsuallyWrittenInKana, Guard);
      MatchingConfiguration = new VocabNoteMatchingConfiguration(PartsOfSpeech, data?.Matching, Services.SharedConditions, new NoteGuard(FlushAfterTheMatchingConfigurationChanged));
   }

   public override void UpdateInCache()
   {
      Services.Collection.Vocab.Cache.JpNoteUpdated(this);
   }

   public override CorpusNoteDto ToDto() => TakeSnapshot();

   protected override Type WrapperTeventImplementation => typeof(VocabTevent<IVocabTevent>);

   protected override Dictionary<string, string> StudyNoteFieldsBesidesTheId() => new()
   {
      [AnkiFieldNames.Question] = Question.DisambiguationName,
      [AnkiFieldNames.Vocab.AudioB] = AnkiMediaMarkup.SoundTagsFor(AllAudio.Recordings.Take(1)),
      [AnkiFieldNames.Vocab.AudioG] = AnkiMediaMarkup.SoundTagsFor(AllAudio.Recordings.Skip(1)),
      [AnkiFieldNames.Vocab.AudioTTS] = AnkiMediaMarkup.SoundTagsFor(AllAudio.TextToSpeech)
   };

   public override string GetQuestion() => Question.Text;

   public string TextToSpeak => IHasTextToSpeechAudio.SpeakableTextOf(GetQuestion());

   /// <summary>The file of the vocab's <see cref="AllAudioOfANote.Preferred"/> audio in the media layer, or null when it has no audio, or the media layer hasn't its file.</summary>
   public AudioAttachment? PreferredAudio => AllAudio.PreferredFileIn(Services.MediaFileIndex);

   public override IEnumerable<MediaReference> MediaReferences => [..SourceData.Audio.All, ..SourceData.Images.All, ..Audio.All, ..Images.All];

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

   /// <summary>
   /// Replaces <paramref name="oldName"/>, the old name of a vocab that was renamed, with its new name, <paramref name="newName"/>, wherever this vocab<br/>
   /// refers to it: in its related vocab and its compound parts (<see cref="Collection.VocabCollection.RenameTheReferences"/>).
   /// </summary>
   internal void RenameTheReferences(WordReference oldName, WordReference newName)
   {
      RelatedNotes.Rename(oldName, newName);
      CompoundParts.Rename(oldName, newName);
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
      var note = new VocabNote(services, question);
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
