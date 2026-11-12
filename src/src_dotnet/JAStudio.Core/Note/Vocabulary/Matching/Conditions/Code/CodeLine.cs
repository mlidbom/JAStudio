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

   /// <summary>What a line is indented by for each level it's deeper than the first line of its condition.</summary>
   const string Indentation = "   ";

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
   /// The line's pieces as they're shown under <paramref name="firstLine"/>, the first line of its condition: indented as much deeper as it is, and its<br/>
   /// comment kept apart from the code before it.
   /// </summary>
   public IReadOnlyList<CodePiece> PiecesShownUnder(CodeLine firstLine)
   {
      return [.. TheIndentation(), .. Pieces.SelectMany(WithTheSpaceBeforeACommentAfterCode)];

      IEnumerable<CodePiece> TheIndentation() =>
         Depth > firstLine.Depth ? [CodePiece.Of(string.Concat(Enumerable.Repeat(Indentation, Depth - firstLine.Depth)))] : [];

      static IEnumerable<CodePiece> WithTheSpaceBeforeACommentAfterCode(CodePiece piece, int index) =>
         piece.Kind == CodePieceKind.Comment && index > 0 ? [CodePiece.Of(BeforeTheComment), piece] : [piece];
   }

   /// <summary>The line as text under <paramref name="firstLine"/>, the first line of its condition: its <see cref="PiecesShownUnder"/> it, joined.</summary>
   public string TextUnder(CodeLine firstLine) => string.Concat(PiecesShownUnder(firstLine).Select(it => it.Text));

   /// <summary>The line as text, its comment after its code.</summary>
   public override string ToString() => TextUnder(this);
}
