using System.Collections.Generic;
using JAStudio.Core.Note.KanjiReadings;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A kanji's <see cref="KanjiNoteReadings"/> as the corpus stores them, in its <see cref="KanjiDto.Readings"/>.</summary>
[MemoryPackable]
public partial class KanjiReadingsDto
{
   public List<KanjiReadingDto> On { get; init; } = [];
   public List<KanjiReadingDto> Kun { get; init; } = [];
   public List<KanjiReadingDto> Nanori { get; init; } = [];
}
