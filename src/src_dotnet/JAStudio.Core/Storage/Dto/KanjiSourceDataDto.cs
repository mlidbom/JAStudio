using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A kanji's <see cref="Note.KanjiSourceData"/> as the corpus stores it.</summary>
[MemoryPackable]
public partial class KanjiSourceDataDto
{
   public string Answer { get; init; } = string.Empty;
   public string MeaningMnemonic { get; init; } = string.Empty;
   public string ReadingMnemonic { get; init; } = string.Empty;
   public string MeaningInfo { get; init; } = string.Empty;
   public string ReadingInfo { get; init; } = string.Empty;
}
