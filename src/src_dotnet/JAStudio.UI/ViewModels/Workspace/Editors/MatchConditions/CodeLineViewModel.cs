using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

namespace JAStudio.UI.ViewModels.Workspace.Editors.MatchConditions;

/// <summary>
/// One line of code of a line in the match-condition's tree editor (<see cref="MatchConditionLineViewModel"/>), in pieces each shown as its kind should be<br/>
/// (<see cref="CodePieceViewModel"/>): indented under the first line of its condition as much deeper as it is, its comment after its code.
/// </summary>
class CodeLineViewModel
{
   const string Indentation = "   ";
   const string BeforeTheComment = "   ";

   CodeLineViewModel(IReadOnlyList<CodePieceViewModel> pieces) => Pieces = pieces;

   /// <summary>The lines of code of a condition, each indented under the first, <paramref name="lines"/>[0], as much deeper as it is.</summary>
   public static IReadOnlyList<CodeLineViewModel> LinesOf(IReadOnlyList<CodeLine> lines) => [.. lines.Select(line => Of(line, line.Depth - lines[0].Depth))];

   public IReadOnlyList<CodePieceViewModel> Pieces { get; }

   static CodeLineViewModel Of(CodeLine line, int deeperThanTheFirstLine)
   {
      return new([..TheIndentation(), ..line.Texts.SelectMany(TheTextWithTheSpaceBeforeACommentAfterCode)]);

      IEnumerable<CodePieceViewModel> TheIndentation() => deeperThanTheFirstLine > 0 ? [CodePieceViewModel.Spaces(string.Concat(Enumerable.Repeat(Indentation, deeperThanTheFirstLine)))] : [];

      IEnumerable<CodePieceViewModel> TheTextWithTheSpaceBeforeACommentAfterCode(CodeText text, int index) =>
         text.Kind == CodeTextKind.Comment && index > 0 ? [CodePieceViewModel.Spaces(BeforeTheComment), CodePieceViewModel.Of(text)] : [CodePieceViewModel.Of(text)];
   }
}
