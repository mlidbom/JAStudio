using System;
using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

/// <summary>A condition that a text is one of the <see cref="TextComparison.Texts"/>. Made by <see cref="TextConditions.In"/>.</summary>
public sealed record TextIsIn : TextComparison
{
   internal TextIsIn(TextSet texts) : base(texts) {}

   protected override bool Compares(string subject, string text) => string.Equals(subject, text, StringComparison.Ordinal);

   protected override string Name => "In";

   /// <summary>
   /// The key this comparison is stored under in the corpus. It keeps the name the comparison had before its code was written <c>In</c>: a new key would change<br/>
   /// every note that stores one, and the stored notes are migrated only when there's more to gain.
   /// </summary>
   internal const string JsonKey = "isOneOf";

   internal override JsonObject ToJson() => ToJsonUnder(JsonKey);
}
