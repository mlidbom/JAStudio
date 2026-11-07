using System;
using System.Collections.Generic;
using JAStudio.Core.Note.Search.TextProperties;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.Search;

/// <summary>One of the ways a text condition can compare a text, as the text condition's editor offers them, such as "contains".</summary>
sealed class TextMatchChoice
{
   TextMatchChoice(char menuKey, string name, Func<TextProperty, string, TextCondition> makeCondition)
   {
      MenuKey = menuKey;
      Name = name;
      _makeCondition = makeCondition;
   }

   readonly Func<TextProperty, string, TextCondition> _makeCondition;

   public static readonly TextMatchChoice Contains = new('c', "contains", (property, text) => property.Contains(text));
   public static readonly TextMatchChoice IsExactly = new('e', "is exactly", (property, text) => property.Is(text));
   public static readonly TextMatchChoice MatchesWildcards = new('w', "matches the wildcard pattern", (property, pattern) => property.MatchesWildcards(pattern));
   public static readonly TextMatchChoice MatchesRegex = new('r', "matches the regular expression", (property, pattern) => property.MatchesRegex(pattern));

   /// <summary>Every choice, in the order the editor lists them.</summary>
   public static IReadOnlyList<TextMatchChoice> All { get; } = [Contains, IsExactly, MatchesWildcards, MatchesRegex];

   /// <summary>The choice as the editor lists it, such as "contains".</summary>
   public string Name { get; }

   /// <summary>The key that picks this choice in a text property's submenu of the search's add menu, such as c for "contains".</summary>
   public char MenuKey { get; }

   /// <summary>A condition that <paramref name="property"/>'s text matches <paramref name="text"/> in this way.</summary>
   /// <exception cref="System.Text.RegularExpressions.RegexParseException">When this is <see cref="MatchesRegex"/> and <paramref name="text"/> isn't a valid regular expression.</exception>
   public TextCondition MakeCondition(TextProperty property, string text) => _makeCondition(property, text);

   /// <summary>The choice that makes <paramref name="match"/>.</summary>
   public static TextMatchChoice For(TextMatch match) => match switch
   {
      TextContains => Contains,
      TextIsExactly => IsExactly,
      TextMatchesWildcards => MatchesWildcards,
      TextMatchesRegex => MatchesRegex,
      _ => throw new ArgumentOutOfRangeException(nameof(match), match, "The editor has no choice for this kind of text match.")
   };

   /// <summary>The text or pattern <paramref name="match"/> compares with.</summary>
   public static string TextOf(TextMatch match) => match switch
   {
      TextContains contains => contains.Text,
      TextIsExactly isExactly => isExactly.Text,
      TextMatchesWildcards wildcards => wildcards.Pattern,
      TextMatchesRegex regex => regex.Pattern,
      _ => throw new ArgumentOutOfRangeException(nameof(match), match, "The editor has no choice for this kind of text match.")
   };

   public override string ToString() => Name;
}
