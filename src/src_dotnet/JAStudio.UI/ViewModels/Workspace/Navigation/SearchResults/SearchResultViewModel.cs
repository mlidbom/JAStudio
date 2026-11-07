using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Compze.Contracts;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.SysUtils;
using JAStudio.UI.Utils;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;

/// <summary>
/// A note a search found, as a row of the results grid shows it: a value for each column, without markup.<br/>
/// Each note type has its own, with columns of its own, such as <see cref="SentenceSearchResultViewModel"/>.
/// </summary>
/// <remarks>
/// The row always shows the note as it is now. Its values are read from the note whenever the grid asks for them,<br/>
/// and <see cref="ShowTheCurrentDataOf"/> tells the grid to ask again after the note changed.
/// </remarks>
abstract class SearchResultViewModel(JPNote note) : ObservableObject
{
   public JPNote Note { get; private set; } = note;

   public string Question => Note.GetQuestion().StripHtmlAndBracketMarkupAndNoiseCharacters();
   public string Answer => Note.GetAnswer().StripHtmlAndBracketMarkupAndNoiseCharacters();

   /// <summary>Whether the note's reading is being studied, as the search's "Studying reading" property says.</summary>
   public bool IsStudyingReading => Note.IsStudyingRead();

   /// <summary>Whether the note's listening is being studied, as the search's "Studying listening" property says.</summary>
   public bool IsStudyingListening => Note.IsStudyingListening();

   /// <summary>
   /// The note's tags other than the jastudio-defined-tags (<see cref="NoteTags.NotDefinedByJAStudio"/>), separated by spaces, as Anki shows them.<br/>
   /// Sorting by them compares the numbers in them as numbers, so the tag of season 2 comes before that of season 10.
   /// </summary>
   public TextSortedWithNumbersAsNumbers TagsNotDefinedByJAStudio => new(string.Join(" ", Note.Tags.NotDefinedByJAStudio().Select(it => it.Name)));

   /// <summary>
   /// Makes the row show <paramref name="note"/> as it is now: the row's own note after it changed,<br/>
   /// or the object that replaced it when the collection was reloaded after an Anki sync, which has the same id.
   /// </summary>
   public void ShowTheCurrentDataOf(JPNote note)
   {
      Contract.Argument.Assert(note.GetId() == Note.GetId(), () => $"A row shows one note, {Note.GetId()}, and can't be made to show the note {note.GetId()}.");
      Note = note;
      TellTheGridThatEveryValueChanged();
   }

   /// <summary>An empty property name says that every property may have changed, so the grid reads every value of the row again.</summary>
   void TellTheGridThatEveryValueChanged() => OnPropertyChanged(string.Empty);
}
