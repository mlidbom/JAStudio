using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;

namespace JAStudio.UI.Menus.Notes.Kanji;

/// <summary>
/// The menu of what a kanji can do with a text selected in it, or on the clipboard: put the vocab the text names at a position among the kanji's primary<br/>
/// vocab, or remove it, which is disabled when the text names no word (<see cref="WordReference.FromDisambiguationNameOrNone"/>); add the text as a kanji<br/>
/// with a similar meaning or one the kanji is confused with; and make a reading it names primary or secondary.
/// </summary>
static class KanjiStringMenus
{
   public static SpecMenuItem BuildStringMenuSpec(string title, string text, KanjiNote kanji)
   {
      var menuItems = new List<SpecMenuItem>
                      {
                         BuildPrimaryVocabMenuSpec(WordReference.FromDisambiguationNameOrNone(text)),
                         BuildAddMenuSpec()
                      };
      menuItems.AddRange(AddPrimaryReadingsActions(ShortcutFinger.Home3, text));

      return SpecMenuItem.Submenu(title, menuItems);

      SpecMenuItem BuildPrimaryVocabMenuSpec(WordReference? word)
      {
         var primaryVocab = kanji.PrimaryVocab;
         var items = primaryVocab.Words
                                 .Select((primary, position) => ForTheWord(ShortcutFinger.Numpad(position, primary.DisambiguationName), _ => true, it => primaryVocab.PutAt(it, position)))
                                 .ToList();

         items.Add(ForTheWord(ShortcutFinger.Home1("[Last]"), _ => true, primaryVocab.PutLast));
         items.Add(ForTheWord(ShortcutFinger.Home2("Remove"), primaryVocab.Contains, primaryVocab.Remove));

         return SpecMenuItem.Submenu(ShortcutFinger.Home1("Highlighted Vocab"), items);

         SpecMenuItem ForTheWord(string name, Func<WordReference, bool> applies, Action<WordReference> action) =>
            word is {} it
               ? SpecMenuItem.UICommand(name, () => action(it), enabled: applies(it))
               : SpecMenuItem.UICommand(name, () => {}, enabled: false);
      }

      SpecMenuItem BuildAddMenuSpec()
      {
         var items = new List<SpecMenuItem>
                     {
                        NoteCommandMenuItem.Running(ShortcutFinger.Home1, KanjiCommands.AddSimilarMeaning, kanji, text),
                        SpecMenuItem.UICommand(ShortcutFinger.Home2("Confused with"), () => kanji.ConfusedWith.Add(text))
                     };

         return SpecMenuItem.Submenu(ShortcutFinger.Home2("Add"), items);
      }

      List<SpecMenuItem> AddPrimaryReadingsActions(Func<string, string> titleFactory, string reading)
      {
         var items = new List<SpecMenuItem>();

         if(KanaUtils.IsOnlyKatakana(reading))
         {
            var onReadings = kanji.Readings.On;
            if(onReadings.IsPrimary(reading))
            {
               items.Add(SpecMenuItem.UICommand(titleFactory("Remove primary Onyomi Reading"), () => onReadings.MakeSecondary(reading)));
            } else if(onReadings.Contains(reading))
            {
               items.Add(SpecMenuItem.UICommand(titleFactory("Make primary Onyomi Reading"), () => onReadings.MakePrimary(reading)));
            }
         } else if(KanaUtils.IsOnlyHiragana(reading))
         {
            var kunReadings = kanji.Readings.Kun;
            if(kunReadings.IsPrimary(reading))
            {
               items.Add(SpecMenuItem.UICommand(titleFactory("Remove primary Kunyomi reading"), () => kunReadings.MakeSecondary(reading)));
            } else if(kunReadings.Contains(reading))
            {
               items.Add(SpecMenuItem.UICommand(titleFactory("Make primary Kunyomi reading"), () => kunReadings.MakePrimary(reading)));
            }
         }

         return items;
      }
   }
}
