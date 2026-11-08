using System.Collections.Generic;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.KanjiReadings;

/// <summary>The readings of a <see cref="KanjiNote"/>: its <see cref="On"/>, <see cref="Kun"/> and <see cref="Nanori"/> readings, each primary or secondary.</summary>
public class KanjiNoteReadings
{
   internal KanjiNoteReadings(KanjiReadingsDto? data, NoteGuard guard)
   {
      On = new KanjiReadingList(KanjiReadingKind.On, data?.On ?? [], guard);
      Kun = new KanjiReadingList(KanjiReadingKind.Kun, data?.Kun ?? [], guard);
      Nanori = new KanjiReadingList(KanjiReadingKind.Nanori, data?.Nanori ?? [], guard);
   }

   /// <summary>The on readings: see <see cref="KanjiReadingKind.On"/>.</summary>
   public KanjiReadingList On { get; }

   /// <summary>The kun readings: see <see cref="KanjiReadingKind.Kun"/>.</summary>
   public KanjiReadingList Kun { get; }

   /// <summary>The nanori readings: see <see cref="KanjiReadingKind.Nanori"/>.</summary>
   public KanjiReadingList Nanori { get; }

   /// <summary>The kana of every reading: the on readings, then the kun readings, then the nanori readings.</summary>
   public List<string> AllKana() => [.. On.AllKana(), .. Kun.AllKana(), .. Nanori.AllKana()];

   /// <summary>The kana of the primary readings: the on readings', then the kun readings', then the nanori readings'.</summary>
   public List<string> PrimaryKana() => [.. On.PrimaryKana(), .. Kun.PrimaryKana(), .. Nanori.PrimaryKana()];

   internal KanjiReadingsDto ToDto() => new() { On = On.ToDto(), Kun = Kun.ToDto(), Nanori = Nanori.ToDto() };
}
