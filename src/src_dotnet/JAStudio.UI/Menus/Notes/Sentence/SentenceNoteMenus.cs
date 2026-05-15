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

class SentenceNoteMenus
{
   readonly Core.TemporaryServiceCollection _services;

   public SentenceNoteMenus(Core.TemporaryServiceCollection services) => _services = services;

   public SpecMenuItem BuildNoteActionsMenuSpec(string title, SentenceNote sentence)
   {
      return SpecMenuItem.Submenu(title,
                                  new List<SpecMenuItem>
                                  {
                                     SpecMenuItem.Submenu(ShortcutFinger.Home1("Open"),
                                                          new List<SpecMenuItem>
                                                          {
                                                             SpecMenuItem.Command(ShortcutFinger.Home1("Highlighted Vocab"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().VocabsLookupStrings(sentence.Configuration.HighlightedWords))),
                                                             SpecMenuItem.Command(ShortcutFinger.Home2("Highlighted Vocab Read Card"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().VocabsLookupStringsReadCard(sentence.Configuration.HighlightedWords))),
                                                             SpecMenuItem.Command(ShortcutFinger.Home3("Kanji"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().KanjiInString(string.Join("", sentence.ExtractKanji())))),
                                                             SpecMenuItem.Command(ShortcutFinger.Home4("Parsed words"), () => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesByIds(GetParsedWordsNoteIds(sentence))))
                                                          }),
                                     SpecMenuItem.UICommand(ShortcutFinger.Home2("Edit"), () => Dispatcher.UIThread.Invoke(() => new SentenceEditorDialog(sentence).ShowNearCursor())),
                                     SpecMenuItem.Submenu(ShortcutFinger.Home3("Remove"),
                                                          new List<SpecMenuItem>
                                                          {
                                                             SpecMenuItem.UICommand(ShortcutFinger.Home1("All highlighted"), () => sentence.Configuration.ResetHighlightedWords(), enabled: sentence.Configuration.HighlightedWords.Any()),
                                                             SpecMenuItem.UICommand(ShortcutFinger.Home2("All incorrect matches"), () => sentence.Configuration.IncorrectMatches.Reset(), enabled: sentence.Configuration.IncorrectMatches.Get().Any()),
                                                             SpecMenuItem.UICommand(ShortcutFinger.Home3("All hidden matches"), () => sentence.Configuration.HiddenMatches.Reset(), enabled: sentence.Configuration.HiddenMatches.Get().Any()),
                                                             SpecMenuItem.UICommand(ShortcutFinger.Home4("Source comments"), () => sentence.SourceComments.Empty(), enabled: sentence.SourceComments.HasValue())
                                                          }),
                                     SpecMenuItem.Submenu(ShortcutFinger.Home4("Remove User"),
                                                          new List<SpecMenuItem>
                                                          {
                                                             SpecMenuItem.UICommand(ShortcutFinger.Home1("comments"), () => sentence.User.Comments.Empty(), enabled: sentence.User.Comments.HasValue()),
                                                             SpecMenuItem.UICommand(ShortcutFinger.Home2("answer"), () => sentence.User.Answer.Empty(), enabled: sentence.User.Answer.HasValue()),
                                                             SpecMenuItem.UICommand(ShortcutFinger.Home3("question"), () => sentence.User.Question.Empty(), enabled: sentence.User.Question.HasValue())
                                                          })
                                  }
      );
   }

   public SpecMenuItem BuildViewMenuSpec(string title)
   {
      // View menu with config toggles
      var config = _services.CoreApp.Config;
      var items = new List<SpecMenuItem>();

      // Add toggles for sentence view configuration
      for(var i = 0; i < config.SentenceViewToggles.Count; i++)
      {
         var toggle = config.SentenceViewToggles[i];
         items.Add(SpecMenuItem.UICommand(ShortcutFinger.FingerByPriorityOrder(i, toggle.Title), () => toggle.Value = !toggle.Value));
      }

      // Add the "Toggle all auto yield flags" action
      items.Add(SpecMenuItem.UICommand(ShortcutFinger.FingerByPriorityOrder(items.Count, "Toggle all sentence auto yield compound last token flags (Ctrl+Shift+Alt+d)"), () => config.ToggleAllSentenceDisplayAutoYieldFlags()));

      return SpecMenuItem.Submenu(title, items);
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
