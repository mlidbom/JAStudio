using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>
/// Conditions written as code, as the match-condition editor, the failure reasons and the specifications show them, such as<br/>
/// <c>!PrecedingToken.Surface.EndsWithOneOf("ん")</c> (Magnus, 2026-11-12: "it is a logical boolean condition. I would like it to display very similarly<br/>
/// to how such a condition displays in a programming language").
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>The dots go from the word to what's tested: <c>PrecedingToken.Surface.EndsWithOneOf("ん")</c>.</item>
/// <item>Groups are <c>AllOf(...)</c> and <c>AnyOf(...)</c>, with their conditions on lines of their own, commas between them and the closing parenthesis<br/>
/// after the last. A negation is <c>!</c>.</item>
/// <item>Texts are C# string literals. The tokenizer's values, such as parts of speech, are unquoted, in Japanese, with their English as a comment, one to<br/>
/// a line when there are several.</item>
/// <item>A vocab's match-condition is its conditions, each a statement of its own that must hold, under a comment heading: its "all of" isn't written.</item>
/// </list>
/// See <c>dev_docs/matching-configuration.md</c>, "How conditions display: as code".
/// </remarks>
public static class ConditionCode
{
   const string Indentation = "   ";

   /// <summary>The comment heading a vocab's match-condition, on the line of the match-condition itself.</summary>
   public const string MatchConditionHeading = "A word matches the vocab when each of these holds:";

   /// <summary>
   /// The lines of a vocab's <paramref name="matchCondition"/>: <see cref="MatchConditionHeading"/> as a comment, on the line of its root, then each of its<br/>
   /// conditions, a statement of its own.
   /// </summary>
   public static IReadOnlyList<CodeLine> LinesOfAMatchCondition(AllOfGroup<CandidateWordVariant> matchCondition) =>
   [
      new(ConditionPosition.Root, 0, [CodeText.Comment(MatchConditionHeading)]),
      ..matchCondition.Conditions.SelectMany((condition, index) => condition.CodeLines(CodeWriting.OfAStatementOfAMatchCondition(index)))
   ];

   /// <summary>The lines of <paramref name="condition"/> as a whole: a group's conditions on lines of their own.</summary>
   public static IReadOnlyList<CodeLine> LinesOf(Condition condition) => [.. condition.CodeLines(CodeWriting.OfAWholeCondition)];

   /// <summary>
   /// <paramref name="condition"/> on one line, without the comments, as a failure reason shows it: its lines joined, a space after each comma, such as<br/>
   /// <c>AnyOf(!Surface.In("本の"), FollowingToken.PartOfSpeech.Is(名詞・数))</c>.
   /// </summary>
   public static string OnOneLine(Condition condition)
   {
      var code = new StringBuilder();
      foreach(var line in LinesOf(condition).Select(it => it.Code))
      {
         if(code.Length > 0 && code[^1] != '(') code.Append(' ');
         code.Append(line);
      }

      return code.ToString();
   }

   /// <summary><paramref name="lines"/> as text, each indented as much deeper as it is than the first, its comment after its code.</summary>
   public static string AsText(IReadOnlyList<CodeLine> lines) =>
      string.Join(Environment.NewLine, lines.Select(it => string.Concat(Enumerable.Repeat(Indentation, it.Depth - lines[0].Depth)) + it));
}
