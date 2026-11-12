using System;
using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

/// <summary>A condition that a text starts with one of the <see cref="TextComparison.Texts"/>. Made by <see cref="TextConditions.StartsWithOneOf"/>.</summary>
public sealed record TextStartsWithOneOf : TextComparison
{
   internal TextStartsWithOneOf(TextSet texts) : base(texts) {}

   protected override bool Compares(string subject, string text) => subject.StartsWith(text, StringComparison.Ordinal);

   protected override string Name => "StartsWithOneOf";

   /// <summary>The key this comparison is stored under in the corpus.</summary>
   internal const string JsonKey = "startsWithOneOf";

   internal override JsonObject ToJson() => ToJsonUnder(JsonKey);
}
