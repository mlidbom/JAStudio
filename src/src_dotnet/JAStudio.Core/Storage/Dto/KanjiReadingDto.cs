using JAStudio.Core.Note.KanjiReadings;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A <see cref="KanjiReading"/> as the corpus stores it, in a kanji's <see cref="KanjiReadingsDto"/>.</summary>
[MemoryPackable]
public partial class KanjiReadingDto
{
   public string Kana { get; init; } = string.Empty;
   public bool IsPrimary { get; init; }
}
