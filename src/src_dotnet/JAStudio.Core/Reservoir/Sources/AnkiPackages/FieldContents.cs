using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages;

/// <summary>
/// What a field of a deck's note type holds across a sample of its notes, as shares of the values that aren't empty: how many hold Japanese,<br/>
/// Anki's furigana such as <c>漢字[かんじ]</c>, an audio file, an image, or Latin letters, as a translation does; and how long its Japanese is.<br/>
/// <see cref="DeckFieldRoles"/> recognizes the fields by them.
/// </summary>
sealed class FieldContents
{
   /// <summary>
   /// Japanese averaging fewer characters than this is most likely words, as a vocabulary deck's are, not sentences: a subs2srs deck's sentences<br/>
   /// average 10 to 25 characters.
   /// </summary>
   const double ShortestAverageLengthOfSentences = 6;

   FieldContents(string name, IReadOnlyList<string> values)
   {
      Name = name;
      JapaneseShare = ShareOf(values, it => ReservoirSentenceText.CleanedFrom(it) is not null);
      FuriganaShare = ShareOf(values, AnkiFieldMarkup.HoldsFurigana);
      SoundShare = ShareOf(values, AnkiFieldMarkup.HoldsASound);
      ImageShare = ShareOf(values, AnkiFieldMarkup.HoldsAnImage);
      LatinShare = ShareOf(values, it => it.Count(char.IsAsciiLetter) * 2 > it.Length && ReservoirSentenceText.CleanedFrom(it) is null);
      var texts = values.Select(TextOf).OfType<ReservoirSentenceText>().ToList();
      AverageTextLength = texts.Count == 0 ? 0 : texts.Average(it => it.Text.Length);
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

   /// <summary>The average length of the field's Japanese, cleaned as the reservoir keeps it (<see cref="TextOf"/>), in characters.</summary>
   public double AverageTextLength { get; }

   /// <summary>Whether the field holds Japanese, by what it holds: mostly Japanese, and seldom media.</summary>
   public bool HoldsJapanese => JapaneseShare >= 0.5 && SoundShare < 0.5 && ImageShare < 0.5;

   /// <summary>Whether the field's Japanese comes with furigana, as a sentence's reading does, or a sentence only some decks give with readings.</summary>
   public bool HoldsFurigana => FuriganaShare >= 0.3;

   /// <summary>Whether the field's Japanese is most likely words, not sentences: it averages fewer than <see cref="ShortestAverageLengthOfSentences"/> characters.</summary>
   public bool HoldsWordsRatherThanSentences => AverageTextLength < ShortestAverageLengthOfSentences;

   /// <summary>Whether the field holds translations, by what it holds: mostly Latin letters, and seldom media.</summary>
   public bool HoldsTranslations => LatinShare >= 0.5 && SoundShare < 0.5 && ImageShare < 0.5;

   /// <summary>The Japanese text <paramref name="value"/>, a value of the field, holds, cleaned as the reservoir keeps it, its furigana gone when the field holds them.</summary>
   public ReservoirSentenceText? TextOf(string value) => ReservoirSentenceText.CleanedFrom(HoldsFurigana ? AnkiFieldMarkup.TextWithoutFurigana(value) : value);

   static double ShareOf(IReadOnlyList<string> values, Func<string, bool> holds) => values.Count == 0 ? 0 : values.Count(holds) / (double)values.Count;
}
