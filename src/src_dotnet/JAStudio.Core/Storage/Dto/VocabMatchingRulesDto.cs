using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A vocab's configurable matching rules (<see cref="Note.Vocabulary.VocabNoteMatchingRules"/>) as the corpus stores them.</summary>
[MemoryPackable]
public partial class VocabMatchingRulesDto
{
   public List<string> PrefixIsNot { get; init; } = [];
   public List<string> SuffixIsNot { get; init; } = [];
   public List<string> SurfaceIsNot { get; init; } = [];
   public List<string> YieldToSurface { get; init; } = [];
   public List<string> RequiredPrefix { get; init; } = [];
}
