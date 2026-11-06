using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Anki;
using JAStudio.Core;
using JAStudio.UI.Views.Workspace.Navigation;

namespace JAStudio.UI.ViewModels.Workspace.Navigation;

/// <summary>
/// The "List" tab of a workspace's <see cref="NavigationPaneViewModel"/> (<see cref="NoteListView"/>): the <see cref="NoteList"/> shown last,<br/>
/// such as the sentences a vocab matches, shown by an "Open" item of a note menu. Each list shown replaces the one before.
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
   }

   /// <summary>What the listed notes are, with how many there are.</summary>
   [ObservableProperty] string _title = "No list shown yet";

   public ListedNotesViewModel Notes { get; }

   /// <summary>Lists <paramref name="noteList"/>'s notes instead of the list shown so far.</summary>
   public void Show(NoteList noteList)
   {
      _shownList = noteList;
      Title = $"{noteList.Title} ({noteList.Notes.Count})";
      Notes.Show(noteList.Notes);
      ShowInAnkisBrowserCommand.NotifyCanExecuteChanged();
   }

   /// <summary>Opens Anki's browser on the listed notes, the ones that are in Anki, and no others.</summary>
   [RelayCommand(CanExecute = nameof(AnyListedNoteIsInAnki))]
   void ShowInAnkisBrowser() => AnkiFacade.Browser.ExecuteLookup(_services.QueryBuilder().NotesLookup(_shownList.Notes));

   /// <summary>
   /// A note that isn't in Anki has no Anki id to search for. If no listed note is in Anki, the search would be empty,<br/>
   /// and Anki's browser would show its default search instead of nothing.
   /// </summary>
   bool AnyListedNoteIsInAnki() => _shownList.Notes.Any(it => _services.ExternalNoteIdMap.ToExternalId(it.GetId()) != null);
}
