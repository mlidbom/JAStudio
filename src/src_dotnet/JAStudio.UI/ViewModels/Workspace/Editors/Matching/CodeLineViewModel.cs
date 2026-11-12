using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// One line of a condition's code as it's shown (<see cref="ShownCodeLine"/>), in pieces each shown as its kind should be (<see cref="CodePieceViewModel"/>),<br/>
/// such as a line in the tree editor (<see cref="ConditionTreeLineViewModel"/>), or of a condition's code shown read-only (<see cref="ConditionCodeViewModel"/>).
/// </summary>
class CodeLineViewModel
{
   CodeLineViewModel(IReadOnlyList<CodePieceViewModel> pieces) => Pieces = pieces;

   /// <summary><paramref name="lines"/>, the lines of a condition's code, laid out by <paramref name="layout"/>, starting at <paramref name="column"/>.</summary>
   public static IReadOnlyList<CodeLineViewModel> LinesOf(IReadOnlyList<CodeLine> lines, CodeLayout layout, int column = 0) =>
      [.. layout.LinesShowing(lines, column).Select(line => new CodeLineViewModel([.. line.Pieces.Select(CodePieceViewModel.Of)]))];

   public IReadOnlyList<CodePieceViewModel> Pieces { get; }

   /// <summary>The line as text: its pieces joined.</summary>
   public string Text => string.Concat(Pieces.Select(it => it.Text));
}
