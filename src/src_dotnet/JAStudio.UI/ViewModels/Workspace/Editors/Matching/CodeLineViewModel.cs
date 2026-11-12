using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// One line of code of a line in the tree editor (<see cref="ConditionTreeLineViewModel"/>), in pieces each shown as its kind should be<br/>
/// (<see cref="CodePieceViewModel"/>): indented under the first line of its condition as much deeper as it is, its comment after its code<br/>
/// (<see cref="CodeLine.PiecesShownUnder"/>).
/// </summary>
class CodeLineViewModel
{
   CodeLineViewModel(IReadOnlyList<CodePieceViewModel> pieces) => Pieces = pieces;

   /// <summary>The lines of code of a condition, each indented under the first, <paramref name="lines"/>[0], as much deeper as it is.</summary>
   public static IReadOnlyList<CodeLineViewModel> LinesOf(IReadOnlyList<CodeLine> lines) =>
      [.. lines.Select(line => new CodeLineViewModel([.. line.PiecesShownUnder(lines[0]).Select(CodePieceViewModel.Of)]))];

   public IReadOnlyList<CodePieceViewModel> Pieces { get; }
}
