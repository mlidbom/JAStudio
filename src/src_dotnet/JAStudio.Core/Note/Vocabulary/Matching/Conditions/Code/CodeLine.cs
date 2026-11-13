using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.ConditionTrees;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>
/// One line of a condition written as code (<see cref="ConditionCode"/>), made of pieces of each kind (<see cref="Pieces"/>), with the <see cref="Position"/><br/>
/// in the tree of the condition it's a line of, and how deep in the code it is.
/// </summary>
/// <remarks>
/// A condition takes more than one line when it's a group, whose conditions go on lines of their own under it, or when it lists values with their<br/>
/// English, one to a line, such as <c>InflectedForm.In(</c> with <c>連用形,</c> and <c>連用タ接続)</c> under it.
/// </remarks>
public sealed class CodeLine
{
   //review:matching: CodeLine, CodePiece, CodePieceKind and CodeWriting repeat their namespace, Conditions.Code, which the naming rule says a type
   //   doesn't: Code.Line, Code.Piece, Code.PieceKind. Kept, since outside the namespace, as in the tree editor's view models, a bare Line or Piece
   //   says less. CodePiece was CodeText: every comment and the view models already called them pieces.

   /// <summary>What keeps a comment apart from the code before it: three spaces, not a column, since Japanese is wider than a monospace cell.</summary>
   const string BeforeTheComment = "   ";

   internal CodeLine(ConditionPosition position, int depth, IReadOnlyList<CodePiece> pieces)
   {
      Position = position;
      Depth = depth;
      Pieces = pieces;
   }

   /// <summary>
   /// Where the condition this is a line of is in the tree: the whole condition the tree editor shows on one line of its own, with the<br/>
   /// negations and subjects in front of it, such as <c>!PrecedingToken.IsAGodanVerb</c>.
   /// </summary>
   public ConditionPosition Position { get; }

   /// <summary>How deep the line is in the code: a group's conditions are one deeper than the group, and so are the values under a condition's first line.</summary>
   public int Depth { get; }

   /// <summary>The line's pieces, in order, such as <c>!</c>, <c>PrecedingToken.</c> and <c>IsAGodanVerb</c>.</summary>
   public IReadOnlyList<CodePiece> Pieces { get; }

   /// <summary>The line's code, without its comment.</summary>
   public string Code => string.Concat(Pieces.Where(it => it.Kind != CodePieceKind.Comment).Select(it => it.Text));

   /// <summary>
   /// Whether the line opens what the lines after it hold with a parenthesis, such as <c>AnyOf(</c> a group's conditions, or <c>InflectedForm.In(</c> a<br/>
   /// list's values: they're shown starting at the column right after it (<see cref="CodeLayout"/>).
   /// </summary>
   public bool OpensWhatFollows
   {
      get
      {
         //review:matching: read off the code's last character, which holds since a text is always a quoted literal, so only a line that opens
         //   something ends in an opening parenthesis. CodeWriting.OpeningLineOf, which writes every line that opens something, could instead say so on the line it makes, so that
         //   it's a fact of the line rather than of how its code happens to be written. Left as it is, since it's true now and simple.
         return Code.EndsWith('(');
      }
   }

   /// <summary>The line's pieces, with its comment kept apart from the code before it.</summary>
   internal IEnumerable<CodePiece> PiecesWithTheCommentApart
   {
      get
      {
         return Pieces.SelectMany(WithTheSpaceBeforeACommentAfterCode);

         static IEnumerable<CodePiece> WithTheSpaceBeforeACommentAfterCode(CodePiece piece, int index) =>
            piece.Kind == CodePieceKind.Comment && index > 0 ? [CodePiece.Of(BeforeTheComment), piece] : [piece];
      }
   }

   /// <summary>The line as text, its comment after its code.</summary>
   public override string ToString() => string.Concat(PiecesWithTheCommentApart.Select(it => it.Text));
}
