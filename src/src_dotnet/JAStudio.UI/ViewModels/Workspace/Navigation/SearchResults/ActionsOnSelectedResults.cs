using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace.Tagging;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

/// <summary>
/// What can be done to the notes of the search results selected in the grid, all at once: adding or removing a tag,<br/>
/// and unsuspending or suspending all their cards. The selection's right-click menu offers them, and the grid's keys run some directly.
/// </summary>
/// <remarks>
/// Each runs in the background, with a progress dialog, a note at a time (<see cref="BulkNoteChanges"/>). The rows show each change as it's made,<br/>
/// and stay, even if the search would no longer find them.
/// </remarks>
class ActionsOnSelectedResults
{
   /// <summary>The key that adds a tag to the selected notes, as the menu shows it: T for tag, as everywhere in the search.</summary>
   public const string AddATagKey = "Ctrl+T";

   /// <summary>The key that removes a tag from the selected notes: Shift for the reverse of adding one.</summary>
   public const string RemoveATagKey = "Ctrl+Shift+T";

   readonly JPCollection _collection;
   readonly BulkNoteChanges _bulkNoteChanges;
   readonly IFatalErrorHandler _fatalErrorHandler;

   public ActionsOnSelectedResults(TemporaryServiceCollection services)
   {
      _collection = services.CoreApp.Collection;
      _bulkNoteChanges = new BulkNoteChanges(services);
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
   }

   /// <summary>The menu of the <paramref name="selected"/> results: what can be done to all their notes at once, each picked by a single key.</summary>
   /// <param name="selected">The results selected in the grid.</param>
   /// <param name="pickATagInADialog">Shows a tag picker dialog, and returns the tag path picked in it, or null when it was cancelled.</param>
   internal List<SpecMenuItem> Menu(IReadOnlyList<SearchResultViewModel> selected, Func<TagPickerDialogViewModel, Task<string?>> pickATagInADialog)
   {
      var notes = NotesOf(selected);
      var withSuspendedCards = notes.Where(it => it.HasSuspendedCards()).ToList();
      var withActiveCards = notes.Where(it => it.HasActiveCards()).ToList();
      return
      [
         SpecMenuItem.Command(ShortcutFinger.Mnemonic('a', $"Add a tag... ({AddATagKey})"), () => AddATagTo(selected, pickATagInADialog)),
         SpecMenuItem.Command(ShortcutFinger.Mnemonic('r', $"Remove a tag... ({RemoveATagKey})"), () => RemoveATagFrom(selected, pickATagInADialog),
                              enabled: notes.Any(it => it.Tags.NotMaintainedByJAStudio().Count > 0 || it.AnkiTags.Any())),
         SpecMenuItem.Separator(),
         SpecMenuItem.Command(ShortcutFinger.Mnemonic('u', "Unsuspend all cards"),
                              () => StartChangingEachOf(withSuspendedCards, "Unsuspending all cards", it => it.UnsuspendAllCards()),
                              enabled: withSuspendedCards.Count > 0),
         SpecMenuItem.Command(ShortcutFinger.Mnemonic('s', "Suspend all cards"),
                              () => StartChangingEachOf(withActiveCards, "Suspending all cards", it => it.SuspendAllCards()),
                              enabled: withActiveCards.Count > 0)
      ];
   }

   /// <summary>Picks a tag through <paramref name="pickATagInADialog"/>, any tag in use or a new one, and adds it to the notes of the <paramref name="selected"/> results.</summary>
   internal void AddATagTo(IReadOnlyList<SearchResultViewModel> selected, Func<TagPickerDialogViewModel, Task<string?>> pickATagInADialog)
   {
      var notes = NotesOf(selected);
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(await pickATagInADialog(TagPickerDialogViewModel.ForAddingATagTo(notes, _collection)) is not {} tagPath) return;

         await _bulkNoteChanges.AddTheTagTo(notes, Tag.FromName(tagPath));
      });
   }

   /// <summary>Picks one of the tags the notes of the <paramref name="selected"/> results have through <paramref name="pickATagInADialog"/>, and removes it from them.</summary>
   internal void RemoveATagFrom(IReadOnlyList<SearchResultViewModel> selected, Func<TagPickerDialogViewModel, Task<string?>> pickATagInADialog)
   {
      var notes = NotesOf(selected);
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(await pickATagInADialog(TagPickerDialogViewModel.ForRemovingATagFrom(notes)) is not {} tagPath) return;

         await _bulkNoteChanges.RemoveTheTagFrom(notes, Tag.FromName(tagPath));
      });
   }

   static List<JPNote> NotesOf(IReadOnlyList<SearchResultViewModel> selected) => [..selected.Select(it => it.Note)];

   void StartChangingEachOf(List<JPNote> notes, string whatIsDone, Action<JPNote> changeANote) =>
      _fatalErrorHandler.StartAndHandleExceptionsFrom(() => _bulkNoteChanges.ChangeEachOf(notes, whatIsDone, changeANote));
}
