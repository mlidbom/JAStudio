using System.Collections.Generic;
using Avalonia.Threading;
using JAStudio.Anki;
using JAStudio.Core.Note;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views;

namespace JAStudio.UI.Menus.Notes.Kanji;

class KanjiNoteMenus
{
   readonly Core.TemporaryServiceCollection _services;

   public KanjiNoteMenus(Core.TemporaryServiceCollection services) => _services = services;

   public SpecMenuItem BuildNoteActionsMenuSpec(string title, KanjiNote kanji)
   {
      var items = new List<SpecMenuItem>
                  {
                     SpecMenuItem.Submenu(ShortcutFinger.Home1("Open"),
                                          new List<SpecMenuItem>
                                          {
                                             SpecMenuItem.Command(ShortcutFinger.Home1("Primary Vocabs"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().VocabsLookupStrings(kanji.PrimaryVocab))),
                                             SpecMenuItem.Command(ShortcutFinger.Home2("Vocabs"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().VocabWithKanji(kanji))),
                                             SpecMenuItem.Command(ShortcutFinger.Home3("Radicals"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesLookup(kanji.GetRadicalsNotes()))),
                                             SpecMenuItem.Command(ShortcutFinger.Home4("Kanji"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesLookup(_services.CoreApp.Collection.Kanji.WithRadical(kanji.GetQuestion())))),
                                             SpecMenuItem.Command(ShortcutFinger.Home5("Sentences"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().SentenceSearch(kanji.GetQuestion(), exact: true)))
                                          }),
                     SpecMenuItem.UICommand(ShortcutFinger.Home2("Edit"), () => Dispatcher.UIThread.Invoke(() => new KanjiEditorDialog(kanji).ShowNearCursor())),
                     SpecMenuItem.UICommand(ShortcutFinger.Home3("Reset Primary Vocabs"), () => kanji.PrimaryVocab = [])
                  };

      // Add conditional "Accept meaning" if no user answer exists
      if(string.IsNullOrEmpty(kanji.UserAnswer.Value))
      {
         items.Add(SpecMenuItem.UICommand(ShortcutFinger.Up1("Accept meaning"), () => kanji.UserAnswer.Set(FormatKanjiMeaning(kanji.GetAnswer()))));
      }

      items.Add(SpecMenuItem.UICommand(ShortcutFinger.Up2("Populate radicals from mnemonic tags"), kanji.PopulateRadicalsFromMnemonicTags));
      items.Add(SpecMenuItem.UICommand(ShortcutFinger.Up3("Bootstrap mnemonic from radicals"), kanji.BootstrapMnemonicFromRadicals));
      items.Add(SpecMenuItem.UICommand(ShortcutFinger.Up4("Reset mnemonic"), () => kanji.UserMnemonic.Set("")));

      return SpecMenuItem.Submenu(title, items);
   }

   public static SpecMenuItem BuildViewMenuSpec(string title) => SpecMenuItem.Submenu(title, new List<SpecMenuItem>());

   static string FormatKanjiMeaning(string meaning)
   {
      // Replace HTML and bracket markup with pipe separator
      var result = meaning
                  .Replace("<", "|")
                  .Replace(">", "|")
                  .Replace("[", "|")
                  .Replace("]", "|")
                  .ToLower()
                  .Replace("||", "|")
                  .Replace("||", "|")
                  .Replace("||", "|")
                  .Replace(", ", "|")
                  .Replace(" ", "-")
                  .Replace("-|-", " | ");

      // Remove leading/trailing pipes
      result = result.TrimEnd('|').TrimStart('|');
      return result;
   }
}
