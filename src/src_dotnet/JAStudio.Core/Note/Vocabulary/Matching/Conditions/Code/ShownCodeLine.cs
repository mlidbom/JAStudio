using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>
/// A line of a condition's code as it's shown, as text or in pieces (<see cref="CodePiece"/>), laid out by a <see cref="CodeLayout"/>: one of its<br/>
/// <see cref="CodeLine"/>s, or two, when where the code is read the first condition of a group starts on the group's line.
/// </summary>
public sealed class ShownCodeLine
{
   internal ShownCodeLine(IReadOnlyList<CodePiece> pieces) => Pieces = pieces;

   /// <summary>The line's pieces, in order: the spaces it's indented by first, if any, and a comment last, kept apart from the code before it.</summary>
   public IReadOnlyList<CodePiece> Pieces { get; }

   /// <summary>The line as text: its pieces joined.</summary>
   public string Text => string.Concat(Pieces.Select(it => it.Text));

   /// <summary>How wide the line is, in characters: the column right after it, such as after the parenthesis of a line that opens a group.</summary>
   internal int Width => Text.Length;
}
