using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.KanjiReadings;

/// <summary>
/// A kanji's readings of one <see cref="KanjiReadingKind"/>, in order, each primary or secondary (<see cref="KanjiReading"/>).<br/>
/// Every reading has kana, and no two have the same. A change publishes the list's tevent with the whole new list, which the kanji's readings apply<br/>
/// (<see cref="Apply"/>).
/// </summary>
/// <remarks>
/// On readings are kept in hiragana, however they're typed, so that they compare with the readings of vocab, which are in hiragana.<br/>
/// So every method takes an on reading in hiragana or katakana. Other readings are kept as typed: a few are katakana, such as 頁's kun reading ページ.
/// </remarks>
public class KanjiReadingList
{
   readonly KanjiNote _kanji;
   readonly Func<IReadOnlyList<KanjiReading>, NoteTevent> _teventOf;
   IReadOnlyList<KanjiReading> _readings;

   /// <param name="kanji">The kanji they're the readings of, which publishes their tevent.</param>
   /// <param name="kind">The <see cref="Kind"/>.</param>
   /// <param name="data">The readings as the kanji stores them.</param>
   /// <param name="teventOf">The list's tevent: that its readings are now those it's given.</param>
   internal KanjiReadingList(KanjiNote kanji, KanjiReadingKind kind, IEnumerable<KanjiReadingDto> data, Func<IReadOnlyList<KanjiReading>, NoteTevent> teventOf)
   {
      _kanji = kanji;
      Kind = kind;
      _teventOf = teventOf;
      _readings = [.. data.Select(it => new KanjiReading(it.Kana, it.IsPrimary))];
      Contract.Argument.Assert(EveryReadingHasKana, () => $"The {Kind} readings {Listed()} include one with no kana.");
      Contract.Argument.Assert(EveryReadingIsKeptAsItShouldBe, () => $"The {Kind} readings {Listed()} aren't all kept as they should be: an on reading in hiragana.");
      Contract.Argument.Assert(NoTwoReadingsHaveTheSameKana, () => $"The {Kind} readings {Listed()} have the same kana more than once.");
   }

   public KanjiReadingKind Kind { get; }

   /// <summary>The readings, in order, each with whether it's primary.</summary>
   public List<KanjiReading> All() => [.. _readings];

   /// <summary>The kana of every reading, in order.</summary>
   public List<string> AllKana() => [.. _readings.Select(it => it.Kana)];

   /// <summary>The kana of the primary readings, in order.</summary>
   public List<string> PrimaryKana() => [.. _readings.Where(it => it.IsPrimary).Select(it => it.Kana)];

   /// <summary>Whether <paramref name="kana"/> is one of the readings.</summary>
   public bool Contains(string kana) => IndexOf(kana) >= 0;

   /// <summary>Whether <paramref name="kana"/> is one of the primary readings.</summary>
   public bool IsPrimary(string kana) => PrimaryKana().Contains(AsKept(kana));

   /// <summary>Adds <paramref name="kana"/> last, as a secondary reading. Adding a reading the list has already changes nothing.</summary>
   public void Add(string kana)
   {
      Contract.Argument.Assert(!string.IsNullOrWhiteSpace(kana), () => $"A {Kind} reading must have kana.");
      if(Contains(kana)) return;

      SetTheReadings([.. _readings, new KanjiReading(AsKept(kana), IsPrimary: false)]);
   }

   /// <summary>Removes the reading <paramref name="kana"/>.</summary>
   public void Remove(string kana)
   {
      List<KanjiReading> readings = [.. _readings];
      readings.RemoveAt(IndexOfTheReading(kana));
      SetTheReadings(readings);
   }

   /// <summary>Makes the reading <paramref name="kana"/> primary.</summary>
   public void MakePrimary(string kana) => SetWhetherPrimary(kana, isPrimary: true);

   /// <summary>Makes the reading <paramref name="kana"/> secondary, that is no longer primary.</summary>
   public void MakeSecondary(string kana) => SetWhetherPrimary(kana, isPrimary: false);

   /// <summary>
   /// Makes <paramref name="readings"/> the readings, in their order, each primary or secondary as it says, an on reading kept in hiragana however it's typed.<br/>
   /// Each must have kana, and no two the same.
   /// </summary>
   public void ReplaceWith(IReadOnlyList<KanjiReading> readings)
   {
      var kept = readings.Select(it => it with { Kana = AsKept(it.Kana) }).ToList();
      Contract.Argument.Assert(kept.All(it => !string.IsNullOrWhiteSpace(it.Kana)), () => $"A {Kind} reading must have kana.");
      Contract.Argument.Assert(kept.DistinctBy(it => it.Kana).Count() == kept.Count, () => $"The {Kind} readings {string.Join(", ", kept.Select(it => it.Kana))} have the same kana more than once.");

      SetTheReadings(kept);
   }

   /// <summary>Makes <paramref name="readings"/> the readings: what the list's tevent's applier does.</summary>
   internal void Apply(IReadOnlyList<KanjiReading> readings) => _readings = readings;

   internal List<KanjiReadingDto> ToDto() => [.. _readings.Select(it => new KanjiReadingDto { Kana = it.Kana, IsPrimary = it.IsPrimary })];

   void SetWhetherPrimary(string kana, bool isPrimary)
   {
      var index = IndexOfTheReading(kana);
      List<KanjiReading> readings = [.. _readings];
      readings[index] = readings[index] with { IsPrimary = isPrimary };
      SetTheReadings(readings);
   }

   void SetTheReadings(IReadOnlyList<KanjiReading> readings) => _kanji.PublishIfTheListChanged(_readings, readings, _teventOf);

   /// <summary>The index of the reading <paramref name="kana"/>, which must be one of the readings.</summary>
   int IndexOfTheReading(string kana)
   {
      var index = IndexOf(kana);
      Contract.Argument.Assert(index >= 0, () => $"{kana} isn't one of the {Kind} readings {Listed()}.");
      return index;
   }

   int IndexOf(string kana) => AllKana().IndexOf(AsKept(kana));

   /// <summary><paramref name="kana"/> as the list keeps it: an on reading in hiragana, any other as it is.</summary>
   string AsKept(string kana) => Kind == KanjiReadingKind.On ? KanaUtils.KatakanaToHiragana(kana) : kana;

   bool EveryReadingHasKana => _readings.All(it => !string.IsNullOrWhiteSpace(it.Kana));

   bool EveryReadingIsKeptAsItShouldBe => _readings.All(it => it.Kana == AsKept(it.Kana));

   bool NoTwoReadingsHaveTheSameKana => _readings.DistinctBy(it => it.Kana).Count() == _readings.Count;

   string Listed() => string.Join(", ", AllKana());
}
