using JAStudio.Core.Note;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

/// <summary>A kanji a search found, as a row of the results grid shows it. Its <see cref="SearchResultViewModel.Question"/> is the kanji itself.</summary>
sealed class KanjiSearchResultViewModel(KanjiNote kanji) : SearchResultViewModel(kanji)
{
   /// <summary>The row's note, which is always a kanji, since the note it's replaced by has the same id.</summary>
   KanjiNote Kanji => (KanjiNote)Note;

   /// <summary>The kanji's on readings, separated by commas.</summary>
   public string OnReadings => string.Join(", ", Kanji.Readings.On.AllKana());

   /// <summary>The kanji's kun readings, separated by commas.</summary>
   public string KunReadings => string.Join(", ", Kanji.Readings.Kun.AllKana());
}
