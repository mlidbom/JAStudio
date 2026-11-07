using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

/// <summary>A vocab a search found, as a row of the results grid shows it.</summary>
sealed class VocabSearchResultViewModel(VocabNote vocab) : SearchResultViewModel(vocab)
{
   /// <summary>The row's note, which is always a vocab, since the note it's replaced by has the same id.</summary>
   VocabNote Vocab => (VocabNote)Note;

   /// <summary>The vocab's readings, separated by commas.</summary>
   public string Readings => string.Join(", ", Vocab.GetReadings());
}
