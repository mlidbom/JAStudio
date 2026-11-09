using System.Collections.Generic;
using JAStudio.Core.Note.KanjiReadings;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>A kanji's readings of each kind, in order, each with whether it's primary, as <see cref="KanjiReadingsChange"/> reports them.</summary>
public record KanjiReadingsReported(IReadOnlyList<KanjiReading> On, IReadOnlyList<KanjiReading> Kun, IReadOnlyList<KanjiReading> Nanori)
{
   public static KanjiReadingsReported Of(KanjiNoteReadings readings) => new(readings.On.All(), readings.Kun.All(), readings.Nanori.All());
}
