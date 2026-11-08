using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A <see cref="Note.Vocabulary.VocabNote"/> as the corpus stores it.</summary>
[MemoryPackable]
public partial class VocabDto : CorpusNoteDto
{
   public VocabQuestionDto Question { get; init; } = new();
   public string CorpusAnswer { get; init; } = string.Empty;
   public string Explanation { get; init; } = string.Empty;
   public string ExplanationLong { get; init; } = string.Empty;
   public string Mnemonic { get; init; } = string.Empty;
   public VocabCompoundPartsDto CompoundParts { get; init; } = new();
   public List<string> Readings { get; init; } = [];
   public string PartsOfSpeech { get; init; } = string.Empty;
   public string AudioB { get; init; } = string.Empty;
   public string AudioG { get; init; } = string.Empty;
   public string AudioTTS { get; init; } = string.Empty;
   public List<EnteredFormDto> Forms { get; init; } = [];
   public int SentenceCount { get; init; }
   public string TechnicalNotes { get; init; } = string.Empty;
   public string References { get; init; } = string.Empty;
   public string Image { get; init; } = string.Empty;
   public string UserImage { get; init; } = string.Empty;
   public VocabMatchingRulesDto MatchingRules { get; init; } = new();
   public VocabRelatedDto RelatedVocab { get; init; } = new();
   public VocabSourceDataDto SourceData { get; init; } = new();
}
