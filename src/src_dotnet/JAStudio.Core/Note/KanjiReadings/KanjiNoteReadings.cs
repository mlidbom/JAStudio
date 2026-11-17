using System;
using System.Collections.Generic;
using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using JAStudio.Core.Note.KanjiTevents;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.KanjiReadings;

/// <summary>The readings of a <see cref="KanjiNote"/>: its <see cref="On"/>, <see cref="Kun"/> and <see cref="Nanori"/> readings, each primary or secondary.</summary>
/// <remarks>A component of its kanji: each kind of reading publishes a tevent of its own, <see cref="IKanjiTevent.Readings"/>, which its appliers apply.</remarks>
public class KanjiNoteReadings : TeventiveComponent<KanjiNote, INoteTevent, NoteTevent, KanjiNoteReadings, IKanjiTevent.Readings, KanjiTevent.Readings>, ITeventRouted
{
   static readonly TeventRouting<KanjiNoteReadings, IKanjiTevent.Readings> Appliers = DeclareAppliers(it => it
     .For<IKanjiTevent.Readings.PropertyUpdated.On>(static (readings, tevent) => readings.On.Apply(tevent.On))
     .For<IKanjiTevent.Readings.PropertyUpdated.Kun>(static (readings, tevent) => readings.Kun.Apply(tevent.Kun))
     .For<IKanjiTevent.Readings.PropertyUpdated.Nanori>(static (readings, tevent) => readings.Nanori.Apply(tevent.Nanori)));

   TeventRouting ITeventRouted.TeventAppliers => Appliers;

   /// <param name="kanji">The kanji they're the readings of.</param>
   /// <param name="data">The readings as the kanji stores them, or null for a new kanji.</param>
   internal KanjiNoteReadings(KanjiNote kanji, KanjiReadingsDto? data) : base(kanji)
   {
      On = new KanjiReadingList(kanji, KanjiReadingKind.On, data?.On ?? [], static it => new KanjiTevent.Readings.OnUpdated(it));
      Kun = new KanjiReadingList(kanji, KanjiReadingKind.Kun, data?.Kun ?? [], static it => new KanjiTevent.Readings.KunUpdated(it));
      Nanori = new KanjiReadingList(kanji, KanjiReadingKind.Nanori, data?.Nanori ?? [], static it => new KanjiTevent.Readings.NanoriUpdated(it));
   }

   /// <summary>The on readings: see <see cref="KanjiReadingKind.On"/>.</summary>
   public KanjiReadingList On { get; }

   /// <summary>The kun readings: see <see cref="KanjiReadingKind.Kun"/>.</summary>
   public KanjiReadingList Kun { get; }

   /// <summary>The nanori readings: see <see cref="KanjiReadingKind.Nanori"/>.</summary>
   public KanjiReadingList Nanori { get; }

   /// <summary>The readings of the kind <paramref name="kind"/>: <see cref="On"/>, <see cref="Kun"/> or <see cref="Nanori"/>.</summary>
   public KanjiReadingList OfKind(KanjiReadingKind kind) => kind switch
   {
      KanjiReadingKind.On => On,
      KanjiReadingKind.Kun => Kun,
      KanjiReadingKind.Nanori => Nanori,
      _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "There is no such kind of kanji reading.")
   };

   /// <summary>The kana of every reading: the on readings, then the kun readings, then the nanori readings.</summary>
   public List<string> AllKana() => [.. On.AllKana(), .. Kun.AllKana(), .. Nanori.AllKana()];

   /// <summary>The kana of the primary readings: the on readings', then the kun readings', then the nanori readings'.</summary>
   public List<string> PrimaryKana() => [.. On.PrimaryKana(), .. Kun.PrimaryKana(), .. Nanori.PrimaryKana()];

   internal KanjiReadingsDto ToDto() => new() { On = On.ToDto(), Kun = Kun.ToDto(), Nanori = Nanori.ToDto() };
}
