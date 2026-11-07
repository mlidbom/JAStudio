using System.Collections.Generic;
using JAStudio.UI.Utils;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

/// <summary>The notes of one note type that a search found, as rows of the type <typeparamref name="TSearchResult"/>, such as <see cref="VocabSearchResultViewModel"/>.</summary>
/// <remarks>
/// The rows are replaced in place, in the same <see cref="Results"/> collection, so the grid keeps the sort it has when the search changes,<br/>
/// and sorts the new rows the same way.
/// </remarks>
sealed class SearchResultsOfOneTypeViewModel<TSearchResult>(WorkspaceViewModel workspace, string noteType)
   : SearchResultsOfOneTypeViewModel(workspace, noteType) where TSearchResult : SearchResultViewModel
{
   public ReplaceableObservableCollection<TSearchResult> Results { get; } = [];

   public override int Count => Results.Count;

   /// <summary>Shows <paramref name="results"/> instead of the rows shown so far.</summary>
   public void Show(IEnumerable<TSearchResult> results)
   {
      Results.ReplaceAllWith(results);
      OnResultsReplaced();
   }
}
