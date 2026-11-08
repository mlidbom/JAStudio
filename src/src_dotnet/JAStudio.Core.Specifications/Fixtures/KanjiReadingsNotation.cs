using System;
using System.Linq;
using JAStudio.Core.Note.KanjiReadings;

namespace JAStudio.Core.Specifications.Fixtures;

/// <summary>
/// A kanji's readings of one kind written as text, the way specifications give them: separated by commas, each primary reading<br/>
/// inside <c>&lt;primary&gt;</c> tags, as in <c>"&lt;primary&gt;カン&lt;/primary&gt;, ケン"</c>. The corpus stored readings this way before they were a <see cref="KanjiReadingList"/>.
/// </summary>
static class KanjiReadingsNotation
{
   const string PrimaryStart = "<primary>";
   const string PrimaryEnd = "</primary>";

   /// <summary>Adds the readings <paramref name="notation"/> writes to <paramref name="readings"/>, in order, making the primary ones primary.</summary>
   public static void AddTo(KanjiReadingList readings, string notation)
   {
      foreach(var written in notation.Split([',', '、'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
      {
         var kana = written.Replace(PrimaryStart, "").Replace(PrimaryEnd, "");
         readings.Add(kana);
         if(written.StartsWith(PrimaryStart)) readings.MakePrimary(kana);
      }
   }

   /// <summary><paramref name="readings"/> written in the notation, separated by <c>", "</c>.</summary>
   public static string Of(KanjiReadingList readings) =>
      string.Join(", ", readings.All().Select(it => it.IsPrimary ? $"{PrimaryStart}{it.Kana}{PrimaryEnd}" : it.Kana));
}
