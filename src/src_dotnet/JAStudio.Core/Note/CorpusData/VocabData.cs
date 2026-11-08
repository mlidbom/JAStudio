using System.Collections.Generic;
using JAStudio.Core.Anki;
using MemoryPack;

namespace JAStudio.Core.Note.CorpusData;

[MemoryPackable]
public partial class VocabData : CorpusObjectData
{
   public string Question { get; init; } = string.Empty;
   public string SourceAnswer { get; init; } = string.Empty;
   public string UserAnswer { get; init; } = string.Empty;
   public string ActiveAnswer { get; init; } = string.Empty;
   public string UserExplanation { get; init; } = string.Empty;
   public string UserExplanationLong { get; init; } = string.Empty;
   public string UserMnemonic { get; init; } = string.Empty;
   public VocabCompoundPartsSubData CompoundParts { get; init; } = new();
   public List<string> Readings { get; init; } = [];
   public string PartsOfSpeech { get; init; } = string.Empty;
   public string SourceMnemonic { get; init; } = string.Empty;
   public string SourceReadingMnemonic { get; init; } = string.Empty;
   public string AudioB { get; init; } = string.Empty;
   public string AudioG { get; init; } = string.Empty;
   public string AudioTTS { get; init; } = string.Empty;
   public List<EnteredFormSubData> Forms { get; init; } = [];
   public int SentenceCount { get; init; }
   public string TechnicalNotes { get; init; } = string.Empty;
   public string References { get; init; } = string.Empty;
   public string Image { get; init; } = string.Empty;
   public string UserImage { get; init; } = string.Empty;
   public VocabMatchingRulesSubData MatchingRules { get; init; } = new();
   public VocabRelatedSubData RelatedVocab { get; init; } = new();

   protected override NoteId CreateTypedId() => new VocabId(Id);

   protected override void PopulateAnkiNoteFields(Dictionary<string, string> fields)
   {
      fields[AnkiFieldNames.Question] = Question;
      fields[AnkiFieldNames.Vocab.AudioB] = AudioB;
      fields[AnkiFieldNames.Vocab.AudioG] = AudioG;
      fields[AnkiFieldNames.Vocab.AudioTTS] = AudioTTS;
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
