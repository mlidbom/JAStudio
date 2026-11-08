using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

[MemoryPackable]
public partial class WordExclusionDto
{
   public string Word { get; init; } = string.Empty;
   public int Index { get; init; } = -1;
}
