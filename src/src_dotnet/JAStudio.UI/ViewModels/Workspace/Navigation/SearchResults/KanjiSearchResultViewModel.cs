using JAStudio.Core.Note;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

/// <summary>A kanji a search found, as a row of the results grid shows it. Its <see cref="SearchResultViewModel.Question"/> is the kanji itself.</summary>
sealed class KanjiSearchResultViewModel(KanjiNote kanji) : SearchResultViewModel(kanji)
{
   /// <summary>The kanji's on readings, separated by commas.</summary>
   public string OnReadings { get; } = string.Join(", ", kanji.ReadingsOn);

   /// <summary>The kanji's kun readings, separated by commas.</summary>
   public string KunReadings { get; } = string.Join(", ", kanji.ReadingsKun);
}
