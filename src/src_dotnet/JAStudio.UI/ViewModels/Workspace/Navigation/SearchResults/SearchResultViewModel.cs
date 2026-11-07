using JAStudio.Core.Note;
using JAStudio.Core.SysUtils;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

/// <summary>
/// A note a search found, as a row of the results grid shows it: a value for each column, without markup.<br/>
/// Each note type has its own, with columns of its own, such as <see cref="SentenceSearchResultViewModel"/>.
/// </summary>
/// <remarks>The values are read once, when the search finds the note. Running the search again shows any change.</remarks>
abstract class SearchResultViewModel
{
   protected SearchResultViewModel(JPNote note)
   {
      Note = note;
      Question = note.GetQuestion().StripHtmlAndBracketMarkupAndNoiseCharacters();
      Answer = note.GetAnswer().StripHtmlAndBracketMarkupAndNoiseCharacters();
      IsStudyingReading = note.IsStudyingRead();
      IsStudyingListening = note.IsStudyingListening();
   }

   public JPNote Note { get; }
   public string Question { get; }
   public string Answer { get; }

   /// <summary>Whether the note's reading is being studied, as the search's "Studying reading" property says.</summary>
   public bool IsStudyingReading { get; }

   /// <summary>Whether the note's listening is being studied, as the search's "Studying listening" property says.</summary>
   public bool IsStudyingListening { get; }
}
