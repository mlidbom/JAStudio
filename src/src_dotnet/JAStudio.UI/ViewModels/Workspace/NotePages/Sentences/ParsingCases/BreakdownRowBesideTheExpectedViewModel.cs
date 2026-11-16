using JAStudio.Core.Note.Sentences.ParsingCases.Breakdowns;

namespace JAStudio.UI.ViewModels.Workspace.NotePages.Sentences.ParsingCases;

/// <summary>
/// One row of a sentence's breakdown shown beside the one its parsing case expects (<see cref="BreakdownBesideTheExpected.Row"/>): the word the<br/>
/// sentence displays, as the breakdown lists it, the word expected, or both, and whether the two are the same word.
/// </summary>
/// <param name="Displayed">The word the sentence displays. Null in the row of a word that is expected and not displayed.</param>
/// <param name="Expected">The word the parsing case expects. Null in the row of a word that is displayed and not expected.</param>
/// <param name="IsTheSameWord">Whether the row is of a word displayed just as it's expected.</param>
sealed record BreakdownRowBesideTheExpectedViewModel(BreakdownEntryViewModel? Displayed, ExpectedBreakdownEntryViewModel? Expected, bool IsTheSameWord)
{
   /// <summary>Whether the row's two sides differ, which the page marks.</summary>
   public bool Differs => !IsTheSameWord;
}
