using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Utilities.Logging;
using JAStudio.Anki;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Menus.Notes.Kanji;
using JAStudio.UI.Menus.Notes.Sentence;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;

namespace JAStudio.UI.Menus;

class AnkiBrowserMenuBuilder
{
   readonly Core.TemporaryServiceCollection _services;
   readonly VocabNoteMenus _vocabNoteMenus;
   readonly KanjiNoteMenus _kanjiNoteMenus;
   readonly SentenceNoteMenus _sentenceNoteMenus;

   internal AnkiBrowserMenuBuilder(Core.TemporaryServiceCollection services)
   {
      _services = services;
      _vocabNoteMenus = new VocabNoteMenus(services);
      _kanjiNoteMenus = new KanjiNoteMenus(services);
      _sentenceNoteMenus = new SentenceNoteMenus(services);
   }

   public SpecMenuItem BuildBrowserMenuSpec(IReadOnlyList<long> selectedCardIds, IReadOnlyList<long> selectedNoteIds)
   {
      var items = new List<SpecMenuItem>();

      if(selectedCardIds.Count == 1)
      {
         items.Add(SpecMenuItem.Command("Prioritize selected cards", () => AnkiFacade.Browser.MenuActions.PrioritizeCards(selectedCardIds)));

         var note = GetNoteFromCardId(selectedCardIds[0]);
         if(note != null)
         {
            items.Add(BuildNoteActionsSubmenu(note));
         }
      }

      if(selectedCardIds.Count > 0)
      {
         items.Add(BuildSpreadSubmenu(selectedCardIds));
      }

      var sentenceNotes = GetSentenceNotes(selectedNoteIds);
      if(sentenceNotes.Any())
      {
         items.Add(SpecMenuItem.Command("Reparse sentence words", () => OnReparseSentences(sentenceNotes)));
      }

      return SpecMenuItem.Submenu("&Magnus", items);
   }

   SpecMenuItem BuildNoteActionsSubmenu(JPNote note)
   {
      if(note is VocabNote vocab)
      {
         return _vocabNoteMenus.BuildNoteActionsMenuSpec(ShortcutFinger.Home3("Note actions"), vocab);
      } else if(note is KanjiNote kanji)
      {
         return _kanjiNoteMenus.BuildNoteActionsMenuSpec(ShortcutFinger.Home3("Note actions"), kanji);
      } else if(note is SentenceNote sentence)
      {
         return _sentenceNoteMenus.BuildNoteActionsMenuSpec(ShortcutFinger.Home3("Note actions"), sentence);
      }

      return SpecMenuItem.Submenu(ShortcutFinger.Home3("Note"), new List<SpecMenuItem>());
   }

   static SpecMenuItem BuildSpreadSubmenu(IReadOnlyList<long> cardIds)
   {
      var startDayMenus = new List<SpecMenuItem>();

      for(var startDay = 0; startDay <= 9; startDay++)
      {
         var daysApartItems = new List<SpecMenuItem>();

         for(var daysApart = 1; daysApart <= 9; daysApart++)
         {
            var currentStartDay = startDay;
            var currentDaysApart = daysApart;

            daysApartItems.Add(SpecMenuItem.Command($"{currentDaysApart} days apart", () => OnSpreadCards(cardIds, currentStartDay, currentDaysApart)));
         }

         startDayMenus.Add(SpecMenuItem.Submenu($"First card in {startDay} days", daysApartItems));
      }

      return SpecMenuItem.Submenu("&Spread selected cards", startDayMenus);
   }

   static void OnSpreadCards(IReadOnlyList<long> cardIds, int startDay, int daysApart) => AnkiFacade.Browser.MenuActions.SpreadCardsOverDays(cardIds, startDay, daysApart);

   void OnReparseSentences(List<SentenceNote> sentences)
   {
      _services.BackgroundTaskManager.Run(() =>
      {
         _services.LocalNoteUpdater.ReparseSentences(sentences);
         AnkiFacade.UIUtils.Refresh();
         AnkiFacade.UIUtils.ShowTooltip($"Reparsed {sentences.Count} sentence(s)");
      });
   }

   JPNote? GetNoteFromCardId(long cardId)
   {
      var ankiNoteId = AnkiFacade.GetNoteIdFromCardId(cardId);

      JPNote? vocab = _services.CoreApp.Collection.Vocab.WithExternalIdOrNone(ankiNoteId);
      if(vocab != null) return vocab;

      JPNote? sentence = _services.CoreApp.Collection.Sentences.WithExternalIdOrNone(ankiNoteId);
      if(sentence != null) return sentence;

      JPNote? kanji = _services.CoreApp.Collection.Kanji.WithExternalIdOrNone(ankiNoteId);
      return kanji;
   }

   List<SentenceNote> GetSentenceNotes(IReadOnlyList<long> ankiNoteIds)
   {
      var sentences = new List<SentenceNote>();

      foreach(var ankiNoteId in ankiNoteIds)
      {
         try
         {
            var note = _services.CoreApp.Collection.Sentences.WithExternalIdOrNone(ankiNoteId);
            if(note != null)
            {
               sentences.Add(note);
            }
         }
         catch(Exception ex)
         {
            this.Log().Info($"Failed to load note {ankiNoteId}: {ex.Message}");
         }
      }

      return sentences;
   }
}
