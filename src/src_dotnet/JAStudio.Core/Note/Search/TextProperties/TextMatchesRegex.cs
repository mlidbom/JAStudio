using System.Text.RegularExpressions;

namespace JAStudio.Core.Note.Search.TextProperties;

/// <summary>
/// A text matches when the regular expression <see cref="Pattern"/> matches it, or any part of it, ignoring case.<br/>
/// The pattern uses .NET's regular expression syntax. <c>^</c> and <c>$</c> anchor it to the whole text.
/// </summary>
public sealed record TextMatchesRegex : TextMatch
{
   readonly Regex _regex;

   /// <exception cref="RegexParseException">When <paramref name="pattern"/> isn't a valid regular expression.</exception>
   internal TextMatchesRegex(string pattern)
   {
      Pattern = pattern;
      _regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
   }

   /// <summary>The regular expression, as it was written.</summary>
   public string Pattern { get; }

   internal override bool IsMatchedBy(string text) => _regex.IsMatch(text);

   internal override string Description => $"matches the regular expression \"{Pattern}\"";

   /// <summary>Equal when the patterns are, whatever the compiled <see cref="Regex"/> objects are.</summary>
   public bool Equals(TextMatchesRegex? other) => other is not null && Pattern == other.Pattern;

   public override int GetHashCode() => Pattern.GetHashCode();
}
