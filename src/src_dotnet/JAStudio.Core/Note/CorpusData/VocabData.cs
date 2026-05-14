using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.NoteFields.AutoSaveWrappers;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.RelatedVocab;
using MemoryPack;

namespace JAStudio.Core.Note.CorpusData;

[MemoryPackable]
public partial class VocabData : CorpusObjectData
{
   static readonly VocabNoteMatchingRulesSerializer MatchingRulesSerializer = new();
   static readonly IObjectSerializer<RelatedVocabData> RelatedVocabSerializer = RelatedVocabData.Serializer();

   public string Question { get; init; } = string.Empty;
   public string SourceAnswer { get; init; } = string.Empty;
   public string UserAnswer { get; init; } = string.Empty;
   public string ActiveAnswer { get; init; } = string.Empty;
   public string UserExplanation { get; init; } = string.Empty;
   public string UserExplanationLong { get; init; } = string.Empty;
   public string UserMnemonic { get; init; } = string.Empty;
   public List<string> UserCompounds { get; init; } = [];
   public List<string> Readings { get; init; } = [];
   public string PartsOfSpeech { get; init; } = string.Empty;
   public string SourceMnemonic { get; init; } = string.Empty;
   public string SourceReadingMnemonic { get; init; } = string.Empty;
   public string AudioB { get; init; } = string.Empty;
   public string AudioG { get; init; } = string.Empty;
   public string AudioTTS { get; init; } = string.Empty;
   public List<string> Forms { get; init; } = [];
   public int SentenceCount { get; init; }
   public string TechnicalNotes { get; init; } = string.Empty;
   public string References { get; init; } = string.Empty;
   public string Image { get; init; } = string.Empty;
   public string UserImage { get; init; } = string.Empty;
   public VocabMatchingRulesSubData MatchingRules { get; init; } = new();
   public VocabRelatedSubData RelatedVocab { get; init; } = new();

   protected override NoteId CreateTypedId() => new VocabId(Id);

   protected override void PopulateFields(Dictionary<string, string> fields)
   {
      fields[NoteFieldsConstants.Vocab.Question] = Question;
      fields[NoteFieldsConstants.Vocab.SourceAnswer] = SourceAnswer;
      fields[NoteFieldsConstants.Vocab.UserAnswer] = UserAnswer;
      fields[NoteFieldsConstants.Vocab.ActiveAnswer] = ActiveAnswer;
      fields[NoteFieldsConstants.Vocab.UserExplanation] = UserExplanation;
      fields[NoteFieldsConstants.Vocab.UserExplanationLong] = UserExplanationLong;
      fields[NoteFieldsConstants.Vocab.UserMnemonic] = UserMnemonic;
      fields[NoteFieldsConstants.Vocab.UserCompounds] = string.Join(", ", UserCompounds);
      fields[NoteFieldsConstants.Vocab.Reading] = string.Join(", ", Readings);
      fields[NoteFieldsConstants.Vocab.PartsOfSpeech] = PartsOfSpeech;
      fields[NoteFieldsConstants.Vocab.SourceMnemonic] = SourceMnemonic;
      fields[NoteFieldsConstants.Vocab.SourceReadingMnemonic] = SourceReadingMnemonic;
      fields[NoteFieldsConstants.Vocab.AudioB] = AudioB;
      fields[NoteFieldsConstants.Vocab.AudioG] = AudioG;
      fields[NoteFieldsConstants.Vocab.AudioTTS] = AudioTTS;
      fields[NoteFieldsConstants.Vocab.Forms] = string.Join(", ", Forms);
      fields[NoteFieldsConstants.Vocab.SentenceCount] = SentenceCount.ToString();
      fields[NoteFieldsConstants.Vocab.TechnicalNotes] = TechnicalNotes;
      fields[NoteFieldsConstants.Vocab.References] = References;
      fields[NoteFieldsConstants.Vocab.Image] = Image;
      fields[NoteFieldsConstants.Vocab.UserImage] = UserImage;
      fields[NoteFieldsConstants.Vocab.MatchingRules] = SerializeMatchingRules();
      fields[NoteFieldsConstants.Vocab.RelatedVocab] = SerializeRelatedVocab();
   }

   string SerializeMatchingRules() =>
      MatchingRulesSerializer.Serialize(new VocabNoteMatchingRulesData(
                                           MatchingRules.SurfaceIsNot.ToHashSet(),
                                           MatchingRules.PrefixIsNot.ToHashSet(),
                                           MatchingRules.SuffixIsNot.ToHashSet(),
                                           MatchingRules.RequiredPrefix.ToHashSet(),
                                           MatchingRules.YieldToSurface.ToHashSet()));

   string SerializeRelatedVocab() =>
      RelatedVocabSerializer.Serialize(new RelatedVocabData(
                                          RelatedVocab.ErgativeTwin,
                                          new ValueWrapper<string>(RelatedVocab.DerivedFrom),
                                          RelatedVocab.PerfectSynonyms.ToHashSet(),
                                          RelatedVocab.Synonyms.ToHashSet(),
                                          RelatedVocab.Antonyms.ToHashSet(),
                                          RelatedVocab.ConfusedWith.ToHashSet(),
                                          RelatedVocab.SeeAlso.ToHashSet()));

   /// Creates VocabData from raw Anki NoteData (for NoteCache and Python interop paths).
   public static VocabData FromAnkiNoteData(NoteData data) => FromAnki(new AnkiVocabNote(data));

   public static VocabData FromAnki(AnkiVocabNote anki) =>
      new()
      {
         Id = (anki.Id ?? VocabId.New()).Value,
         Tags = [..anki.Tags],
         Question = anki.Question,
         ActiveAnswer = anki.ActiveAnswer,
         AudioB = anki.AudioB,
         AudioG = anki.AudioG,
         AudioTTS = anki.AudioTTS,
         Image = anki.Image,
         UserImage = anki.UserImage,
      };

   /// Merges Anki-owned fields (every field exposed by <see cref="AnkiVocabNote"/>, plus tags) into existing data, preserving all fields Anki does not store.
   public VocabData MergeAnkiData(NoteData ankiData)
   {
      var anki = new AnkiVocabNote(ankiData);
      return new VocabData
             {
                Id = Id,
                Tags = [..anki.Tags],
                Question = anki.Question,
                SourceAnswer = SourceAnswer,
                UserAnswer = UserAnswer,
                ActiveAnswer = anki.ActiveAnswer,
                UserExplanation = UserExplanation,
                UserExplanationLong = UserExplanationLong,
                UserMnemonic = UserMnemonic,
                UserCompounds = UserCompounds,
                Readings = Readings,
                PartsOfSpeech = PartsOfSpeech,
                SourceMnemonic = SourceMnemonic,
                SourceReadingMnemonic = SourceReadingMnemonic,
                AudioB = anki.AudioB,
                AudioG = anki.AudioG,
                AudioTTS = anki.AudioTTS,
                Forms = Forms,
                SentenceCount = SentenceCount,
                TechnicalNotes = TechnicalNotes,
                References = References,
                Image = anki.Image,
                UserImage = anki.UserImage,
                MatchingRules = MatchingRules,
                RelatedVocab = RelatedVocab,
             };
   }
}

/// Matching rules sub-data for vocab word matching (serialized in JSON).
[MemoryPackable]
public partial class VocabMatchingRulesSubData
{
   public List<string> PrefixIsNot { get; init; } = [];
   public List<string> SuffixIsNot { get; init; } = [];
   public List<string> SurfaceIsNot { get; init; } = [];
   public List<string> YieldToSurface { get; init; } = [];
   public List<string> RequiredPrefix { get; init; } = [];
}

/// Related vocab sub-data (serialized in JSON).
[MemoryPackable]
public partial class VocabRelatedSubData
{
   public string ErgativeTwin { get; init; } = string.Empty;
   public string DerivedFrom { get; init; } = string.Empty;
   public List<string> PerfectSynonyms { get; init; } = [];
   public List<string> Synonyms { get; init; } = [];
   public List<string> Antonyms { get; init; } = [];
   public List<string> ConfusedWith { get; init; } = [];
   public List<string> SeeAlso { get; init; } = [];
}
