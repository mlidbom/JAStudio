using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages;

/// <summary>
/// What a field of a deck's note type holds across a sample of its notes, as shares of the values that aren't empty: how many hold Japanese,<br/>
/// Anki's furigana such as <c>漢字[かんじ]</c>, an audio file, an image, or Latin letters, as a translation does. <see cref="DeckFieldRoles"/><br/>
/// recognizes the fields by them.
/// </summary>
sealed class FieldContents
{
   FieldContents(string name, IReadOnlyList<string> values)
   {
      Name = name;
      JapaneseShare = ShareOf(values, it => ReservoirSentenceText.CleanedFrom(it) is not null);
      FuriganaShare = ShareOf(values, AnkiFieldMarkup.HoldsFurigana);
      SoundShare = ShareOf(values, AnkiFieldMarkup.HoldsASound);
      ImageShare = ShareOf(values, AnkiFieldMarkup.HoldsAnImage);
      LatinShare = ShareOf(values, it => it.Count(char.IsAsciiLetter) * 2 > it.Length && ReservoirSentenceText.CleanedFrom(it) is null);
      AverageLength = values.Count == 0 ? 0 : values.Average(it => it.Length);
   }

   /// <summary>What <paramref name="name"/> holds in <paramref name="sample"/>, notes of the note type the field is of.</summary>
   public static FieldContents Of(string name, IReadOnlyList<RawNote> sample) =>
      new(name, [.. sample.Select(it => it.Field(name)).Where(it => it.Trim().Length > 0)]);

   public string Name { get; }
   public double JapaneseShare { get; }
   public double FuriganaShare { get; }
   public double SoundShare { get; }
   public double ImageShare { get; }
   public double LatinShare { get; }
   public double AverageLength { get; }

   /// <summary>Whether the field holds Japanese sentences, by what it holds: mostly Japanese, and seldom media or furigana, as a word's reading has.</summary>
   public bool HoldsJapaneseSentences => JapaneseShare >= 0.5 && SoundShare < 0.5 && ImageShare < 0.5 && FuriganaShare < 0.3;

   /// <summary>Whether the field holds translations, by what it holds: mostly Latin letters, and seldom media.</summary>
   public bool HoldsTranslations => LatinShare >= 0.5 && SoundShare < 0.5 && ImageShare < 0.5;

   static double ShareOf(IReadOnlyList<string> values, Func<string, bool> holds) => values.Count == 0 ? 0 : values.Count(holds) / (double)values.Count;
}
