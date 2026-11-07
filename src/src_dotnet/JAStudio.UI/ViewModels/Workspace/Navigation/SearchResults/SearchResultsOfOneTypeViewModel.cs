using CommunityToolkit.Mvvm.ComponentModel;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

/// <summary>
/// The notes of one note type that a search found, such as its sentences, shown in a grid with that type's columns.<br/>
/// When the search finds several types, each type gets a tab of its own. See <see cref="SearchResultsViewModel"/>.
/// </summary>
/// <remarks>The rows are in <see cref="SearchResultsOfOneTypeViewModel{TSearchResult}.Results"/>, typed by note type, so the grid's columns know what they show.</remarks>
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

   /// <summary>How many notes of this type the search found.</summary>
   public abstract int Count { get; }

   public bool HasResults => Count > 0;

   /// <summary>The type and how many notes of it were found, as its tab shows it, such as "Vocab (12)".</summary>
   public string Header => $"{NoteType} ({Count})";

   /// <summary>Whether these are the results shown, the grid of the other types being hidden. Set by the <see cref="SearchResultsViewModel"/>.</summary>
   [ObservableProperty] bool _isShown;

   /// <summary>Shows <paramref name="result"/>'s note in the workspace, in the tab already showing it or else in a new tab, as Enter or a double-click does.</summary>
   public void Open(SearchResultViewModel result) => _workspace.OpenNote(result.Note);

   /// <summary>Opens <paramref name="result"/>'s note in a new tab in the background, as middle-clicking a link does.</summary>
   public void OpenInBackgroundTab(SearchResultViewModel result) => _workspace.OpenNoteInBackgroundTab(result.Note);

   /// <summary>Called by the typed subclass when its rows are replaced, since what the tab shows depends on them.</summary>
   protected void OnResultsReplaced()
   {
      OnPropertyChanged(nameof(Count));
      OnPropertyChanged(nameof(HasResults));
      OnPropertyChanged(nameof(Header));
   }
}
