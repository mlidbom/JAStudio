namespace JAStudio.Core.Note.Search.TextProperties;

/// <summary>
/// How a <see cref="TextCondition"/> compares a property's text: <see cref="TextIsExactly"/>, <see cref="TextContains"/>,<br/>
/// <see cref="TextMatchesWildcards"/> or <see cref="TextMatchesRegex"/>. All of them ignore case.
/// </summary>
public abstract record TextMatch
{
   /// <summary>Whether <paramref name="text"/> matches.</summary>
   internal abstract bool IsMatchedBy(string text);

   /// <summary>The comparison as the search editor shows it after the property's name, such as <c>contains "行く"</c>.</summary>
   internal abstract string Description { get; }
}
