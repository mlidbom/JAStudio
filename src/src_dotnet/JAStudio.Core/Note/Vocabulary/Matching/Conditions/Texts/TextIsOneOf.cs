using System;
using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

/// <summary>A condition that a text is one of the <see cref="TextComparison.Texts"/>. Made by <see cref="TextConditions.IsOneOf"/>.</summary>
public sealed record TextIsOneOf : TextComparison
{
   internal TextIsOneOf(TextSet texts) : base(texts) {}

   protected override bool Compares(string subject, string text) => string.Equals(subject, text, StringComparison.Ordinal);

   protected override string Description => "is one of";

   /// <summary>The key this comparison is stored under in the corpus.</summary>
   internal const string JsonKey = "isOneOf";

   internal override JsonObject ToJson() => ToJsonUnder(JsonKey);
}
