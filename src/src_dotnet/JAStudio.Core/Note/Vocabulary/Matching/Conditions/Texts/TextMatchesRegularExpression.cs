using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

/// <summary>
/// A condition that the regular expression <see cref="Pattern"/> matches a text, or any part of it. The pattern uses .NET's regular expression<br/>
/// syntax: <c>^</c> and <c>$</c> anchor it to the whole text. Made by <see cref="TextConditions.MatchesRegularExpression"/>.
/// </summary>
public sealed record TextMatchesRegularExpression : Condition<string>
{
   readonly Regex _regex;

   /// <exception cref="RegexParseException">When <paramref name="pattern"/> isn't a valid regular expression.</exception>
   internal TextMatchesRegularExpression(string pattern)
   {
      Pattern = pattern;
      _regex = new Regex(pattern, RegexOptions.CultureInvariant);
   }

   /// <summary>The regular expression, as it was written.</summary>
   public string Pattern { get; }

   public override bool IsMetBy(string subject) => _regex.IsMatch(subject);

   public override IEnumerable<string> DescriptionLines() => [$"matches the regular expression {Pattern}"];

   /// <summary>The key this condition is stored under in the corpus.</summary>
   internal const string JsonKey = "matchesRegularExpression";

   internal override JsonObject ToJson() => new() { [JsonKey] = Pattern };

   /// <summary>Equal when the patterns are, whatever the compiled <see cref="Regex"/> objects are.</summary>
   public bool Equals(TextMatchesRegularExpression? other) => other is not null && Pattern == other.Pattern;

   public override int GetHashCode() => Pattern.GetHashCode();
}
