using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// A piece of a line of a condition's code (<see cref="CodeLineViewModel"/>), shown as its kind should be: a negation in a color that stands out, since a<br/>
/// <c>!</c> at the start of a line is easy to miss, a shared-condition's name in a color of its own, which shows its definition on hover, and a comment<br/>
/// dimmed.
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

   /// <summary>The shared-condition the piece names, when it's a shared-condition's name: what Ctrl+click on it opens.</summary>
   public ISharedCondition? SharedCondition => _piece.SharedCondition;

   /// <summary>
   /// The shared-condition the piece names, as code as it's defined, which hovering over the name shows: its full name, then its definition under it,<br/>
   /// such as <c>Word.FollowsAPastTenseStem =</c> then <c>AnyOf(PrecedingToken.IsAPastTenseStem,</c> and the rest. None when it names none.
   /// </summary>
   public string? SharedConditionAsDefined =>
      SharedCondition is {} sharedCondition ? ConditionCode.AsText(ConditionCode.LinesOfASharedCondition(sharedCondition)) : null;
}
