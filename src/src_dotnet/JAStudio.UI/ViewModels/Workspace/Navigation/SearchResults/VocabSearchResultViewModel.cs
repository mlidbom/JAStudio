using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

/// <summary>A vocab a search found, as a row of the results grid shows it.</summary>
sealed class VocabSearchResultViewModel(VocabNote vocab) : SearchResultViewModel(vocab)
{
   /// <summary>The vocab's readings, separated by commas.</summary>
   public string Readings { get; } = string.Join(", ", vocab.GetReadings());
}
