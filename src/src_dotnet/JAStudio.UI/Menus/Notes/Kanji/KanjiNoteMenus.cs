using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI.Menus.Notes.Kanji;

class KanjiNoteMenus
{
   readonly Core.TemporaryServiceCollection _services;

   public KanjiNoteMenus(Core.TemporaryServiceCollection services) => _services = services;

   JPCollection Collection => _services.CoreApp.Collection;

   public SpecMenuItem BuildNoteActionsMenuSpec(string title, KanjiNote kanji)
   {
      var items = new List<SpecMenuItem>
                  {
                     SpecMenuItem.Submenu(ShortcutFinger.Home1("Open"),
                                          new List<SpecMenuItem>
                                          {
                                             NoteListMenuItem.Showing(ShortcutFinger.Home1("Primary Vocabs"), $"Primary vocab of {kanji.GetQuestion()}", () => Collection.Vocab.WithAnyFormIn(kanji.PrimaryVocab)),
                                             NoteListMenuItem.Showing(ShortcutFinger.Home2("Vocabs"), $"Vocab with {kanji.GetQuestion()}", kanji.GetVocabNotes),
                                             NoteListMenuItem.Showing(ShortcutFinger.Home3("Radicals"), $"Radicals of {kanji.GetQuestion()}", kanji.GetRadicalsNotes),
                                             NoteListMenuItem.Showing(ShortcutFinger.Home4("Kanji"), $"Kanji with the radical {kanji.GetQuestion()}", () => Collection.Kanji.WithRadical(kanji.GetQuestion())),
                                             NoteListMenuItem.Showing(ShortcutFinger.Home5("Sentences"), $"Sentences with {kanji.GetQuestion()}", () => Collection.Sentences.SentencesWithSubstring(kanji.GetQuestion()))
                                          }),
                     SpecMenuItem.Command(ShortcutFinger.Home2("Edit"), () => WorkspaceWindow.EditNote(kanji)),
                     SpecMenuItem.UICommand(ShortcutFinger.Home3("Reset Primary Vocabs"), () => kanji.PrimaryVocab = [])
                  };

      if(!kanji.CorpusAnswer.HasValue())
      {
         items.Add(NoteCommandMenuItem.Running(ShortcutFinger.Up1, KanjiCommands.AcceptMeaning, kanji));
      }

      items.Add(NoteCommandMenuItem.Running(ShortcutFinger.Up2, KanjiCommands.PopulateRadicalsFromMnemonicTags, kanji));
      items.Add(NoteCommandMenuItem.Running(ShortcutFinger.Up3, KanjiCommands.BootstrapMnemonicFromRadicals, kanji));
      items.Add(SpecMenuItem.UICommand(ShortcutFinger.Up4("Reset mnemonic"), () => kanji.Mnemonic.Empty()));

      return SpecMenuItem.Submenu(title, items);
   }

   public static SpecMenuItem BuildViewMenuSpec(string title) => SpecMenuItem.Submenu(title, new List<SpecMenuItem>());
}
