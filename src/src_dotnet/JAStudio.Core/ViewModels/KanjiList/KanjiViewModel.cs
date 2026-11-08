using System.Linq;
using JAStudio.Core.Note;

namespace JAStudio.Core.ViewModels.KanjiList;

public class KanjiViewModel
{
   public KanjiNote Kanji { get; }

   public KanjiViewModel(KanjiNote kanji) => Kanji = kanji;

   public string Question() => Kanji.GetQuestion();

   public string Answer() => Kanji.GetAnswer();

   public string Readings()
   {
      var readings = $"{KanjiReadingsHtml.Listed(Kanji.Readings.On)} <span class=\"readingsSeparator\">|</span> {KanjiReadingsHtml.Listed(Kanji.Readings.Kun)}";
      if(Kanji.Readings.Nanori.All().Any())
      {
         readings += $" <span class=\"readingsSeparator\">|</span> {KanjiReadingsHtml.Listed(Kanji.Readings.Nanori)}";
      }

      return readings;
   }

   public string Mnemonic() => Kanji.ShownMnemonic;

   public override string ToString() => $"{Question()}      {StringExtensions.PadToLength(Answer(), 60)}: {Readings()}";
}
