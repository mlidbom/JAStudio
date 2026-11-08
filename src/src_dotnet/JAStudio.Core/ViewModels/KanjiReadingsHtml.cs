using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note.KanjiReadings;

namespace JAStudio.Core.ViewModels;

/// <summary>
/// A kanji's readings as its card and the pages show them, in HTML: on readings in katakana, as is the custom, the others as kept,<br/>
/// and a primary reading inside a <c>&lt;primary&gt;</c> element, which the cards' styles underline.
/// </summary>
public static class KanjiReadingsHtml
{
   /// <summary>Each of the <paramref name="readings"/> as HTML, in order.</summary>
   public static List<string> Of(KanjiReadingList readings) => [.. readings.All().Select(it => Of(it, readings.Kind))];

   /// <summary>All of the <paramref name="readings"/> as HTML, separated by commas.</summary>
   public static string Listed(KanjiReadingList readings) => string.Join(", ", Of(readings));

   static string Of(KanjiReading reading, KanjiReadingKind kind)
   {
      var shownKana = kind == KanjiReadingKind.On ? KanaUtils.HiraganaToKatakana(reading.Kana) : reading.Kana;
      return reading.IsPrimary ? $"<primary>{shownKana}</primary>" : shownKana;
   }
}
