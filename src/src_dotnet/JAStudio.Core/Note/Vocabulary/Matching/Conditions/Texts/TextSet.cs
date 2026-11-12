using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Contracts;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

/// <summary>
/// The texts a condition compares with, such as the endings in "ends with one of ん, い": each once, in ordinal order, so that two sets of the<br/>
/// same texts are equal however they were given. There's at least one, and none is empty.
/// </summary>
public sealed class TextSet : IEquatable<TextSet>
{
   const char TypedSeparator = ' ';

   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When there's no text, or one is empty.</exception>
   internal TextSet(IEnumerable<string> texts)
   {
      Texts = [.. texts.Distinct().Order(StringComparer.Ordinal)];
      var whyTheyMakeNoSet = WhyTheyMakeNoSet(Texts);
      Contract.Argument.Assert(whyTheyMakeNoSet is null, () => $"{whyTheyMakeNoSet}");
   }

   /// <summary>The texts, each once, in ordinal order.</summary>
   public IReadOnlyList<string> Texts { get; }

   /// <summary>The texts as C# string literals, separated by commas, as a condition's code has them: <c>"ん", "い"</c>.</summary>
   internal string AsStringLiterals => string.Join(", ", Texts.Select(Code.CodeWriting.StringLiteral));

   /// <summary>The texts as they're typed into the match-condition editor: separated by spaces. <see cref="FromTyped"/> reads them back.</summary>
   public string AsTyped => string.Join(TypedSeparator, Texts);

   /// <summary>The texts typed into the match-condition editor, separated by spaces, as <see cref="AsTyped"/> has them.</summary>
   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When <see cref="WhyTypedTextsMakeNoSet"/> says why they make none.</exception>
   internal static TextSet FromTyped(string typedTexts) => new(TheTextsTyped(typedTexts));

   /// <summary>Why the texts typed into the match-condition editor make no set, such as when nothing is typed, or null when they make one.</summary>
   internal static string? WhyTypedTextsMakeNoSet(string typedTexts) => WhyTheyMakeNoSet(TheTextsTyped(typedTexts));

   /// <summary>What to type for texts, as the match-condition editor tells it.</summary>
   internal const string HowToTypeThem = "The texts, separated by spaces";

   internal JsonArray ToJson() => [.. Texts.Select(JsonNode (it) => it)];

   static string[] TheTextsTyped(string typedTexts) => typedTexts.Split(TypedSeparator, StringSplitOptions.RemoveEmptyEntries);

   /// <summary>Why <paramref name="texts"/> make no set, or null when they make one: there's at least one, and none is empty.</summary>
   static string? WhyTheyMakeNoSet(IReadOnlyCollection<string> texts) =>
      texts.Count == 0                 ? "A condition needs at least one text to compare with."
      : texts.Any(it => it.Length == 0) ? "A condition can't compare with an empty text: every text starts with it, ends with it, and contains it."
                                        : null;

   /// <exception cref="FormatException">When <paramref name="json"/> isn't an array of texts.</exception>
   internal static TextSet From(JsonNode json)
   {
      var array = json as JsonArray ?? throw new FormatException($"Texts that aren't an array: {json.ToJsonString()}");
      return new TextSet(array.Select(TextIn));

      string TextIn(JsonNode? element) => element?.GetValue<string>() ?? throw new FormatException($"A null among texts: {json.ToJsonString()}");
   }

   public bool Equals(TextSet? other) => other is not null && Texts.SequenceEqual(other.Texts);

   public override bool Equals(object? obj) => Equals(obj as TextSet);

   public override int GetHashCode() => Texts.Aggregate(0, HashCode.Combine);

   public override string ToString() => string.Join(", ", Texts);
}
