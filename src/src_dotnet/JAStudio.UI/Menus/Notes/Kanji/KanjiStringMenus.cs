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
            var onReadings = kanji.Readings.On;
            if(onReadings.IsPrimary(str))
            {
               items.Add(SpecMenuItem.UICommand(titleFactory("Remove primary Onyomi Reading"), () => onReadings.MakeSecondary(str)));
            } else if(onReadings.Contains(str))
            {
               items.Add(SpecMenuItem.UICommand(titleFactory("Make primary Onyomi Reading"), () => onReadings.MakePrimary(str)));
            }
         } else if(KanaUtils.IsOnlyHiragana(str))
         {
            var kunReadings = kanji.Readings.Kun;
            if(kunReadings.IsPrimary(str))
            {
               items.Add(SpecMenuItem.UICommand(titleFactory("Remove primary Kunyomi reading"), () => kunReadings.MakeSecondary(str)));
            } else if(kunReadings.Contains(str))
            {
               items.Add(SpecMenuItem.UICommand(titleFactory("Make primary Kunyomi reading"), () => kunReadings.MakePrimary(str)));
            }
         }

         return items;
      }

      SpecMenuItem BuildAddMenuSpec()
      {
         var items = new List<SpecMenuItem>
                     {
                        SpecMenuItem.UICommand(ShortcutFinger.Home1("Similar meaning"), () => kanji.AddSimilarMeaning(text)),
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
      menuItems.AddRange(AddPrimaryReadingsActions(ShortcutFinger.Home3, text));

      return SpecMenuItem.Submenu(
         title,
         menuItems
      );
   }
}
