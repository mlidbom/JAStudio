using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.UI.Views.Workspace.Navigation;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.UI.ViewModels.Workspace.Navigation;

/// <summary>
/// The "List" tab of a workspace's <see cref="NavigationPaneViewModel"/> (<see cref="NoteListView"/>): the <see cref="NoteList"/> shown last,<br/>
/// such as the sentences a vocab matches, shown by an "Open" item of a note menu. Each list shown replaces the one before.<br/>
/// The notes are ordered by their question, character by character.
/// </summary>
/// <remarks>
/// The same notes can be shown in Anki's browser, for what only Anki does with them, such as suspending or rescheduling their cards.
/// </remarks>
partial class NoteListViewModel : ObservableObject
{
   readonly TemporaryServiceCollection _services;

   NoteList _shownList = new("No list shown yet", []);

   public NoteListViewModel(WorkspaceViewModel workspace, TemporaryServiceCollection services)
   {
      _services = services;
      Notes = new ListedNotesViewModel(workspace);
      _ = new TeventsObservedOnTheUIThread(services, observe => observe.ForTevent<INoteTevent.Deleted>(it => StopListing(it.NoteId)));
   }

   /// <summary>What the listed notes are, with how many there are.</summary>
   [ObservableProperty] public partial string Title { get; set; } = "No list shown yet";

   public ListedNotesViewModel Notes { get; }

   /// <summary>The list the tab shows, with no notes before any is shown.</summary>
   internal NoteList ShownList => _shownList;

   /// <summary>Lists <paramref name="noteList"/>'s notes instead of the list shown so far.</summary>
   public void Show(NoteList noteList)
   {
      _shownList = noteList;
      Title = $"{noteList.Title} ({noteList.Notes.Count})";
      Notes.Show(noteList.Notes.OrderBy(it => it.GetQuestion(), StringComparer.Ordinal));
      ShowInAnkisBrowserCommand.NotifyCanExecuteChanged();
   }

   /// <summary>Takes the note <paramref name="deleted"/> off the list, if it's listed.</summary>
   void StopListing(NoteId deleted)
   {
      if(_shownList.Notes.Any(it => it.GetId().Equals(deleted)))
         Show(_shownList.Without(deleted));
   }

   /// <summary>Opens Anki's browser on the listed notes, the ones that are in Anki, and no others.</summary>
   [RelayCommand(CanExecute = nameof(AnyListedNoteIsInAnki))]
   void ShowInAnkisBrowser() => _services.SRSWindows.OpenTheBrowserOn(_services.StudyNoteIds.StudyNoteIdsOf(_shownList.Notes));

   /// <summary>
   /// A note that isn't in Anki has no study-note for Anki's browser to show. If no listed note is in Anki, the browser would have none to open on.
   /// </summary>
   bool AnyListedNoteIsInAnki() => _shownList.Notes.Any(it => _services.StudyNoteIds.StudyNoteIdOf(it.GetId()) != null);
}
