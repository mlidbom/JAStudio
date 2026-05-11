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
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

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
   public List<SpecMenuItem> BuildVocabContextMenuSpec(NoteId vocabId, string selection, string clipboard)
   {
      var vocab = _services.CoreApp.Collection.Vocab.WithIdOrNone(vocabId);
      if(vocab == null)
         return [];

      var menuItems = new List<SpecMenuItem>();

      if(!string.IsNullOrEmpty(selection))
         menuItems.Add(BuildSelectionMenuSpec(selection, "vocab", vocab));

      if(!string.IsNullOrEmpty(clipboard))
         menuItems.Add(BuildClipboardMenuSpec(clipboard, "vocab", vocab));

      menuItems.Add(_vocabNoteMenus.BuildNoteActionsMenuSpec(vocab));
      menuItems.Add(BuildUniversalNoteActionsMenuSpec(vocab));
      menuItems.Add(SpecMenuItem.Submenu(ShortcutFinger.Home5("View"), new List<SpecMenuItem>()));

      return menuItems;
   }

   // ReSharper disable once MemberCanBePrivate.Global used from python
   public List<SpecMenuItem> BuildKanjiContextMenuSpec(NoteId kanjiId, string selection, string clipboard)
   {
      var kanji = _services.CoreApp.Collection.Kanji.WithIdOrNone(kanjiId);
      if(kanji == null)
         return [];

      var menuItems = new List<SpecMenuItem>();

      if(!string.IsNullOrEmpty(selection))
         menuItems.Add(BuildSelectionMenuSpec(selection, "kanji", kanji));

      if(!string.IsNullOrEmpty(clipboard))
         menuItems.Add(BuildClipboardMenuSpec(clipboard, "kanji", kanji));

      menuItems.Add(_kanjiNoteMenus.BuildNoteActionsMenuSpec(kanji));
      menuItems.Add(BuildUniversalNoteActionsMenuSpec(kanji));
      menuItems.Add(_kanjiNoteMenus.BuildViewMenuSpec());

      return menuItems;
   }

   // ReSharper disable once MemberCanBePrivate.Global used from python
   public List<SpecMenuItem> BuildSentenceContextMenuSpec(NoteId sentenceId, string selection, string clipboard)
   {
      var sentence = _services.CoreApp.Collection.Sentences.WithIdOrNone(sentenceId);
      if(sentence == null)
         return [];

      var menuItems = new List<SpecMenuItem>();

      if(!string.IsNullOrEmpty(selection))
         menuItems.Add(BuildSelectionMenuSpec(selection, "sentence", sentence));

      if(!string.IsNullOrEmpty(clipboard))
         menuItems.Add(BuildClipboardMenuSpec(clipboard, "sentence", sentence));

      menuItems.Add(_sentenceNoteMenus.BuildNoteActionsMenuSpec(sentence));
      menuItems.Add(BuildUniversalNoteActionsMenuSpec(sentence));
      menuItems.Add(_sentenceNoteMenus.BuildViewMenuSpec());

      return menuItems;
   }

   // ReSharper disable once MemberCanBePrivate.Global used from python
   public List<SpecMenuItem> BuildGenericContextMenuSpec(string selection, string clipboard)
   {
      var menuItems = new List<SpecMenuItem>();

      if(!string.IsNullOrEmpty(selection))
         menuItems.Add(BuildSelectionMenuSpec(selection, null, null));

      if(!string.IsNullOrEmpty(clipboard))
         menuItems.Add(BuildClipboardMenuSpec(clipboard, null, null));

      return menuItems;
   }

   // ReSharper disable once UnusedMember.Global used from python
   public List<Avalonia.Controls.MenuItem> BuildVocabContextMenu(long vocabId, string selection, string clipboard)
   {
      var noteId = _services.CoreApp.Collection.Vocab.ExternalIdToNoteId(vocabId);
      if(noteId == null) return [];
      var specs = BuildVocabContextMenuSpec(noteId, selection, clipboard);
      return ConvertToAvaloniaMenuItems(specs);
   }

   // ReSharper disable once UnusedMember.Global used from python
   public List<Avalonia.Controls.MenuItem> BuildKanjiContextMenu(long kanjiId, string selection, string clipboard)
   {
      var noteId = _services.CoreApp.Collection.Kanji.ExternalIdToNoteId(kanjiId);
      if(noteId == null) return [];
      var specs = BuildKanjiContextMenuSpec(noteId, selection, clipboard);
      return ConvertToAvaloniaMenuItems(specs);
   }

   // ReSharper disable once UnusedMember.Global used from python
   public List<Avalonia.Controls.MenuItem> BuildSentenceContextMenu(long sentenceId, string selection, string clipboard)
   {
      var noteId = _services.CoreApp.Collection.Sentences.ExternalIdToNoteId(sentenceId);
      if(noteId == null) return [];
      var specs = BuildSentenceContextMenuSpec(noteId, selection, clipboard);
      return ConvertToAvaloniaMenuItems(specs);
   }

   // ReSharper disable once UnusedMember.Global used from python
   public List<Avalonia.Controls.MenuItem> BuildGenericContextMenu(string selection, string clipboard)
   {
      var specs = BuildGenericContextMenuSpec(selection, clipboard);
      return ConvertToAvaloniaMenuItems(specs);
   }

   // String menu builders (for selection/clipboard)
   SpecMenuItem BuildSelectionMenuSpec(string selection, string? noteType, JPNote? note)
   {
      var truncated = TruncateText(selection, 40);
      var menuItems = BuildStringMenuSpec(selection, noteType, note);

      return SpecMenuItem.Submenu(
         ShortcutFinger.Home1($"Selection: \"{truncated}\""),
         menuItems
      );
   }

   SpecMenuItem BuildClipboardMenuSpec(string clipboard, string? noteType, JPNote? note)
   {
      var truncated = TruncateText(clipboard, 40);
      var menuItems = BuildStringMenuSpec(clipboard, noteType, note);

      return SpecMenuItem.Submenu(
         ShortcutFinger.Home2($"Clipboard: \"{truncated}\""),
         menuItems
      );
   }

   List<SpecMenuItem> BuildStringMenuSpec(string text, string? noteType, JPNote? note)
   {
      return
      [
         BuildCurrentNoteActionsSubmenuSpec(text, noteType, note),
         _openInAnkiMenus.BuildOpenInAnkiMenuSpec(() => text),
         WebSearchMenuBuilder.BuildWebSearchMenu(() => text),
         BuildMatchingNotesSubmenuSpec(text),
         BuildCreateNoteSubmenuSpec(text),
         SpecMenuItem.Command(ShortcutFinger.Down1("Reparse matching sentences"), () => OnReparseMatchingSentences(text))
      ];
   }

   SpecMenuItem BuildCurrentNoteActionsSubmenuSpec(string text, string? noteType, JPNote? note)
   {
      // Delegate to note-type-specific string menu builders
      if(noteType == "vocab" && note is VocabNote vocab)
      {
         return _vocabStringMenus.BuildStringMenuSpec(text, vocab);
      }

      if(noteType == "kanji" && note is KanjiNote kanji)
      {
         return KanjiStringMenus.BuildStringMenuSpec(text, kanji);
      }

      if(noteType == "sentence" && note is SentenceNote sentence)
      {
         return SentenceStringMenus.BuildStringMenuSpec(sentence, text);
      }

      return SpecMenuItem.Submenu(ShortcutFinger.Home1("Current note actions"), new List<SpecMenuItem>());
   }

   SpecMenuItem BuildUniversalNoteActionsMenuSpec(JPNote note)
   {
      var hasSuspendedCards = note.HasSuspendedCards();
      var hasActiveCards = note.HasActiveCards();

      return SpecMenuItem.Submenu(
         ShortcutFinger.Home4("Universal note actions"),
         new List<SpecMenuItem>
         {
            SpecMenuItem.Command(ShortcutFinger.Home1("Open in previewer"),
                                 () => OnOpenInPreviewer(note)),
            SpecMenuItem.Command(ShortcutFinger.Home3("Unsuspend all cards"),
                                 note.UnsuspendAllCards,
                                 null,
                                 null,
                                 hasSuspendedCards),
            SpecMenuItem.Command(ShortcutFinger.Home4("Suspend all cards"),
                                 note.SuspendAllCards,
                                 null,
                                 null,
                                 hasActiveCards),
            SpecMenuItem.Command(ShortcutFinger.Home5("Delete note"),
                                 () => OnDeleteNote(note))
         }
      );
   }

   SpecMenuItem BuildMatchingNotesSubmenuSpec(string text)
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
         return SpecMenuItem.Submenu(ShortcutFinger.Home4("Exactly matching notes"), new List<SpecMenuItem>());
      }

      var items = new List<SpecMenuItem>
                  {
                     BuildUniversalNoteActionsMenuSpec(ShortcutFinger.Home1("Vocab Actions"), vocabs.FirstOrDefault()),
                     BuildUniversalNoteActionsMenuSpec(ShortcutFinger.Home2("Sentence Actions"), sentences.FirstOrDefault()),
                     BuildUniversalNoteActionsMenuSpec(ShortcutFinger.Home3("Kanji Actions"), kanjis.FirstOrDefault())
                  };

      return SpecMenuItem.Submenu(ShortcutFinger.Home4("Exactly matching notes"), items);
   }

   SpecMenuItem BuildUniversalNoteActionsMenuSpec(string label, JPNote? note)
   {
      if(note == null)
      {
         return SpecMenuItem.Submenu(label, new List<SpecMenuItem>());
      }

      var hasSuspendedCards = note.HasSuspendedCards();
      var hasActiveCards = note.HasActiveCards();

      return SpecMenuItem.Submenu(
         label,
         new List<SpecMenuItem>
         {
            SpecMenuItem.Command(ShortcutFinger.Home1("Open in previewer"), () => OnOpenInPreviewer(note)),
            SpecMenuItem.Command(ShortcutFinger.Home3("Unsuspend all cards"), note.UnsuspendAllCards, null, null, hasSuspendedCards),
            SpecMenuItem.Command(ShortcutFinger.Home4("Suspend all cards"), note.SuspendAllCards, null, null, hasActiveCards),
            SpecMenuItem.Command(ShortcutFinger.Home5("Delete note"), () => OnDeleteNote(note))
         }
      );
   }

   SpecMenuItem BuildCreateNoteSubmenuSpec(string text)
   {
      return SpecMenuItem.Submenu(
         ShortcutFinger.Up1($"Create: {TruncateText(text, 40)}"),
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

   static List<Avalonia.Controls.MenuItem> ConvertToAvaloniaMenuItems(List<SpecMenuItem> specs)
   {
      var result = new List<Avalonia.Controls.MenuItem>();
      foreach(var spec in specs)
         result.Add(AvaloniaMenuAdapter.ToAvalonia(spec));
      return result;
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
         var box = MessageBoxManager.GetMessageBoxStandard(
            "Delete Note",
            $"Permanently delete \"{note.GetQuestion()}\"?\n\nThis will remove it from corpus data and from Anki, including all review history.",
            ButtonEnum.YesNo,
            Icon.Warning);

         if(await box.ShowAsync() == ButtonResult.Yes)
            _services.BackgroundTaskManager.Run(() => _services.CoreApp.Collection.Delete(note));
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
