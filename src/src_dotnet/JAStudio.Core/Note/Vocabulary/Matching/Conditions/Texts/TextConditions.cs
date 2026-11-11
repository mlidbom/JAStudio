using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

/// <summary>
/// The conditions there are on a text, such as a word's surface or the text preceding it: comparisons with texts (<see cref="TextComparison"/>),<br/>
/// a regular expression (<see cref="TextMatchesRegularExpression"/>), and whether it's empty (<see cref="IsEmptyText"/>). Every comparison is ordinal:<br/>
/// exact, character by character.
/// </summary>
public static class TextConditions
{
   /// <summary>The text is empty, as the text preceding a word at the sentence's start is.</summary>
   public static readonly Fact<string> IsEmptyText = new("is empty", "empty", it => it.Length == 0);

   /// <summary>The conditions on a text, and how they're read back from the corpus.</summary>
   public static readonly ConditionLevel<string> Level =
      new("a text",
          [IsEmptyText],
          [],
          [
             ConditionTestWithTexts.Of("is one of", TextSet.HowToTypeThem, typed => new TextIsOneOf(TextSet.FromTyped(typed)), it => it.Texts.AsTyped),
             ConditionTestWithTexts.Of("starts with one of", TextSet.HowToTypeThem, typed => new TextStartsWithOneOf(TextSet.FromTyped(typed)), it => it.Texts.AsTyped),
             ConditionTestWithTexts.Of("ends with one of", TextSet.HowToTypeThem, typed => new TextEndsWithOneOf(TextSet.FromTyped(typed)), it => it.Texts.AsTyped),
             ConditionTestWithTexts.Of("contains one of", TextSet.HowToTypeThem, typed => new TextContainsOneOf(TextSet.FromTyped(typed)), it => it.Texts.AsTyped),
             ConditionTestWithTexts.Of("matches the regular expression",
                                       "A regular expression in .NET's syntax: ^ and $ anchor it to the whole text",
                                       typed => new TextMatchesRegularExpression(typed),
                                       it => it.Pattern)
          ],
          new Dictionary<string, Func<JsonNode, Condition<string>>>
          {
             [TextIsOneOf.JsonKey] = json => new TextIsOneOf(TextSet.From(json)),
             [TextStartsWithOneOf.JsonKey] = json => new TextStartsWithOneOf(TextSet.From(json)),
             [TextEndsWithOneOf.JsonKey] = json => new TextEndsWithOneOf(TextSet.From(json)),
             [TextContainsOneOf.JsonKey] = json => new TextContainsOneOf(TextSet.From(json)),
             [TextMatchesRegularExpression.JsonKey] = json => new TextMatchesRegularExpression(json.GetValue<string>())
          });

   /// <summary>A condition that the text is one of <paramref name="texts"/>.</summary>
   public static TextIsOneOf IsOneOf(params string[] texts) => new(new TextSet(texts));

   /// <summary>A condition that the text starts with one of <paramref name="texts"/>.</summary>
   public static TextStartsWithOneOf StartsWithOneOf(params string[] texts) => new(new TextSet(texts));

   /// <summary>A condition that the text ends with one of <paramref name="texts"/>.</summary>
   public static TextEndsWithOneOf EndsWithOneOf(params string[] texts) => new(new TextSet(texts));

   /// <summary>A condition that the text contains one of <paramref name="texts"/>.</summary>
   public static TextContainsOneOf ContainsOneOf(params string[] texts) => new(new TextSet(texts));

   /// <summary>A condition that the regular expression <paramref name="pattern"/> matches the text, or any part of it.</summary>
   public static TextMatchesRegularExpression MatchesRegularExpression(string pattern) => new(pattern);
}
