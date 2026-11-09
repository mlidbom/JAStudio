using System;
using System.Collections.Generic;
using JAStudio.Anki;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI.Menus;

/// <summary>
/// The workspace's main menu, its menu bar, with the keyboard shortcuts the workspace window binds: the way to everything in JAStudio<br/>
/// besides the notes, such as the options, the import profiles and the batch updates.
/// </summary>
/// <remarks>
/// It was also Anki's "Japanese" menu until 2026-11-08. Anki now only toggles the workspace, or opens it on its search, from its JA Studio menu<br/>
/// (<see cref="AnkiMenus.BuildJAStudioMenuSpec"/>) or with Ctrl+Shift+O and Ctrl+O.
/// </remarks>
public class WorkspaceMainMenu
{
   readonly Core.TemporaryServiceCollection _services;

   public WorkspaceMainMenu(Core.TemporaryServiceCollection services) => _services = services;

   public List<SpecMenuItem> BuildMenuSpec(Func<string> getClipboardContent) =>
   [
      SpecMenuItem.Submenu(ShortcutFinger.Home1("Config"),
                           new List<SpecMenuItem>
                           {
                              SpecMenuItem.Command(ShortcutFinger.Home1(MainMenuWindows.Options.Name), () => MainMenuWindows.Options.Open(_services), shortcut: "Ctrl+Shift+s"),
                              SpecMenuItem.Command(ShortcutFinger.Home2(MainMenuWindows.ReadingsMappings.Name), () => MainMenuWindows.ReadingsMappings.Open(_services), shortcut: "Ctrl+Shift+m"),
                              SpecMenuItem.Command(ShortcutFinger.Home3(MainMenuWindows.TextToSpeechVoices.Name), () => MainMenuWindows.TextToSpeechVoices.Open(_services))
                           }
      ),
      SpecMenuItem.Submenu(ShortcutFinger.Home2("Lookup"),
                           new List<SpecMenuItem>
                           {
                              SpecMenuItem.Command(ShortcutFinger.Home1("Search notes (Ctrl+O)"), WorkspaceWindow.ToggleNoteSearch),
                              WebSearchMenuBuilder.BuildWebSearchMenu(ShortcutFinger.Home2("Web"), getClipboardContent)
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
                              SpecMenuItem.Command(ShortcutFinger.Home2(MainMenuWindows.ImportProfiles.Name), () => MainMenuWindows.ImportProfiles.Open(_services)),
                              BatchCommand(ShortcutFinger.Home3("Update everything except reanalysing sentences"), _services.LocalNoteUpdater.UpdateAll),
                              BatchCommand(ShortcutFinger.Home4("Create vocab notes for parsed words"), _services.LocalNoteUpdater.CreateMissingVocabWithDictionaryEntries),
                              BatchCommand(ShortcutFinger.Home5("Regenerate vocab source answers from dictionary"), _services.LocalNoteUpdater.RegenerateJamdictVocabAnswers),
                              BatchCommand(ShortcutFinger.Up1("Force flush all cached notes"), _services.LocalNoteUpdater.ForceFlushAllNotes),
                              BatchCommand(ShortcutFinger.Up2("Reload the collection from disk"), () => _services.CoreApp.Collection.ReloadFromBackend()),
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
}
