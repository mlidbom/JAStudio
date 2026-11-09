using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.LanguageServices.JamdictEx;

public class DictLookupResult
{
   static DictLookupResult? _failed;

   public string Word { get; }
   public List<string> LookupReading { get; }
   public List<DictEntry> Entries { get; }

   public DictLookupResult(List<DictEntry> entries, string lookupWord, List<string> lookupReading)
   {
      Word = lookupWord;
      LookupReading = lookupReading;
      Entries = entries;
   }

   public int FoundWordsCount() => Entries.Count;
   public bool FoundWords() => Entries.Count > 0;

   public bool IsUk() => Entries.Any(ent => ent.IsKanaOnly());

   public HashSet<string> ValidForms(bool forceAllowKanaOnly = false)
   {
      return Entries.SelectMany(entry => entry.ValidForms(forceAllowKanaOnly)).ToHashSet();
   }

   public List<string> Readings()
   {
      return Entries.SelectMany(entry => entry.KanaFormsText()).Distinct().ToList();
   }

   public HashSet<PartOfSpeech> PartsOfSpeech()
   {
      return Entries.SelectMany(entry => entry.PartsOfSpeech()).ToHashSet();
   }

   public PrioritySpec PrioritySpec()
   {
      var kanaTags = Entries
                    .SelectMany(entry => entry.KanaForms)
                    .Where(it => it.Text == Word)
                    .SelectMany(it => it.Priorities)
                    .ToHashSet();

      var kanjiTags = Entries
                     .SelectMany(entry => entry.KanjiForms)
                     .Where(it => it.Text == Word)
                     .SelectMany(it => it.Priorities)
                     .ToHashSet();

      var tags = new HashSet<string>(kanaTags);
      tags.UnionWith(kanjiTags);

      return new PrioritySpec(tags);
   }

   public string FormatAnswer()
   {
      if(Entries.Count == 1)
      {
         return Entries[0].FormatAnswer();
      }

      string FormatReadings(List<string> readings)
      {
         string FormatReading(string reading) => $"<read>{reading}</read>";
         return readings.Any()
                   ? $"{string.Join("|", readings.Select(FormatReading))}:"
                   : "";
      }

      string CreateSeparatingDescription(DictEntry entry)
      {
         var readingDiff = "";
         var kanjiDiff = "";
         var otherEntries = Entries.Where(other => other != entry).ToList();

         var kanaForms = entry.KanaFormsText().Select(KanaUtils.KatakanaToHiragana).ToHashSet();
         var otherKanaForms = otherEntries
                             .SelectMany(it => it.KanaFormsText())
                             .Select(KanaUtils.KatakanaToHiragana)
                             .ToHashSet();

         if(entry.KanjiForms.Any() && entry.KanjiFormsText()[0] != Word)
         {
            kanjiDiff = $"<tag><ja>{entry.KanjiFormsText()[0]}</ja></tag>:";
         }

         if(entry.KanaFormsText()[0] != Word && !kanaForms.SetEquals(otherKanaForms))
         {
            readingDiff = FormatReadings(
               entry.KanaFormsText()
                    .Select(KanaUtils.KatakanaToHiragana)
                    .Distinct()
                    .ToList()
            );
         }

         return (readingDiff != "" || kanjiDiff != "")
                   ? $"{readingDiff}{kanjiDiff}: "
                   : "";
      }

      return string.Join("\n",
                         Entries.Select(entry =>
                                           $"{CreateSeparatingDescription(entry)}{entry.FormatAnswer()}"));
   }

   public static DictLookupResult Failed()
   {
      if(_failed == null)
      {
         _failed = new DictLookupResult([], "", []);
      }

      return _failed;
   }
}
