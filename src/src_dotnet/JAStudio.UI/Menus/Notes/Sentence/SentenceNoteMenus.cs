using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using JAStudio.Anki;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views;

namespace JAStudio.UI.Menus.Notes.Sentence;

/// <summary>
/// Sentence note-specific menu builders.
/// Corresponds to notes/sentence/main.py in Python.
/// </summary>
class SentenceNoteMenus
{
   readonly Core.TemporaryServiceCollection _services;

   public SentenceNoteMenus(Core.TemporaryServiceCollection services) => _services = services;

   public SpecMenuItem BuildNoteActionsMenuSpec(SentenceNote sentence)
   {
      return SpecMenuItem.Submenu(
         ShortcutFinger.Home3("Note actions"),
         new List<SpecMenuItem>
         {
            BuildOpenMenuSpec(sentence),
            SpecMenuItem.Command(ShortcutFinger.Home2("Edit"), () => Dispatcher.UIThread.Invoke(() => new SentenceEditorDialog(sentence).ShowNearCursor())),
            BuildRemoveMenuSpec(sentence),
            BuildRemoveUserMenuSpec(sentence)
         }
      );
   }

   SpecMenuItem BuildOpenMenuSpec(SentenceNote sentence)
   {
      var items = new List<SpecMenuItem>
                  {
                     SpecMenuItem.Command(ShortcutFinger.Home1("Highlighted Vocab"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().VocabsLookupStrings(sentence.Configuration.HighlightedWords))),
                     SpecMenuItem.Command(ShortcutFinger.Home2("Highlighted Vocab Read Card"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().VocabsLookupStringsReadCard(sentence.Configuration.HighlightedWords))),
                     SpecMenuItem.Command(ShortcutFinger.Home3("Kanji"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().KanjiInString(string.Join("", sentence.ExtractKanji())))),
                     SpecMenuItem.Command(ShortcutFinger.Home4("Parsed words"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesByIds(GetParsedWordsNoteIds(sentence))))
                  };

      return SpecMenuItem.Submenu(ShortcutFinger.Home1("Open"), items);
   }

   static SpecMenuItem BuildRemoveMenuSpec(SentenceNote sentence)
   {
      var hasHighlightedWords = sentence.Configuration.HighlightedWords.Any();
      var hasIncorrectMatches = sentence.Configuration.IncorrectMatches.Get().Any();
      var hasHiddenMatches = sentence.Configuration.HiddenMatches.Get().Any();
      var hasSourceComments = sentence.SourceComments.HasValue();

      var items = new List<SpecMenuItem>
                  {
                     SpecMenuItem.Command(ShortcutFinger.Home1("All highlighted"), () => sentence.Configuration.ResetHighlightedWords(), enabled: hasHighlightedWords),
                     SpecMenuItem.Command(ShortcutFinger.Home2("All incorrect matches"), () => sentence.Configuration.IncorrectMatches.Reset(), enabled: hasIncorrectMatches),
                     SpecMenuItem.Command(ShortcutFinger.Home3("All hidden matches"), () => sentence.Configuration.HiddenMatches.Reset(), enabled: hasHiddenMatches),
                     SpecMenuItem.Command(ShortcutFinger.Home4("Source comments"), () => sentence.SourceComments.Empty(), enabled: hasSourceComments)
                  };

      return SpecMenuItem.Submenu(ShortcutFinger.Home2("Remove"), items);
   }

   static SpecMenuItem BuildRemoveUserMenuSpec(SentenceNote sentence)
   {
      var hasUserComments = sentence.User.Comments.HasValue();
      var hasUserAnswer = sentence.User.Answer.HasValue();
      var hasUserQuestion = sentence.User.Question.HasValue();

      var items = new List<SpecMenuItem>
                  {
                     SpecMenuItem.Command(ShortcutFinger.Home1("comments"), () => sentence.User.Comments.Empty(), enabled: hasUserComments),
                     SpecMenuItem.Command(ShortcutFinger.Home2("answer"), () => sentence.User.Answer.Empty(), enabled: hasUserAnswer),
                     SpecMenuItem.Command(ShortcutFinger.Home3("question"), () => sentence.User.Question.Empty(), enabled: hasUserQuestion)
                  };

      return SpecMenuItem.Submenu(ShortcutFinger.Home3("Remove User"), items);
   }

   public SpecMenuItem BuildViewMenuSpec()
   {
      // View menu with config toggles
      var config = _services.CoreApp.Config;
      var items = new List<SpecMenuItem>();

      // Add toggles for sentence view configuration
      for(var i = 0; i < config.SentenceViewToggles.Count; i++)
      {
         var toggle = config.SentenceViewToggles[i];
         items.Add(SpecMenuItem.Command(
                      ShortcutFinger.FingerByPriorityOrder(i, toggle.Title),
                      () => toggle.Value = !toggle.Value));
      }

      // Add the "Toggle all auto yield flags" action
      items.Add(SpecMenuItem.Command(
                   ShortcutFinger.FingerByPriorityOrder(items.Count, "Toggle all sentence auto yield compound last token flags (Ctrl+Shift+Alt+d)"),
                   () => config.ToggleAllSentenceDisplayAutoYieldFlags()));

      return SpecMenuItem.Submenu(ShortcutFinger.Home5("View"), items);
   }

   static IEnumerable<NoteId> GetParsedWordsNoteIds(SentenceNote sentence)
   {
      var parsingResult = sentence.GetParsingResult();
      var vocabIds = parsingResult.ParsedWords
                                  .Where(p => p.VocabId != null)
                                  .Select(p => p.VocabId!)
                                  .Distinct();
      return vocabIds;
   }
}
