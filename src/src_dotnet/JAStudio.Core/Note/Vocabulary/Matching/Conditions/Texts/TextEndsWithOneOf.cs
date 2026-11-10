using System;
using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

/// <summary>A condition that a text ends with one of the <see cref="TextComparison.Texts"/>. Made by <see cref="TextConditions.EndsWithOneOf"/>.</summary>
public sealed record TextEndsWithOneOf : TextComparison
{
   internal TextEndsWithOneOf(TextSet texts) : base(texts) {}

   protected override bool Compares(string subject, string text) => subject.EndsWith(text, StringComparison.Ordinal);

   protected override string Description => "ends with one of";

   /// <summary>The key this comparison is stored under in the corpus.</summary>
   internal const string JsonKey = "endsWithOneOf";

   internal override JsonObject ToJson() => ToJsonUnder(JsonKey);
}
