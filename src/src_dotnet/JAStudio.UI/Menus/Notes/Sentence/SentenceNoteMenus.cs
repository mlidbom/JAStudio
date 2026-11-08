using Avalonia.Threading;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Anki;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Views.TextToSpeech;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI.Menus.Notes.Sentence;

class SentenceNoteMenus
{
   readonly Core.TemporaryServiceCollection _services;

   public SentenceNoteMenus(Core.TemporaryServiceCollection services) => _services = services;

   JPCollection Collection => _services.CoreApp.Collection;

   public SpecMenuItem BuildNoteActionsMenuSpec(string title, SentenceNote sentence)
   {
      return SpecMenuItem.Submenu(title,
                                  new List<SpecMenuItem>
                                  {
                                     SpecMenuItem.Submenu(ShortcutFinger.Home1("Open"),
                                                          new List<SpecMenuItem>
                                                          {
                                                             NoteListMenuItem.Showing(ShortcutFinger.Home1("Highlighted Vocab"), $"Highlighted vocab in {sentence.GetQuestion()}", () => Collection.Vocab.WithAnyFormIn(sentence.Configuration.HighlightedWords)),
                                                             NoteListMenuItem.Showing(ShortcutFinger.Home2("Kanji"), $"Kanji in {sentence.GetQuestion()}", () => Collection.Kanji.WithAnyKanjiIn(sentence.ExtractKanji())),
                                                             NoteListMenuItem.Showing(ShortcutFinger.Home3("Parsed words"), $"Parsed words in {sentence.GetQuestion()}", () => ParsedWordsNotes(sentence))
                                                          }),
                                     SpecMenuItem.Command(ShortcutFinger.Home2("Edit"), () => WorkspaceWindow.EditNote(sentence)),
                                     SpecMenuItem.Command(ShortcutFinger.Home5("Text-to-speech audio..."), () => Dispatcher.UIThread.Invoke(() => TextToSpeechDialog.OpenFor(sentence, _services))),
                                     SpecMenuItem.Submenu(ShortcutFinger.Home3("Remove"),
                                                          new List<SpecMenuItem>
                                                          {
                                                             SpecMenuItem.UICommand(ShortcutFinger.Home1("All highlighted"), () => sentence.Configuration.ResetHighlightedWords(), enabled: sentence.Configuration.HighlightedWords.Any()),
                                                             SpecMenuItem.UICommand(ShortcutFinger.Home2("All incorrect matches"), () => sentence.Configuration.IncorrectMatches.Reset(), enabled: sentence.Configuration.IncorrectMatches.Get().Any()),
                                                             SpecMenuItem.UICommand(ShortcutFinger.Home3("All hidden matches"), () => sentence.Configuration.HiddenMatches.Reset(), enabled: sentence.Configuration.HiddenMatches.Get().Any()),
                                                             SpecMenuItem.UICommand(ShortcutFinger.Home4("Source comments"), () => sentence.SourceData.Comments.Empty(), enabled: sentence.SourceData.Comments.HasValue())
                                                          }),
                                     SpecMenuItem.Submenu(ShortcutFinger.Home4("Remove corpus"),
                                                          new List<SpecMenuItem>
                                                          {
                                                             SpecMenuItem.UICommand(ShortcutFinger.Home1("comments"), () => sentence.Comments.Empty(), enabled: sentence.Comments.HasValue()),
                                                             SpecMenuItem.UICommand(ShortcutFinger.Home2("answer"), () => sentence.CorpusAnswer.Empty(), enabled: sentence.CorpusAnswer.HasValue()),
                                                             SpecMenuItem.UICommand(ShortcutFinger.Home3("question"), () => sentence.CorpusQuestion.Empty(), enabled: sentence.CorpusQuestion.HasValue())
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

   /// <summary>
   /// The vocab the sentence's parsing result matched. The parsing result is a cache, which can name a vocab deleted since the sentence<br/>
   /// was parsed: such a vocab has no note to list.
   /// </summary>
   IEnumerable<VocabNote> ParsedWordsNotes(SentenceNote sentence) =>
      sentence.GetParsingResult().ParsedWords
              .Select(it => it.VocabId)
              .OfType<NoteId>()
              .Distinct()
              .Select(Collection.Vocab.WithIdOrNone)
              .OfType<VocabNote>();
}
