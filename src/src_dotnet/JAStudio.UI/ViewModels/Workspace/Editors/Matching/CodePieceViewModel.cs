using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// A piece of a line of code in the tree editor (<see cref="CodeLineViewModel"/>), shown as its kind should be: a negation in a color<br/>
/// that stands out, since a <c>!</c> at the start of a line is easy to miss, a shared-condition's name in a color of its own, and a comment dimmed.
/// </summary>
class CodePieceViewModel
{
   readonly CodePiece _piece;

   CodePieceViewModel(CodePiece piece) => _piece = piece;

   /// <summary>The piece of the line from a condition's code.</summary>
   public static CodePieceViewModel Of(CodePiece piece) => new(piece);

   public string Text => _piece.Text;

   public bool IsNegation => _piece.Kind == CodePieceKind.Negation;

   public bool IsComment => _piece.Kind == CodePieceKind.Comment;

   public bool IsSharedConditionName => _piece.Kind == CodePieceKind.SharedConditionName;
}
