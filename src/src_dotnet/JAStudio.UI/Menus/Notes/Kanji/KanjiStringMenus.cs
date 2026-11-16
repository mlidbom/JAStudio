using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note;
using JAStudio.Core.Note.KanjiRelationships;
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
         var items = kanji.PrimaryVocab.Words
                          .Select((primary, position) => ForTheWord(ShortcutFinger.Numpad(position, primary.DisambiguationName), _ => true, (primaryVocab, it) => primaryVocab.PutAt(it, position)))
                          .ToList();

         items.Add(ForTheWord(ShortcutFinger.Home1("[Last]"), _ => true, (primaryVocab, it) => primaryVocab.PutLast(it)));
         items.Add(ForTheWord(ShortcutFinger.Home2("Remove"), kanji.PrimaryVocab.Contains, (primaryVocab, it) => primaryVocab.Remove(it)));

         return SpecMenuItem.Submenu(ShortcutFinger.Home1("Highlighted Vocab"), items);

         SpecMenuItem ForTheWord(string name, Func<WordReference, bool> applies, Action<KanjiPrimaryVocab, WordReference> change) =>
            word is {} it
               ? SpecMenuItem.Command(name, () => kanji.Edit(edited => change(edited.PrimaryVocab, it)), enabled: applies(it))
               : SpecMenuItem.Command(name, () => {}, enabled: false);
      }

      SpecMenuItem BuildAddMenuSpec()
      {
         var items = new List<SpecMenuItem>
                     {
                        NoteCommandMenuItem.Running(ShortcutFinger.Home1, KanjiCommands.AddSimilarMeaning, kanji, text),
                        SpecMenuItem.Command(ShortcutFinger.Home2("Confused with"), () => kanji.Edit(it => it.ConfusedWith.Add(text)))
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
               items.Add(SpecMenuItem.Command(titleFactory("Remove primary Onyomi Reading"), () => kanji.Edit(it => it.Readings.On.MakeSecondary(reading))));
            } else if(onReadings.Contains(reading))
            {
               items.Add(SpecMenuItem.Command(titleFactory("Make primary Onyomi Reading"), () => kanji.Edit(it => it.Readings.On.MakePrimary(reading))));
            }
         } else if(KanaUtils.IsOnlyHiragana(reading))
         {
            var kunReadings = kanji.Readings.Kun;
            if(kunReadings.IsPrimary(reading))
            {
               items.Add(SpecMenuItem.Command(titleFactory("Remove primary Kunyomi reading"), () => kanji.Edit(it => it.Readings.Kun.MakeSecondary(reading))));
            } else if(kunReadings.Contains(reading))
            {
               items.Add(SpecMenuItem.Command(titleFactory("Make primary Kunyomi reading"), () => kanji.Edit(it => it.Readings.Kun.MakePrimary(reading))));
            }
         }

         return items;
      }
   }
}
