using System;
using System.Collections.Generic;
using Avalonia.Threading;
using JAStudio.Anki;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views;

namespace JAStudio.UI.Menus;

/// <summary>
/// Builds the main "Japanese" menu for Anki.
/// This will replace the Python menu in tools_menu.py.
/// Now uses UI-agnostic MenuItem specifications and AnkiFacade for Anki calls.
/// </summary>
public class JapaneseMainMenu
{
   readonly Core.TemporaryServiceCollection _services;
   readonly OpenInAnkiMenus _openInAnkiMenus;

   public JapaneseMainMenu(Core.TemporaryServiceCollection services)
   {
      _services = services;
      _openInAnkiMenus = new OpenInAnkiMenus(_services);
   }

   // ReSharper disable once UnusedMember.Global used from python
   public List<SpecMenuItem> BuildMenuSpec(Func<string> getClipboardContent) =>
   [
      SpecMenuItem.Command(ShortcutFinger.Home4("JAStudio Browser"), () => Dispatcher.UIThread.Invoke(() => Views.MainWindow.ToggleVisibility(_services))),
      SpecMenuItem.Submenu(ShortcutFinger.Home1("Config"),
                           new List<SpecMenuItem>
                           {
                              SpecMenuItem.Command(ShortcutFinger.Home1("Options (Ctrl+Shift+S)"), () => Dispatcher.UIThread.Invoke(() => new OptionsDialog(_services).ShowNearCursor())),
                              SpecMenuItem.Command(ShortcutFinger.Home2("Readings mappings (Ctrl+Shift+M)"), () => Dispatcher.UIThread.Invoke(() => new ReadingsMappingsDialog(_services).ShowNearCursor())),
                              SpecMenuItem.Command(ShortcutFinger.Home3("Media import"), () => Dispatcher.UIThread.Invoke(() => new MediaImportDialog(_services).ShowNearCursor()))
                           }
      ),
      SpecMenuItem.Submenu(ShortcutFinger.Home2("Lookup"),
                           new List<SpecMenuItem>
                           {
                              SpecMenuItem.Command(ShortcutFinger.Home1("Open note (Ctrl+O)"), () => Dispatcher.UIThread.Invoke(() => NoteSearchDialog.ToggleVisibility(_services))),
                              _openInAnkiMenus.BuildOpenInAnkiMenuSpec(ShortcutFinger.Home2("Anki"), getClipboardContent),
                              WebSearchMenuBuilder.BuildWebSearchMenu(ShortcutFinger.Home3("Web"), getClipboardContent)
                           }
      ),
      SpecMenuItem.Submenu(ShortcutFinger.Home3("Local Actions"),
                           new List<SpecMenuItem>
                           {
                              SpecMenuItem.Submenu(ShortcutFinger.Home1("Update"),
                                                   new List<SpecMenuItem>
                                                   {
                                                      BatchCommand(ShortcutFinger.Home1("Vocab"), _services.LocalNoteUpdater.UpdateVocab),
                                                      BatchCommand(ShortcutFinger.Home2("Kanji"), _services.LocalNoteUpdater.UpdateKanji),
                                                      BatchCommand(ShortcutFinger.Home3("Sentences"), _services.LocalNoteUpdater.UpdateSentences),
                                                      BatchCommand(ShortcutFinger.Home4("Tag note metadata"), _services.LocalNoteUpdater.TagNoteMetadata),
                                                      BatchCommand(ShortcutFinger.Home5("All the above"), _services.LocalNoteUpdater.UpdateAll),
                                                      BatchCommand(ShortcutFinger.Up1("Reparse sentences"), _services.LocalNoteUpdater.ReparseAllSentences),
                                                      BatchCommand(ShortcutFinger.Down1("All the above: Full rebuild"), _services.LocalNoteUpdater.FullRebuild)
                                                   }
                              ),
                              BatchCommand(ShortcutFinger.Home2("Convert Immersion Kit sentences"), AnkiFacade.Batches.ConvertImmersionKitSentences),
                              BatchCommand(ShortcutFinger.Home3("Update everything except reanalysing sentences"), _services.LocalNoteUpdater.UpdateAll),
                              BatchCommand(ShortcutFinger.Home4("Create vocab notes for parsed words"), _services.LocalNoteUpdater.CreateMissingVocabWithDictionaryEntries),
                              BatchCommand(ShortcutFinger.Home5("Regenerate vocab source answers from dictionary"), _services.LocalNoteUpdater.RegenerateJamdictVocabAnswers),
                              BatchCommand(ShortcutFinger.Up1("Force flush all cached notes"), _services.LocalNoteUpdater.ForceFlushAllNotes),
                              BatchCommand(ShortcutFinger.Up2("Force flush all Anki notes by ID"), FlushAllAnkiNotesById),
                              BatchCommand(ShortcutFinger.Up3("Write file system repository"), _services.LocalNoteUpdater.WriteFileSystemRepository)
                           }
      )
   ];

   SpecMenuItem BatchCommand(string name, Action action, char? acceleratorKey = null, string? shortcut = null, bool enabled = true)
      => SpecMenuItem.Command(name,
                              () => _services.BackgroundTaskManager.Run(() =>
                              {
                                 action();
                                 AnkiFacade.UIUtils.Refresh();
                              }),
                              acceleratorKey,
                              shortcut,
                              enabled);

   void FlushAllAnkiNotesById()
   {
      using var scope = _services.TaskRunner.Current("Flushing all Anki notes by ID");
      var externalIds = _services.ExternalNoteIdMap.AllExternalIds();
      scope.RunBatch(externalIds, AnkiFacade.Batches.FlushAnkiNote, "Flushing Anki notes");
   }
}
