using System.Collections.Generic;
using JAStudio.Core.Note.ConditionTrees;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>
/// Where a condition is being written as code (<see cref="ConditionCode"/>): the position of the line it goes on and its own position in the tree, how deep<br/>
/// the line is, what goes in front of it on the line, such as the negations and subjects that hold it, and what goes after it, such as the comma<br/>
/// before the next condition of its group, or the parentheses that close the groups it ends.
/// </summary>
sealed record CodeWriting(ConditionPosition LinePosition, ConditionPosition Position, int Depth, IReadOnlyList<CodePiece> InFront, string After)
{
   //review:matching: "CodeWriting", for where a condition is being written as code and what surrounds it there. "CodeContext" was the other candidate.
   /// <summary>A whole condition, on lines of its own, with nothing in front or after it.</summary>
   internal static CodeWriting OfAWholeCondition { get; } = new(ConditionPosition.Root, ConditionPosition.Root, 0, [], "");

   /// <summary>The definition of a shared-condition, under the line that names it: one deeper, with nothing in front or after it.</summary>
   internal static CodeWriting OfADefinition { get; } = new(ConditionPosition.Root, ConditionPosition.Root, 1, [], "");

   /// <summary>The condition at <paramref name="index"/> in a vocab's match-condition: a statement of its own, with nothing after it.</summary>
   internal static CodeWriting OfAStatementOfAMatchCondition(int index) => new(ConditionPosition.Root.Child(index), ConditionPosition.Root.Child(index), 0, [], "");

   /// <summary>The condition that a negation or a condition on a subject holds: on the same line, with <paramref name="inFront"/> in front of it too.</summary>
   internal CodeWriting ForTheConditionHeldWith(CodePiece inFront) => this with { Position = Position.Child(0), InFront = [.. InFront, inFront] };

   /// <summary>The condition at <paramref name="index"/> in a group: on a line of its own, one deeper, with <paramref name="after"/> after it.</summary>
   internal CodeWriting ForTheConditionInTheGroupAt(int index, string after) => new(Position.Child(index), Position.Child(index), Depth + 1, [], after);

   /// <summary>A condition's line, the whole of it: what's in front, <paramref name="code"/>, and what's after.</summary>
   internal CodeLine LineOf(string code) => new(LinePosition, Depth, [.. InFront, CodePiece.Of(code + After)]);

   /// <summary>The line of a shared-condition that a condition uses: what's in front, its <paramref name="name"/>, and what's after.</summary>
   internal CodeLine LineOfASharedCondition(string name) =>
      new(LinePosition, Depth, [.. InFront, CodePiece.SharedConditionName(name), .. PiecesAfter]);

   /// <summary>What goes after the condition, as a piece of code of its own: none when nothing does.</summary>
   IEnumerable<CodePiece> PiecesAfter => After.Length > 0 ? [CodePiece.Of(After)] : [];

   /// <summary>A group's line, which opens it with <paramref name="code"/>: what's after it goes after the group's last condition instead.</summary>
   internal CodeLine OpeningLineOf(string code) => new(LinePosition, Depth, [.. InFront, CodePiece.Of(code)]);

   /// <summary>
   /// The lines of a condition that lists <paramref name="values"/> after <paramref name="head"/>, each with its English as a comment: all on one line when<br/>
   /// there's one, such as <c>PartOfSpeech.Is(名詞・数)  // noun › numeric</c>, and else one to a line under the head.
   /// </summary>
   internal IEnumerable<CodeLine> LinesListing(string head, IReadOnlyList<TokenizerValue> values)
   {
      if(values.Count == 1)
      {
         yield return WithTheComment(LineOf($"{head}({values[0].Value})"), values[0].English);
         yield break;
      }

      yield return OpeningLineOf($"{head}(");
      for(var index = 0; index < values.Count; index++)
      {
         yield return new CodeLine(LinePosition, Depth + 1, [CodePiece.Of(values[index].Value + AfterTheValueAt(index)), CodePiece.Comment(values[index].English)]);
      }

      yield break;

      CodeLine WithTheComment(CodeLine line, string comment) => new(line.Position, line.Depth, [.. line.Pieces, CodePiece.Comment(comment)]);

      string AfterTheValueAt(int index) => index < values.Count - 1 ? "," : ")" + After;
   }
}
