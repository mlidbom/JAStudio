namespace JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;

/// <summary>
/// A reason a word of a sentence's breakdown isn't matched, or not shown, as the breakdown lists them above the word in edit mode, such as<br/>
/// <c>forbids::configured_hidden</c>.
/// </summary>
/// <param name="Text">The reason, as the match gives it.</param>
/// <param name="MakesTheMatchIncorrect">Whether the word doesn't match the vocab for it, rather than matching it and not being shown.</param>
sealed record MatchReasonViewModel(string Text, bool MakesTheMatchIncorrect)
{
   /// <summary>Whether it's the result of configuration, such as a vocab configured to be hidden in a sentence, rather than of the matching rules.</summary>
   public bool IsConfigured => Text.Contains("configured");
}
