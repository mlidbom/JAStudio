using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages;

/// <summary>
/// What a field of a deck's note type holds across a sample of its notes, as shares of the values that aren't empty: how many hold Japanese,<br/>
/// Anki's furigana such as <c>漢字[かんじ]</c>, an audio file, an image, or Latin letters, as a translation does. <see cref="DeckFieldRoles"/><br/>
/// recognizes the fields by them.
/// </summary>
sealed partial class FieldContents
{
   FieldContents(string name, IReadOnlyList<string> values)
   {
      Name = name;
      JapaneseShare = ShareOf(values, it => ReservoirSentenceText.CleanedFrom(it) is not null);
      FuriganaShare = ShareOf(values, it => AnkiFurigana().IsMatch(it));
      SoundShare = ShareOf(values, it => DeckFieldRoles.SoundFile().IsMatch(it));
      ImageShare = ShareOf(values, it => DeckFieldRoles.ImageFile().IsMatch(it));
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

   static double ShareOf(IReadOnlyList<string> values, System.Func<string, bool> holds) => values.Count == 0 ? 0 : values.Count(holds) / (double)values.Count;

   [GeneratedRegex(@"\S\[[^\]\s]+\]")] private static partial Regex AnkiFurigana();
}
