using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using JAStudio.Anki;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Menus.Notes.Kanji;
using JAStudio.UI.Menus.Notes.Sentence;
using JAStudio.UI.Menus.Notes.Vocab;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI.Menus;

public class NoteContextMenu(TemporaryServiceCollection services)
{
   readonly TemporaryServiceCollection _services = services;
   readonly VocabNoteMenus _vocabNoteMenus = new(services);
   readonly KanjiNoteMenus _kanjiNoteMenus = new(services);
   readonly SentenceNoteMenus _sentenceNoteMenus = new(services);
   readonly VocabStringMenus _vocabStringMenus = new(services);

   /// <summary>
   /// The right-click menu for <paramref name="note"/>, the note being shown.<br/>
   /// With no note shown, it holds only the actions on the selected text and the clipboard text.
   /// </summary>
   public List<SpecMenuItem> BuildContextMenuSpec(JPNote? note, string selection, string clipboard) =>
      note switch
      {
         VocabNote vocab => BuildVocabContextMenuSpec(vocab.GetId(), selection, clipboard),
         KanjiNote kanji => BuildKanjiContextMenuSpec(kanji.GetId(), selection, clipboard),
         SentenceNote sentence => BuildSentenceContextMenuSpec(sentence.GetId(), selection, clipboard),
         _ => BuildGenericContextMenuSpec(selection, clipboard)
      };

   // ReSharper disable once MemberCanBePrivate.Global used from python
   // ReSharper disable once UnusedMember.Global used from python
   public List<SpecMenuItem> BuildVocabContextMenuSpec(NoteId vocabId, string selection, string clipboard)
   {
      var vocab = _services.CoreApp.Collection.Vocab.WithIdOrNone(vocabId);
      if(vocab == null)
         return [];

      var menuItems = new List<SpecMenuItem>();

      if(!string.IsNullOrEmpty(selection))
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home1("Selection: "), selection, "vocab", vocab));

      if(!string.IsNullOrEmpty(clipboard))
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home2("Clipboard: "), clipboard, "vocab", vocab));

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
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home1("Selection: "), selection, "kanji", kanji));

      if(!string.IsNullOrEmpty(clipboard))
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home2("Clipboard: "), clipboard, "kanji", kanji));

      menuItems.Add(_kanjiNoteMenus.BuildNoteActionsMenuSpec(ShortcutFinger.Home3("Note actions"), kanji));
      menuItems.Add(BuildUniversalNoteActionsMenuSpec(ShortcutFinger.Home4("Universal note actions"), kanji));
      menuItems.Add(KanjiNoteMenus.BuildViewMenuSpec(ShortcutFinger.Home5("View")));

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
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home1("Selection: "), selection, "sentence", sentence));

      if(!string.IsNullOrEmpty(clipboard))
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home2("Clipboard: "), clipboard, "sentence", sentence));

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
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home1("Selection: "), selection, null, null));

      if(!string.IsNullOrEmpty(clipboard))
         menuItems.Add(BuildStringMenuSpec(ShortcutFinger.Home2("Clipboard: "), clipboard, null, null));

      return menuItems;
   }

   // String menu builders (for selection/clipboard)
   SpecMenuItem BuildStringMenuSpec(string title, string text, string? noteType, JPNote? note) =>
      SpecMenuItem.Submenu(title + $" \"{TruncateText(text, 40)}\"",
                           (List<SpecMenuItem>)
                           [
                              BuildCurrentNoteActionsSubmenuSpec(ShortcutFinger.Home1("Current note actions"), text, noteType, note),
                              WebSearchMenuBuilder.BuildWebSearchMenu(ShortcutFinger.Home2("Web"), () => text),
                              BuildMatchingNotesSubmenuSpec(ShortcutFinger.Home3("Exactly matching notes"), text),
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
      var vocabs = _services.CoreApp.Collection.Vocab.WithQuestionOrNamedBy(text);
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
            SpecMenuItem.Command(ShortcutFinger.Home2("Open in workspace"), () => WorkspaceWindow.OpenNote(note)),
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
         if(await WhatToDoWithTheAnkiNoteOfTheDeleted(note) is {} itsAnkiNote)
         {
            _services.BackgroundTaskManager.Run(() => _services.CoreApp.Collection.Delete(note, itsAnkiNote));
         }
      });
   }

   /// <summary>
   /// Asks whether to delete <paramref name="note"/>, and when it has an Anki note, whether to keep the Anki note, with its review history, or to delete<br/>
   /// it too. What to do with the Anki note, or null when the note isn't to be deleted.
   /// </summary>
   static async Task<ItsAnkiNote?> WhatToDoWithTheAnkiNoteOfTheDeleted(JPNote note)
   {
      if(!note.HasAnAnkiNote())
         return await ConfirmDialog.ShowAsync("Delete Note", $"Permanently delete \"{note.GetQuestion()}\" from JAStudio?") ? ItsAnkiNote.IsKept : null;

      return await ChoiceDialog<ItsAnkiNote>.ShowAsync(
                "Delete Note",
                $"Permanently delete \"{note.GetQuestion()}\" from JAStudio?\n\n"
                + "It has an Anki note, which holds its cards and their review history. Keeping the Anki note keeps the history, and its cards will "
                + "show that the note no longer exists in JAStudio. Deleting the Anki note too deletes the history, which can't be undone.",
                [("Delete, keep the Anki note", ItsAnkiNote.IsKept), ("Delete with the Anki note and its history", ItsAnkiNote.IsDeleted)]);
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

   /// <summary>Opens the vocab whose question is <paramref name="text"/>, creating it from the dictionary first when there's none: no two vocab may share a question.</summary>
   void OnCreateVocabNote(string text)
   {
      _services.BackgroundTaskManager.Run(() => WorkspaceWindow.OpenNote(_services.CoreApp.Collection.Vocab.NamedExactlyByOrNone(new WordReference(text)) ?? CreatedFromTheDictionary()));
      return;

      VocabNote CreatedFromTheDictionary()
      {
         var newVocab = _services.VocabNoteFactory.CreateWithDictionary(text);
         _services.LocalNoteUpdater.ReparseSentencesForVocab(newVocab);
         return newVocab;
      }
   }

   void OnCreateSentenceNote(string text) =>
      WorkspaceWindow.OpenNote(SentenceNote.Create(_services.NoteServices, text));

   void OnCreateKanjiNote(string text) =>
      WorkspaceWindow.OpenNote(KanjiNote.CreateByHand(_services.NoteServices, text, "TODO"));
}
