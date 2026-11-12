using System;
using System.Collections.Generic;
using JAStudio.Core.Note.ConditionTrees;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>
/// Where a condition is being written as code (<see cref="ConditionCode"/>): the position of the line it goes on and its own position in the tree, how deep<br/>
/// the line is, what goes in front of it on the line, such as the negations and subjects that hold it, and what goes after it, such as the comma<br/>
/// before the next condition of its group, or the parentheses that close the groups it ends.
/// </summary>
internal sealed record CodeWriting(ConditionPosition LinePosition, ConditionPosition Position, int Depth, IReadOnlyList<CodeText> InFront, string After)
{
   /// <summary>A whole condition, on lines of its own, with nothing in front or after it.</summary>
   internal static CodeWriting OfAWholeCondition { get; } = new(ConditionPosition.Root, ConditionPosition.Root, 0, [], "");

   /// <summary>The condition at <paramref name="index"/> in a vocab's match-condition: a statement of its own, with nothing after it.</summary>
   internal static CodeWriting OfAStatementOfAMatchCondition(int index) => new(ConditionPosition.Root.Child(index), ConditionPosition.Root.Child(index), 0, [], "");

   /// <summary>The condition that a negation or a condition on a subject holds: on the same line, with <paramref name="inFront"/> in front of it too.</summary>
   internal CodeWriting ForTheConditionHeldWith(CodeText inFront) => this with { Position = Position.Child(0), InFront = [.. InFront, inFront] };

   /// <summary>The condition at <paramref name="index"/> in a group: on a line of its own, one deeper, with <paramref name="after"/> after it.</summary>
   internal CodeWriting ForTheConditionInTheGroupAt(int index, string after) => new(Position.Child(index), Position.Child(index), Depth + 1, [], after);

   /// <summary>A condition's line, the whole of it: what's in front, <paramref name="code"/>, and what's after.</summary>
   internal CodeLine LineOf(string code) => new(LinePosition, Depth, [.. InFront, CodeText.Of(code + After)]);

   /// <summary>A group's line, which opens it with <paramref name="code"/>: what's after it goes after the group's last condition instead.</summary>
   internal CodeLine OpeningLineOf(string code) => new(LinePosition, Depth, [.. InFront, CodeText.Of(code)]);

   /// <summary>
   /// The lines of a condition that lists <paramref name="values"/> after <paramref name="head"/>, each with its English as a comment: all on one line when<br/>
   /// there's one, such as <c>PartOfSpeech.Is(名詞・数)  // noun › numeric</c>, and else one to a line under the head.
   /// </summary>
   internal IEnumerable<CodeLine> LinesListing(string head, IReadOnlyList<(string Value, string English)> values)
   {
      if(values.Count == 1)
      {
         yield return WithTheComment(LineOf($"{head}({values[0].Value})"), values[0].English);
         yield break;
      }

      yield return OpeningLineOf($"{head}(");
      for(var index = 0; index < values.Count; index++)
      {
         var (value, english) = values[index];
         var isTheLast = index == values.Count - 1;
         yield return new CodeLine(LinePosition, Depth + 1, [CodeText.Of(isTheLast ? $"{value}){After}" : $"{value},"), CodeText.Comment(english)]);
      }

      yield break;

      CodeLine WithTheComment(CodeLine line, string comment) => new(line.Position, line.Depth, [.. line.Texts, CodeText.Comment(comment)]);
   }

   /// <summary><paramref name="text"/> as a C# string literal, such as <c>"ん"</c>, with its quotes and backslashes escaped.</summary>
   internal static string StringLiteral(string text) => $"\"{text.Replace(@"\", @"\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

   /// <summary><paramref name="text"/> as a C# verbatim string literal, such as <c>@"^よ.$"</c>, as a regular expression is written: only its quotes are doubled.</summary>
   internal static string VerbatimStringLiteral(string text) => $"@\"{text.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
