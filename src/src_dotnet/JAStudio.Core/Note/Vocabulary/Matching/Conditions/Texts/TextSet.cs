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
   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When there's no text, or one is empty.</exception>
   internal TextSet(IEnumerable<string> texts)
   {
      Texts = [.. texts.Distinct().Order(StringComparer.Ordinal)];
      Contract.Argument.Assert(Texts.Count > 0, () => "A condition needs at least one text to compare with.");
      Contract.Argument.Assert(Texts.All(it => it.Length > 0), () => "A condition can't compare with an empty text: every text starts with it, ends with it, and contains it.");
   }

   /// <summary>The texts, each once, in ordinal order.</summary>
   public IReadOnlyList<string> Texts { get; }

   internal JsonArray ToJson() => [.. Texts.Select(JsonNode (it) => it)];

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
