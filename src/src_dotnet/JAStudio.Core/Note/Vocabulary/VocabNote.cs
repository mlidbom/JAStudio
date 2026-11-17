using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Compze.Contracts;
using Compze.Teventive;
using Compze.Teventive.Taggregates;
using Compze.Teventive.TaggregateStores;
using JAStudio.Core.Anki;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Core.Note.Vocabulary.Tevents;
using JAStudio.Core.Storage.Converters;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.SysUtils;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNote : JPNote, IHasTextToSpeechAudio, ISnapshotable<VocabNote, VocabDto>, ITeventRouted
{
   static readonly TeventRouting<VocabNote> Appliers = NoteAppliers.Extend<VocabNote>(it => it
     .For<IVocabTevent.PropertyUpdated.CorpusAnswer>(static (vocab, tevent) => vocab.CorpusAnswer = tevent.CorpusAnswer)
     .For<IVocabTevent.PropertyUpdated.Explanation>(static (vocab, tevent) => vocab.Explanation = tevent.Explanation)
     .For<IVocabTevent.PropertyUpdated.ExplanationLong>(static (vocab, tevent) => vocab.ExplanationLong = tevent.ExplanationLong)
     .For<IVocabTevent.PropertyUpdated.Mnemonic>(static (vocab, tevent) => vocab.Mnemonic = tevent.Mnemonic)
     .For<IVocabTevent.PropertyUpdated.TechnicalNotes>(static (vocab, tevent) => vocab.TechnicalNotes = tevent.TechnicalNotes)
     .For<IVocabTevent.PropertyUpdated.References>(static (vocab, tevent) => vocab.References = tevent.References)
     .For<IVocabTevent.PropertyUpdated.Readings>(static (vocab, tevent) => vocab._readings = tevent.Readings)
     .For<IVocabTevent.PropertyUpdated.Audio>(static (vocab, tevent) => vocab.Audio.Apply(tevent.Audio))
     .For<IVocabTevent.PropertyUpdated.Images>(static (vocab, tevent) => vocab.Images.Apply(tevent.Images))
     .For<IVocabTevent.PropertyUpdated.PartsOfSpeech>(static (vocab, tevent) => vocab.PartsOfSpeech.Apply(tevent.PartsOfSpeech))
     .Component(static vocab => vocab.Question)
     .Component(static vocab => vocab.Forms)
     .Component(static vocab => vocab.CompoundParts)
     .Component(static vocab => vocab.SourceData)
     .Component(static vocab => vocab.RelatedNotes)
     .Component(static vocab => vocab.MetaData)
     .Component(static vocab => vocab.Register)
     .Component(static vocab => vocab.UsuallyWrittenInKana)
     .Component(static vocab => vocab.MatchingConfiguration));

   TeventRouting ITeventRouted.TeventAppliers => Appliers;

   public VocabNoteQuestion Question { get; private set; }

   /// <summary>
   /// The corpus's own answer for the vocab: the canonical one. While it's empty the source's answer is shown instead (<see cref="GetAnswer"/>).
   /// </summary>
   public string CorpusAnswer { get; private set; }

   /// <summary>The corpus's explanation of the vocab, shown on its card.</summary>
   public string Explanation { get; private set; }

   /// <summary>The corpus's longer explanation of the vocab, shown on its card.</summary>
   public string ExplanationLong { get; private set; }

   /// <summary>The corpus's own mnemonic for the vocab, shown on its card.</summary>
   public string Mnemonic { get; private set; }

   /// <summary>
   /// Gives the vocab <paramref name="answer"/>, trimmed, as its <see cref="CorpusAnswer"/>, and so its perfect synonyms the answer the vocab then<br/>
   /// shows (<see cref="Vocabulary.RelatedVocab.PerfectSynonyms.ShareTheAnswerThrough"/>). An answer the vocab has already changes nothing, which<br/>
   /// is what ends the pushing of an answer around the web of perfect synonyms.
   /// </summary>
   public void SetCorpusAnswer(string answer) => PublishIfChanged(CorpusAnswer, answer.Trim(), static it => new VocabTevent.CorpusAnswerUpdated(it));

   /// <summary>Gives the vocab <paramref name="explanation"/>, trimmed, as its <see cref="Explanation"/>.</summary>
   public void SetExplanation(string explanation) => PublishIfChanged(Explanation, explanation.Trim(), static it => new VocabTevent.ExplanationUpdated(it));

   /// <summary>Gives the vocab <paramref name="explanationLong"/>, trimmed, as its <see cref="ExplanationLong"/>.</summary>
   public void SetExplanationLong(string explanationLong) =>
      PublishIfChanged(ExplanationLong, explanationLong.Trim(), static it => new VocabTevent.ExplanationLongUpdated(it));

   /// <summary>Gives the vocab <paramref name="mnemonic"/>, trimmed, as its <see cref="Mnemonic"/>.</summary>
   public void SetMnemonic(string mnemonic) => PublishIfChanged(Mnemonic, mnemonic.Trim(), static it => new VocabTevent.MnemonicUpdated(it));

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

   /// <summary>The corpus's technical notes on the vocab, shown on its card.</summary>
   public string TechnicalNotes { get; private set; }

   /// <summary>The corpus's references for the vocab, shown on its card.</summary>
   public string References { get; private set; }

   /// <summary>Gives the vocab <paramref name="technicalNotes"/>, trimmed, as its <see cref="TechnicalNotes"/>.</summary>
   public void SetTechnicalNotes(string technicalNotes) =>
      PublishIfChanged(TechnicalNotes, technicalNotes.Trim(), static it => new VocabTevent.TechnicalNotesUpdated(it));

   /// <summary>Gives the vocab <paramref name="references"/>, trimmed, as its <see cref="References"/>.</summary>
   public void SetReferences(string references) => PublishIfChanged(References, references.Trim(), static it => new VocabTevent.ReferencesUpdated(it));

   IReadOnlyList<string> _readings;

   /// <summary>An empty vocab, which is no vocab yet: <see cref="RestoreFrom"/> makes it a stored one, and <see cref="VocabTevent.Created"/> a new one.</summary>
   internal VocabNote(NoteServices services) : base(services)
   {
      Kanji = new VocabNoteKanji(this);
      Conjugator = new VocabNoteConjugator(this);
      Sentences = new VocabNoteSentences(this);
      Cloner = new VocabCloner(this);
      BuildThePartsFrom(null);
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
      CorpusAnswer = data?.CorpusAnswer ?? string.Empty;
      Explanation = data?.Explanation ?? string.Empty;
      ExplanationLong = data?.ExplanationLong ?? string.Empty;
      Mnemonic = data?.Mnemonic ?? string.Empty;
      SourceData = new VocabSourceData(this, data?.SourceData);
      Audio = new AudioReferenceList(this, data?.Audio.Select(it => it.ToReference()) ?? [], static it => new VocabTevent.AudioUpdated(it));
      Images = new MediaReferenceList<ImageReference>(this, data?.Images.Select(it => it.ToReference()) ?? [], static it => new VocabTevent.ImagesUpdated(it));
      AllAudio = new AllAudioOfANote(SourceData.Audio, Audio);
      TechnicalNotes = data?.TechnicalNotes ?? string.Empty;
      References = data?.References ?? string.Empty;
      _readings = data?.Readings != null ? [..data.Readings] : [];

      Question = new VocabNoteQuestion(this, data);
      Forms = new VocabNoteForms(this, data);
      PartsOfSpeech = new VocabNotePartsOfSpeech(this, data);
      CompoundParts = new VocabNoteCompoundParts(this, data);
      RelatedNotes = new Vocabulary.RelatedVocab.RelatedVocab(this, data?.RelatedVocab);
      MetaData = new VocabNoteMetaData(this, data);
      Register = new VocabNoteRegister(this, data?.Register ?? []);
      UsuallyWrittenInKana = new VocabNoteUsuallyWrittenInKana(this, data?.UsuallyWrittenInKana);
      MatchingConfiguration = new VocabNoteMatchingConfiguration(this, data?.Matching, Services.SharedConditions);
   }

   public override CorpusNoteDto ToDto() => TakeSnapshot();

   internal override void Delete() => Publish(new VocabTevent.Deleted(Question.Reference));

   internal override INoteChangedInAUnitOfWork ChangedInAUnitOfWork() => new NoteChangedInAUnitOfWork.Vocab(Id);

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

   void IHasTextToSpeechAudio.Edit(Action<IHasTextToSpeechAudio> edit) => this.Edit(it => edit(it));
   ITaggregateReference<IHasTextToSpeechAudio> IHasTextToSpeechAudio.Reference() => this.Reference();

   /// <summary>The file of the vocab's <see cref="AllAudioOfANote.Preferred"/> audio in the media layer, or null when it has no audio, or the media layer hasn't its file.</summary>
   public AudioAttachment? PreferredAudio => AllAudio.PreferredFileIn(Services.MediaFileIndex);

   public override IEnumerable<MediaReference> MediaReferences => [..SourceData.Audio.All, ..SourceData.Images.All, ..Audio.All, ..Images.All];

   /// <summary>The answer shown: the <see cref="CorpusAnswer"/>, or the source's answer while the corpus has none.</summary>
   public override string GetAnswer() => CorpusAnswer is not "" ? CorpusAnswer : SourceData.Answer;

   /// <summary>
   /// Makes the source's answer the corpus's own (<see cref="CorpusAnswer"/>), written as the corpus writes a vocab's answers: in lower case, without<br/>
   /// markup, the meanings separated by slashes and the words of each joined by dashes. Only while the vocab has no answer of its own.
   /// </summary>
   public void AcceptMeaning()
   {
      Contract.State.Assert(CorpusAnswer is "", () => $"The vocab {GetQuestion()} has an answer of its own already: {CorpusAnswer}");
      SetCorpusAnswer(WrittenAsTheCorpusWritesAVocabsAnswers(SourceData.Answer));

      static string WrittenAsTheCorpusWritesAVocabsAnswers(string sourceAnswer) =>
         sourceAnswer.Replace(" SOURCE", "")
                     .Replace(", ", "/")
                     .Replace(" ", "-")
                     .ToLowerInvariant()
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

   /// <summary>Whether the vocab holds <paramref name="word"/> as a word-reference, in its related vocab or its compound parts: whether <see cref="RenameTheReferences"/> renames it.</summary>
   internal bool HoldsTheWordReference(WordReference word) => RelatedNotes.HoldsTheWord(word) || CompoundParts.HasEnteredThePart(word);

   public List<string> GetReadings() => [.. _readings];

   /// <summary>Gives the vocab <paramref name="readings"/>, in their order, as its readings.</summary>
   public void SetReadings(IReadOnlyList<string> readings) => PublishIfTheListChanged(_readings, readings, static it => new VocabTevent.ReadingsUpdated(it));

   public void GenerateAndSetAnswer()
   {
      var dictLookup = Services.DictLookup.LookupVocabWordOrName(this);
      if(dictLookup.FoundWords())
      {
         var generated = dictLookup.FormatAnswer();
         SourceData.SetAnswer(generated);
      }

      UpdateGeneratedData();
   }

   public static VocabNote Create(NoteServices services, string question, string answer, List<string> readings, List<string> forms)
   {
      var note = new VocabNote(services, question);
      note.SetCorpusAnswer(answer);
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
