using System;
using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

/// <summary>A condition that a text contains one of the <see cref="TextComparison.Texts"/>. Made by <see cref="TextConditions.ContainsOneOf"/>.</summary>
public sealed record TextContainsOneOf : TextComparison
{
   internal TextContainsOneOf(TextSet texts) : base(texts) {}

   protected override bool Compares(string subject, string text) => subject.Contains(text, StringComparison.Ordinal);

   protected override string Name => "ContainsOneOf";

   /// <summary>The key this comparison is stored under in the corpus.</summary>
   internal const string JsonKey = "containsOneOf";

   internal override JsonObject ToJson() => ToJsonUnder(JsonKey);
}
