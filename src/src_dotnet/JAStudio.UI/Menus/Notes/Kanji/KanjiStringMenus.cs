using System;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;

namespace JAStudio.UI.Menus.Notes.Kanji;

/// <summary>
/// Kanji string menu builders (selection/clipboard context menus).
/// Corresponds to notes/kanji/string_menu.py in Python.
/// </summary>
static class KanjiStringMenus
{
   public static SpecMenuItem BuildStringMenuSpec(string title, string text, KanjiNote kanji)
   {
      SpecMenuItem BuildHighlightedVocabMenuSpec(string vocabToAdd)
      {
         var items = new List<SpecMenuItem>();
         var primaryVocab = kanji.PrimaryVocab;

         // Add positioning actions for each existing primary vocab
         for(var i = 0; i < primaryVocab.Count; i++)
         {
            var vocab = primaryVocab[i];
            var index = i; // Capture for lambda
            items.Add(SpecMenuItem.UICommand(ShortcutFinger.Numpad(index, vocab), () => kanji.PositionPrimaryVocab(vocabToAdd, index)));
         }

         // Add [Last] option
         items.Add(SpecMenuItem.UICommand(ShortcutFinger.Home1("[Last]"), () => kanji.PositionPrimaryVocab(vocabToAdd)));

         // Add Remove option if vocab is already in primary vocab
         if(primaryVocab.Contains(vocabToAdd))
         {
            items.Add(SpecMenuItem.UICommand(ShortcutFinger.Home2("Remove"), () => kanji.RemovePrimaryVocab(vocabToAdd)));
         }

         return SpecMenuItem.Submenu(ShortcutFinger.Home1("Highlighted Vocab"), items);
      }

      List<SpecMenuItem> AddPrimaryReadingsActions(Func<string, string> titleFactory, string str)
      {
         var items = new List<SpecMenuItem>();

         if(KanaUtils.IsOnlyKatakana(str))
         {
            var hiraganaString = KanaUtils.KatakanaToHiragana(str);
            if(kanji.PrimaryReadingsOn.Contains(hiraganaString))
            {
               items.Add(SpecMenuItem.UICommand(titleFactory("Remove primary Onyomi Reading"), () => kanji.RemovePrimaryOnReading(hiraganaString)));
            } else if(kanji.ReadingsOn.Contains(hiraganaString))
            {
               items.Add(SpecMenuItem.UICommand(titleFactory("Make primary Onyomi Reading"), () => kanji.AddPrimaryOnReading(hiraganaString)));
            }
         } else if(KanaUtils.IsOnlyHiragana(str))
         {
            if(kanji.PrimaryReadingsKun.Contains(str))
            {
               items.Add(SpecMenuItem.UICommand(titleFactory("Remove primary Kunyomi reading"), () => kanji.RemovePrimaryKunReading(str)));
            } else if(kanji.ReadingsKun.Contains(str))
            {
               items.Add(SpecMenuItem.UICommand(titleFactory("Make primary Kunyomi reading"), () => kanji.AddPrimaryKunReading(str)));
            }
         }

         return items;
      }

      SpecMenuItem BuildAddMenuSpec()
      {
         var items = new List<SpecMenuItem>
                     {
                        SpecMenuItem.UICommand(ShortcutFinger.Home1("Similar meaning"), () => kanji.AddUserSimilarMeaning(text)),
                        SpecMenuItem.UICommand(ShortcutFinger.Home2("Confused with"), () => kanji.AddRelatedConfusedWith(text))
                     };

         return SpecMenuItem.Submenu(ShortcutFinger.Home2("Add"), items);
      }

      // Build the menu hierarchy
      var menuItems = new List<SpecMenuItem>
                      {
                         BuildHighlightedVocabMenuSpec(text),
                         BuildAddMenuSpec()
                      };

      // Add primary readings actions directly to menu
      menuItems.AddRange(AddPrimaryReadingsActions(
                            title => ShortcutFinger.Home3(title),
                            text));

      return SpecMenuItem.Submenu(
         title,
         menuItems
      );
   }
}
