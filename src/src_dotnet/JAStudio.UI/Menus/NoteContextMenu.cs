using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using JAStudio.Anki;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Menus.Notes.Kanji;
using JAStudio.UI.Menus.Notes.Sentence;
using JAStudio.UI.Menus.Notes.Vocab;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;

namespace JAStudio.UI.Menus;

public class NoteContextMenu(Core.TemporaryServiceCollection services)
{
   readonly Core.TemporaryServiceCollection _services = services;
   readonly VocabNoteMenus _vocabNoteMenus = new(services);
   readonly KanjiNoteMenus _kanjiNoteMenus = new(services);
   readonly SentenceNoteMenus _sentenceNoteMenus = new(services);
   readonly OpenInAnkiMenus _openInAnkiMenus = new(services);
   readonly VocabStringMenus _vocabStringMenus = new(services);

   // ReSharper disable once MemberCanBePrivate.Global used from python
   // ReSharper disable once UnusedMember.Global used from python
   public List<SpecMenuItem> BuildVocabContextMenuSpec(NoteId vocabId, string selection, string clipboard)
   {
      var vocab = _services.CoreApp.Collection.Vocab.WithIdOrNone(vocabId);
      if(vocab == null)
         return [];

      var menuItems = new List<SpecMenuItem>();

      if(!string.IsNullOrEmpty(selection))
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home1($"Selection: "), selection, "vocab", vocab));

      if(!string.IsNullOrEmpty(clipboard))
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home2($"Clipboard: "), clipboard, "vocab", vocab));

      menuItems.Add(_vocabNoteMenus.BuildNoteActionsMenuSpec(ShortcutFinger.Home3("Note actions"), vocab));
      menuItems.Add(BuildUniversalNoteActionsMenuSpec(ShortcutFinger.Home4("Universal note actions"), vocab));
      menuItems.Add(SpecMenuItem.Submenu(ShortcutFinger.Home5("View"), new List<SpecMenuItem>()));

      return menuItems;
   }

   // ReSharper disable once MemberCanBePrivate.Global used from python
   // ReSharper disable once UnusedMember.Global used from python
   public List<SpecMenuItem> BuildKanjiContextMenuSpec(NoteId kanjiId, string selection, string clipboard)
   {
      var kanji = _services.CoreApp.Collection.Kanji.WithIdOrNone(kanjiId);
      if(kanji == null)
         return [];

      var menuItems = new List<SpecMenuItem>();

      if(!string.IsNullOrEmpty(selection))
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home1($"Selection: "), selection, "kanji", kanji));

      if(!string.IsNullOrEmpty(clipboard))
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home2($"Clipboard: "), clipboard, "kanji", kanji));

      menuItems.Add(_kanjiNoteMenus.BuildNoteActionsMenuSpec(ShortcutFinger.Home3("Note actions"), kanji));
      menuItems.Add(BuildUniversalNoteActionsMenuSpec(ShortcutFinger.Home4("Universal note actions"), kanji));
      menuItems.Add(_kanjiNoteMenus.BuildViewMenuSpec(ShortcutFinger.Home5("View")));

      return menuItems;
   }

   // ReSharper disable once MemberCanBePrivate.Global used from python
   // ReSharper disable once UnusedMember.Global used from python
   public List<SpecMenuItem> BuildSentenceContextMenuSpec(NoteId sentenceId, string selection, string clipboard)
   {
      var sentence = _services.CoreApp.Collection.Sentences.WithIdOrNone(sentenceId);
      if(sentence == null)
         return [];

      var menuItems = new List<SpecMenuItem>();

      if(!string.IsNullOrEmpty(selection))
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home1($"Selection: "), selection, "sentence", sentence));

      if(!string.IsNullOrEmpty(clipboard))
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home2($"Clipboard: "), clipboard, "sentence", sentence));

      menuItems.Add(_sentenceNoteMenus.BuildNoteActionsMenuSpec(ShortcutFinger.Home3("Note actions"), sentence));
      menuItems.Add(BuildUniversalNoteActionsMenuSpec(ShortcutFinger.Home4("Universal note actions"), sentence));
      menuItems.Add(_sentenceNoteMenus.BuildViewMenuSpec(ShortcutFinger.Home5("View")));

      return menuItems;
   }

   // ReSharper disable once MemberCanBePrivate.Global used from python
   // ReSharper disable once UnusedMember.Global used from python
   public List<SpecMenuItem> BuildGenericContextMenuSpec(string selection, string clipboard)
   {
      var menuItems = new List<SpecMenuItem>();

      if(!string.IsNullOrEmpty(selection))
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home1($"Selection: "), selection, null, null));

      if(!string.IsNullOrEmpty(clipboard))
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home2($"Clipboard: "), clipboard, null, null));

      return menuItems;
   }

   // String menu builders (for selection/clipboard)
   SpecMenuItem BuildStringMenuSpec(string title, string text, string? noteType, JPNote? note) =>
      SpecMenuItem.Submenu(title + $" \"{TruncateText(text, 40)}\"",
                           (List<SpecMenuItem>)
                           [
                              BuildCurrentNoteActionsSubmenuSpec(ShortcutFinger.Home1("Current note actions"), text, noteType, note),
                              _openInAnkiMenus.BuildOpenInAnkiMenuSpec(ShortcutFinger.Home2("Anki"), () => text),
                              WebSearchMenuBuilder.BuildWebSearchMenu(ShortcutFinger.Home3("Web"), () => text),
                              BuildMatchingNotesSubmenuSpec(ShortcutFinger.Home4("Exactly matching notes"), text),
                              BuildCreateNoteSubmenuSpec(ShortcutFinger.Up1($"Create: {TruncateText(text, 40)}"), text),
                              SpecMenuItem.Command(ShortcutFinger.Down1("Reparse matching sentences"), () => OnReparseMatchingSentences(text))
                           ]);

   SpecMenuItem BuildCurrentNoteActionsSubmenuSpec(string title, string text, string? noteType, JPNote? note)
   {
      // Delegate to note-type-specific string menu builders
      if(noteType == "vocab" && note is VocabNote vocab)
      {
         return _vocabStringMenus.BuildStringMenuSpec(title, text, vocab);
      }

      if(noteType == "kanji" && note is KanjiNote kanji)
      {
         return KanjiStringMenus.BuildStringMenuSpec(title, text, kanji);
      }

      if(noteType == "sentence" && note is SentenceNote sentence)
      {
         return SentenceStringMenus.BuildStringMenuSpec(title, sentence, text);
      }

      return SpecMenuItem.Submenu(title, new List<SpecMenuItem>());
   }

   SpecMenuItem BuildMatchingNotesSubmenuSpec(string title, string text)
   {
      // Find notes that exactly match the search text
      var vocabs = _services.CoreApp.Collection.Vocab.WithQuestionPreferDisambiguationName(text).ToList();
      var sentences = _services.CoreApp.Collection.Sentences.WithQuestion(text);
      var kanjis = text.Length == 1
                      ? _services.CoreApp.Collection.Kanji.WithAnyKanjiIn([text])
                      : [];

      // Only show submenu if any notes match
      if(!vocabs.Any() && !sentences.Any() && !kanjis.Any())
      {
         return SpecMenuItem.Submenu(title, new List<SpecMenuItem>());
      }

      var items = new List<SpecMenuItem>
                  {
                     BuildUniversalNoteActionsMenuSpec(ShortcutFinger.Home1("Vocab Actions"), vocabs.FirstOrDefault()),
                     BuildUniversalNoteActionsMenuSpec(ShortcutFinger.Home2("Sentence Actions"), sentences.FirstOrDefault()),
                     BuildUniversalNoteActionsMenuSpec(ShortcutFinger.Home3("Kanji Actions"), kanjis.FirstOrDefault())
                  };

      return SpecMenuItem.Submenu(title, items);
   }

   SpecMenuItem BuildUniversalNoteActionsMenuSpec(string label, JPNote? note)
   {
      if(note == null)
      {
         return SpecMenuItem.Submenu(label, new List<SpecMenuItem>());
      }

      return SpecMenuItem.Submenu(
         label,
         new List<SpecMenuItem>
         {
            SpecMenuItem.Command(ShortcutFinger.Home1("Open in previewer"), () => OnOpenInPreviewer(note)),
            SpecMenuItem.UICommand(ShortcutFinger.Home3("Unsuspend all cards"), note.UnsuspendAllCards, enabled: note.HasSuspendedCards()),
            SpecMenuItem.UICommand(ShortcutFinger.Home4("Suspend all cards"), note.SuspendAllCards, enabled: note.HasActiveCards()),
            SpecMenuItem.Command(ShortcutFinger.Home5("Delete note"), () => OnDeleteNote(note))
         }
      );
   }

   SpecMenuItem BuildCreateNoteSubmenuSpec(string title, string text)
   {
      return SpecMenuItem.Submenu(
         title,
         new List<SpecMenuItem>
         {
            SpecMenuItem.Command(ShortcutFinger.Home1("vocab"), () => OnCreateVocabNote(text)),
            SpecMenuItem.Command(ShortcutFinger.Home2("sentence"), () => OnCreateSentenceNote(text)),
            SpecMenuItem.Command(ShortcutFinger.Home3("kanji"), () => OnCreateKanjiNote(text))
         }
      );
   }

   // Utility methods
   static string TruncateText(string text, int maxLength)
   {
      if(text.Length <= maxLength)
         return text;
      return text.Substring(0, maxLength) + "...";
   }

   // Action handlers
   void OnOpenInPreviewer(JPNote note)
   {
      var query = _services.QueryBuilder().NotesLookup([note]);
      AnkiFacade.Browser.ExecuteLookupAndShowPreviewer(query);
   }

   void OnDeleteNote(JPNote note)
   {
      _ = Dispatcher.UIThread.InvokeAsync(async () =>
      {
         if(await ConfirmDialog.ShowAsync(
               "Delete Note",
               $"Permanently delete \"{note.GetQuestion()}\"?\n\nThis will remove it from corpus data and from Anki, including all review history."))
         {
            _services.BackgroundTaskManager.Run(() => _services.CoreApp.Collection.Delete(note));
            AnkiFacade.Browser.ExecuteLookup("");//Keeps Anki from choking on a refresh of a note that no longer exists. Just resets the browser to not having a search.
         }
      });
   }

   void OnReparseMatchingSentences(string text)
   {
      _services.BackgroundTaskManager.Run(() =>
      {
         _services.LocalNoteUpdater.ReparseMatchingSentences(text);
         AnkiFacade.UIUtils.Refresh();
         AnkiFacade.UIUtils.ShowTooltip($"Reparsed sentences matching: {text}");
      });
   }

   void OnCreateVocabNote(string text)
   {
      _services.BackgroundTaskManager.Run(() =>
      {
         var newVocab = _services.VocabNoteFactory.CreateWithDictionary(text);
         _services.LocalNoteUpdater.ReparseSentencesForVocab(newVocab);

         var query = _services.QueryBuilder().NotesLookup([newVocab]);
         AnkiFacade.Browser.ExecuteLookupAndShowPreviewer(query);
      });
   }

   void OnCreateSentenceNote(string text)
   {
      var noteServices = _services.NoteServices;
      var newSentence = SentenceNote.Create(noteServices, text);

      var query = _services.QueryBuilder().NotesLookup([newSentence]);
      AnkiFacade.Browser.ExecuteLookupAndShowPreviewer(query);
   }

   void OnCreateKanjiNote(string text)
   {
      var noteServices = _services.NoteServices;
      var newKanji = KanjiNote.Create(noteServices, text, "TODO", "", "");

      var query = _services.QueryBuilder().NotesLookup([newKanji]);
      AnkiFacade.Browser.ExecuteLookupAndShowPreviewer(query);
   }
}
