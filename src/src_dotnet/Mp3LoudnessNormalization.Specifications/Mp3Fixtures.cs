using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Mp3LoudnessNormalization.Specifications;

/// <summary>
/// The MP3 files in the <c>Mp3Fixtures</c> folder, made by <c>dev_scripts/generate-mp3-fixtures.cs</c>, and what mp3gain 1.4.6 made of them:<br/>
/// the ReplayGain it measured (<c>/o /s s</c>), in <c>mp3gain-measurements.tsv</c>, and each file normalized by it (<c>/r /c /s s</c>),<br/>
/// as <c>name.normalized-by-mp3gain.mp3</c>.
/// </summary>
static class Mp3Fixtures
{
   const string NormalizedSuffix = ".normalized-by-mp3gain.mp3";

   static string Folder => Path.Combine(AppContext.BaseDirectory, "Mp3Fixtures");

   /// <summary>The fixtures' names, such as <c>azure-nanami-24khz</c>.</summary>
   public static IEnumerable<string> Names() =>
      Directory.GetFiles(Folder, "*.mp3")
               .Select(Path.GetFileName)
               .OfType<string>()
               .Where(it => !it.EndsWith(NormalizedSuffix, StringComparison.Ordinal))
               .Select(Path.GetFileNameWithoutExtension)
               .OfType<string>()
               .Order(StringComparer.Ordinal);

   /// <summary>The fixture named <paramref name="name"/>, as it was made.</summary>
   public static byte[] Original(string name) => File.ReadAllBytes(Path.Combine(Folder, name + ".mp3"));

   /// <summary>The fixture named <paramref name="name"/>, as mp3gain normalized it.</summary>
   public static byte[] NormalizedByMp3Gain(string name) => File.ReadAllBytes(Path.Combine(Folder, name + NormalizedSuffix));

   /// <summary>The ReplayGain mp3gain measured for the fixture named <paramref name="name"/>, in decibels, to the hundredth it measures to.</summary>
   public static double ReplayGainMp3GainMeasured(string name) =>
      File.ReadAllLines(Path.Combine(Folder, "mp3gain-measurements.tsv"))
          .Select(line => line.Split('\t'))
          .Where(columns => columns[0] == name + ".mp3")
          .Select(columns => double.Parse(columns[2], CultureInfo.InvariantCulture))
          .Single();
}
