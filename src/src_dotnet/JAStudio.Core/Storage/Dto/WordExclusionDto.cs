using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

[MemoryPackable]
public partial class WordExclusionDto
{
   public string Word { get; init; } = string.Empty;
   public int Index { get; init; } = -1;

   public WordExclusion ToExclusion() => Index == -1 ? WordExclusion.Global(Word) : WordExclusion.AtIndex(Word, Index);
}
