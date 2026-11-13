using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>
/// A line of a condition's code as it's shown, as text or in pieces (<see cref="CodePiece"/>), laid out by a <see cref="CodeLayout"/>: one of its<br/>
/// <see cref="CodeLine"/>s, or two, when where the code is read the first condition of a group starts on the group's line.
/// </summary>
public sealed class ShownCodeLine
{
   ShownCodeLine(IReadOnlyList<CodePiece> pieces) => Pieces = pieces;

   /// <summary><paramref name="line"/> shown on a line of its own, starting at <paramref name="column"/>: indented by as many spaces.</summary>
   internal static ShownCodeLine StartingAt(int column, CodeLine line)
   {
      //review:matching: StartingAt and FollowedBy, for the two ways CodeLayout puts a CodeLine on screen: on a line of its own at a column, or after the
      //   parenthesis on the line before. CodeLayout built lists of pieces for both before, and wrapped one in a ShownCodeLine only to measure its Width.
      return new([.. IndentationTo(column), .. line.PiecesWithTheCommentApart]);
   }

   /// <summary>
   /// This line with <paramref name="line"/> after it on the same line, as where the code is read a group's first condition follows the group's<br/>
   /// parenthesis.
   /// </summary>
   internal ShownCodeLine FollowedBy(CodeLine line) => new([.. Pieces, .. line.PiecesWithTheCommentApart]);

   /// <summary>The line's pieces, in order: the spaces it's indented by first, if any, and a comment last, kept apart from the code before it.</summary>
   public IReadOnlyList<CodePiece> Pieces { get; }

   /// <summary>The line as text: its pieces joined.</summary>
   public string Text => string.Concat(Pieces.Select(it => it.Text));

   /// <summary>How wide the line is, in characters: the column right after it, such as after the parenthesis of a line that opens a group.</summary>
   public int Width => Text.Length;

   /// <summary>The spaces that indent a line to start at <paramref name="column"/>, as a piece of code: none at the first column.</summary>
   static IEnumerable<CodePiece> IndentationTo(int column) => column > 0 ? [CodePiece.Of(new string(' ', column))] : [];
}
