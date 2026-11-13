using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>
/// Conditions written as code, as the tree editor, the failure reasons and the specifications show them, such as<br/>
/// <c>!PrecedingToken.Surface.EndsWithOneOf("ん")</c> (Magnus, 2026-11-12: "it is a logical boolean condition. I would like it to display very similarly<br/>
/// to how such a condition displays in a programming language").
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>The dots go from the word to what's tested: <c>PrecedingToken.Surface.EndsWithOneOf("ん")</c>.</item>
/// <item>Groups are <c>AllOf(...)</c> and <c>AnyOf(...)</c>, with their conditions on lines of their own, commas between them and the closing parenthesis<br/>
/// after the last, each starting at the column right after the group's parenthesis (<see cref="CodeLayout"/>). A negation is <c>!</c>.</item>
/// <item>Texts are C# string literals. The tokenizer's values, such as parts of speech, are unquoted, in Japanese, with their English as a comment, one to<br/>
/// a line when there are several, laid out as a group's conditions are.</item>
/// <item>A vocab's match-condition is its conditions, each a statement of its own that must hold, under a comment heading: its "all of" isn't written.<br/>
/// So is a shared-condition's definition, as the tree editor shows it in the shared-condition's window.</item>
/// </list>
/// See <c>dev_docs/matching-configuration.md</c>, "How conditions display: as code".
/// </remarks>
public static class ConditionCode
{
   /// <summary>The comment heading a vocab's match-condition, on the line of the match-condition itself.</summary>
   public const string MatchConditionHeading = "A word matches the vocab when each of these holds:";

   /// <summary>
   /// The lines of a vocab's <paramref name="matchCondition"/>: <see cref="MatchConditionHeading"/> as a comment, on the line of its root, then each of its<br/>
   /// conditions, a statement of its own.
   /// </summary>
   public static IReadOnlyList<CodeLine> LinesOfAMatchCondition(AllOfGroup<CandidateWordVariant> matchCondition) => LinesOfStatementsUnder(MatchConditionHeading, matchCondition);

   /// <summary>
   /// The comment heading <paramref name="sharedCondition"/>'s definition, as the tree editor shows it in the shared-condition's window, such as<br/>
   /// "A word meets Word.FollowsAPastTenseStem when each of these holds:".
   /// </summary>
   public static string HeadingOfTheDefinitionOf(ISharedCondition sharedCondition) =>
      $"{AtTheStartOfASentence(sharedCondition.Level.SubjectName)} meets {sharedCondition.FullName} when each of these holds:";

   /// <summary><paramref name="text"/> with a capital first letter, as at the start of a sentence: "A word" for "a word".</summary>
   static string AtTheStartOfASentence(string text) => $"{char.ToUpperInvariant(text[0])}{text[1..]}";

   /// <summary>
   /// The lines of <paramref name="sharedCondition"/>'s definition as the tree editor shows it, <paramref name="definitionAsAnAllOf"/>: the definition's<br/>
   /// conditions when it's an "all of" group, else the definition alone. <see cref="HeadingOfTheDefinitionOf"/> as a comment on the root's line, then each<br/>
   /// condition a statement of its own, as a vocab's match-condition is written.
   /// </summary>
   public static IReadOnlyList<CodeLine> LinesOfADefinitionAsStatements(ISharedCondition sharedCondition, IConditionGroup definitionAsAnAllOf) =>
      LinesOfStatementsUnder(HeadingOfTheDefinitionOf(sharedCondition), definitionAsAnAllOf);

   /// <summary><paramref name="heading"/> as a comment, on the line of the root, <paramref name="allOf"/>, then each of its conditions, a statement of its own.</summary>
   static IReadOnlyList<CodeLine> LinesOfStatementsUnder(string heading, IConditionGroup allOf) =>
   [
      new(ConditionPosition.Root, 0, [CodePiece.Comment(heading)]),
      ..allOf.Conditions.SelectMany((condition, index) => condition.CodeLines(CodeWriting.OfAStatement(index)))
   ];

   /// <summary>
   /// The lines of <paramref name="sharedCondition"/> as it's defined: its full name, then its definition under it, such as<br/>
   /// <c>Word.FollowsAPastTenseStem =</c> then <c>AnyOf(PrecedingToken.IsAPastTenseStem,</c> and the rest, one deeper.
   /// </summary>
   public static IReadOnlyList<CodeLine> LinesOfASharedCondition(ISharedCondition sharedCondition) =>
   [
      new(ConditionPosition.Root, 0, [CodePiece.FullNameOf(sharedCondition), CodePiece.Of(" =")]),
      ..sharedCondition.Definition.CodeLines(CodeWriting.OfADefinition)
   ];

   /// <summary>The lines of <paramref name="condition"/> as a whole: a group's conditions on lines of their own.</summary>
   public static IReadOnlyList<CodeLine> LinesOf(Condition condition) => [.. condition.CodeLines(CodeWriting.OfAWholeCondition)];

   /// <summary>
   /// <paramref name="condition"/> on one line, without the comments, as a failure reason shows it: its lines joined, a space after each comma, such as<br/>
   /// <c>AnyOf(!Surface.In("本の"), FollowingToken.PartOfSpeech.Is(名詞・数))</c>.
   /// </summary>
   public static string OnOneLine(Condition condition) => OnOneLine(LinesOf(condition));

   /// <summary>
   /// <paramref name="lines"/>, the lines of a condition, on one line, without their comments: joined, a space before each but the first and those right<br/>
   /// after the parenthesis of a line that opens them (<see cref="CodeLine.OpensWhatFollows"/>).
   /// </summary>
   public static string OnOneLine(IReadOnlyList<CodeLine> lines)
   {
      return string.Concat(lines.Select((line, index) => SpaceBeforeTheLineAt(index) + line.Code));

      string SpaceBeforeTheLineAt(int index) => index == 0 || lines[index - 1].OpensWhatFollows ? "" : " ";
   }

   /// <summary>
   /// <paramref name="lines"/>, the lines of a condition, as text, as it's read (<see cref="CodeLayout.ForReading"/>): the first condition of a group on its<br/>
   /// line, after the parenthesis, the others under it, and each comment after its code.
   /// </summary>
   public static string AsText(IReadOnlyList<CodeLine> lines) => AsText(lines, CodeLayout.ForReading);

   /// <summary><paramref name="lines"/>, the lines of a condition, as text, laid out by <paramref name="layout"/>, starting at <paramref name="column"/>.</summary>
   public static string AsText(IReadOnlyList<CodeLine> lines, CodeLayout layout, int column = 0) =>
      string.Join(Environment.NewLine, layout.LinesShowing(lines, column).Select(it => it.Text));
}
