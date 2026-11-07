using System.Linq;
using System.Text.RegularExpressions;

namespace JAStudio.Core.Note.Search.TextProperties;

/// <summary>
/// A text matches when the whole of it matches <see cref="Pattern"/>, ignoring case.<br/>
/// In the pattern, <c>*</c> stands for any run of characters, none included, and <c>_</c> for exactly one character, as in Anki's searches.
/// </summary>
public sealed record TextMatchesWildcards : TextMatch
{
   const char AnyRun = '*';
   const char AnyOneCharacter = '_';

   readonly Regex _regex;

   internal TextMatchesWildcards(string pattern)
   {
      Pattern = pattern;
      _regex = new Regex(RegexMatchingTheWholeTextLike(pattern), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);
   }

   /// <summary>The pattern, as it was written.</summary>
   public string Pattern { get; }

   internal override bool IsMatchedBy(string text) => _regex.IsMatch(text);

   internal override string Description => $"matches the wildcard pattern \"{Pattern}\"";

   /// <summary>Equal when the patterns are, whatever the compiled <see cref="Regex"/> objects are.</summary>
   public bool Equals(TextMatchesWildcards? other) => other is not null && Pattern == other.Pattern;

   public override int GetHashCode() => Pattern.GetHashCode();

   static string RegexMatchingTheWholeTextLike(string pattern) => $"^{string.Concat(pattern.Select(RegexFor))}$";

   static string RegexFor(char patternCharacter) => patternCharacter switch
   {
      AnyRun => ".*",
      AnyOneCharacter => ".",
      _ => Regex.Escape(patternCharacter.ToString())
   };
}
