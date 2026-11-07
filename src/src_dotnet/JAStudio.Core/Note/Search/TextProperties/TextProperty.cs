using System;

namespace JAStudio.Core.Note.Search.TextProperties;

/// <summary>
/// A property whose value is a text, such as a note's question.<br/>
/// The conditions on it compare that text, ignoring case: <see cref="Is"/>, <see cref="Contains"/>, <see cref="MatchesWildcards"/> and <see cref="MatchesRegex"/>.
/// </summary>
public sealed class TextProperty : NoteProperty
{
   readonly Func<JPNote, string> _textOf;

   internal TextProperty(string name, Func<JPNote, string> textOf) : base(name) => _textOf = textOf;

   /// <summary>This property's text for <paramref name="note"/>.</summary>
   internal string TextOf(JPNote note) => _textOf(note);

   /// <summary>A condition that the whole text is <paramref name="text"/>.</summary>
   public TextCondition Is(string text) => new(this, new TextIsExactly(text));

   /// <summary>A condition that the text contains <paramref name="text"/>.</summary>
   public TextCondition Contains(string text) => new(this, new TextContains(text));

   /// <summary>A condition that the whole text matches <paramref name="wildcardPattern"/>. See <see cref="TextMatchesWildcards"/>.</summary>
   public TextCondition MatchesWildcards(string wildcardPattern) => new(this, new TextMatchesWildcards(wildcardPattern));

   /// <summary>A condition that the text matches the regular expression <paramref name="regexPattern"/>. See <see cref="TextMatchesRegex"/>.</summary>
   public TextCondition MatchesRegex(string regexPattern) => new(this, new TextMatchesRegex(regexPattern));
}
