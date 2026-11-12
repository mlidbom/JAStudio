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
   const RegexOptions TheRegexOptions = RegexOptions.CultureInvariant;
   readonly Regex _regex;

   /// <exception cref="RegexParseException">When <paramref name="pattern"/> isn't a valid regular expression.</exception>
   internal TextMatchesRegularExpression(string pattern)
   {
      Pattern = pattern;
      _regex = new Regex(pattern, TheRegexOptions);
   }

   /// <summary>Why <paramref name="pattern"/>, typed into the match-condition editor, is no valid regular expression, or null when it is one.</summary>
   /// <remarks>
   /// .NET has no way to check a pattern but to parse it, which throws <see cref="RegexParseException"/> for one that isn't valid. So that exception is<br/>
   /// caught here, around the parsing alone: a pattern being typed is invalid on its way to a valid one, and saying why is the point.
   /// </remarks>
   internal static string? WhyItIsNoRegularExpression(string pattern)
   {
      try
      {
         _ = new Regex(pattern, TheRegexOptions);
         return null;
      }
      catch(RegexParseException exception)
      {
         return exception.Message;
      }
   }

   /// <summary>The regular expression, as it was written.</summary>
   public string Pattern { get; }

   public override bool IsMetBy(string subject) => _regex.IsMatch(subject);

   /// <summary>The condition in code, the pattern a verbatim string literal as C# writes one: <c>MatchesRegularExpression(@"^よ.$")</c>.</summary>
   internal override IEnumerable<Code.CodeLine> CodeLines(Code.CodeWriting writing) => [writing.LineOf($"MatchesRegularExpression({Code.CodeWriting.VerbatimStringLiteral(Pattern)})")];

   /// <summary>The key this condition is stored under in the corpus.</summary>
   internal const string JsonKey = "matchesRegularExpression";

   internal override JsonObject ToJson() => new() { [JsonKey] = Pattern };

   /// <summary>Equal when the patterns are, whatever the compiled <see cref="Regex"/> objects are.</summary>
   public bool Equals(TextMatchesRegularExpression? other) => other is not null && Pattern == other.Pattern;

   public override int GetHashCode() => Pattern.GetHashCode();
}
