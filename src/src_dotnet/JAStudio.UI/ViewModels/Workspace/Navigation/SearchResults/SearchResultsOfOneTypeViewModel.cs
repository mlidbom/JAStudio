using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

/// <summary>
/// The notes of one note type that a search found, such as its sentences, shown in a grid with that type's columns.<br/>
/// When the search finds several types, each type gets a tab of its own. See <see cref="SearchResultsViewModel"/>.
/// </summary>
/// <remarks>
/// The rows are in <see cref="SearchResultsOfOneTypeViewModel{TNote,TSearchResult}.Results"/>, typed by note type, so the grid's columns know what they show.<br/>
/// They keep showing the same notes until the search changes or is refreshed, each row showing its note as it is now.
/// </remarks>
abstract partial class SearchResultsOfOneTypeViewModel : ObservableObject
{
   readonly WorkspaceViewModel _workspace;

   /// <param name="workspace">The workspace the notes open in.</param>
   /// <param name="noteType">The type's name, as its tab shows it, such as "Vocab".</param>
   protected SearchResultsOfOneTypeViewModel(WorkspaceViewModel workspace, string noteType)
   {
      _workspace = workspace;
      NoteType = noteType;
   }

   public string NoteType { get; }

   /// <summary>How many notes of this type are shown.</summary>
   public abstract int Count { get; }

   public bool HasResults => Count > 0;

   /// <summary>The type and how many notes of it are shown, as its tab shows it, such as "Vocab (12)".</summary>
   public string Header => $"{NoteType} ({Count})";

   /// <summary>Whether these are the results shown, the grid of the other types being hidden. Set by the <see cref="SearchResultsViewModel"/>.</summary>
   [ObservableProperty] bool _isShown;

   /// <summary>
   /// Shows the notes of this type among <paramref name="found"/>, the notes a changed search found, instead of the rows shown so far.<br/>
   /// It's a single change to the rows, so the grid sorts them all anew, and its selection is cleared.
   /// </summary>
   public abstract void Show(IReadOnlyList<JPNote> found);

   /// <summary>
   /// Shows the notes of this type among <paramref name="found"/>, the notes the same search found when it was refreshed:<br/>
   /// the rows of the notes still found stay, where they are, selected if they were. The rows of the notes no longer found go, and the newly found notes get rows.
   /// </summary>
   public abstract void ShowWhatTheRefreshedSearchFound(IReadOnlyList<JPNote> found);

   /// <summary>Makes the row of <paramref name="updated"/>, if there is one, show the note as it is now.</summary>
   public abstract void ShowTheChangeTo(JPNote updated);

   /// <summary>
   /// After <paramref name="collection"/> was reloaded, which replaces every note object: each row shows the object that now has its note's id,<br/>
   /// and the rows of notes that no longer exist go.
   /// </summary>
   public abstract void ShowTheReloadedNotesOf(JPCollection collection);

   /// <summary>Shows <paramref name="result"/>'s note in the workspace, in the tab already showing it or else in a new tab, as Enter or a double-click does.</summary>
   public void Open(SearchResultViewModel result) => _workspace.OpenNote(result.Note);

   /// <summary>Opens <paramref name="result"/>'s note in a new tab in the background, as middle-clicking a link does.</summary>
   public void OpenInBackgroundTab(SearchResultViewModel result) => _workspace.OpenNoteInBackgroundTab(result.Note);

   /// <summary>Called by the typed subclass when rows were added or removed, since what the tab shows depends on how many there are.</summary>
   protected void OnTheNumberOfRowsChanged()
   {
      OnPropertyChanged(nameof(Count));
      OnPropertyChanged(nameof(HasResults));
      OnPropertyChanged(nameof(Header));
   }
}
