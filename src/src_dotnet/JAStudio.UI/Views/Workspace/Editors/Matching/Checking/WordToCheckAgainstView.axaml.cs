using Avalonia.Controls;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching.Checking;

namespace JAStudio.UI.Views.Workspace.Editors.Matching.Checking;

/// <summary>
/// The view of a <see cref="WordToCheckAgainstViewModel"/>, in a vocab's <see cref="MatchConditionWindow"/>: the sentence to type or paste, and the<br/>
/// words in it that are one of the vocab's forms, to pick the one to check the match-condition against.
/// </summary>
public partial class WordToCheckAgainstView : UserControl
{
   public WordToCheckAgainstView() => InitializeComponent();
}
