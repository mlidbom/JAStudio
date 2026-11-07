using System;
using System.Collections.Generic;

namespace JAStudio.Core.Note.Search.TextProperties;

/// <summary>
/// A property whose value is one or more texts, such as a note's question, or the several texts of its searchable text.<br/>
/// The conditions on it compare each text, ignoring case, and match when any of them does:
/// <see cref="Is"/>, <see cref="Contains"/>, <see cref="MatchesWildcards"/> and <see cref="MatchesRegex"/>.
/// </summary>
public sealed class TextProperty : NoteProperty
{
   readonly Func<JPNote, IEnumerable<string>> _textsOf;

   internal TextProperty(string name, Func<JPNote, IEnumerable<string>> textsOf) : base(name) => _textsOf = textsOf;

   /// <summary>This property's texts for <paramref name="note"/>.</summary>
   internal IEnumerable<string> TextsOf(JPNote note) => _textsOf(note);

   /// <summary>A condition that a whole text is <paramref name="text"/>.</summary>
   public TextCondition Is(string text) => new(this, new TextIsExactly(text));

   /// <summary>A condition that a text contains <paramref name="text"/>.</summary>
   public TextCondition Contains(string text) => new(this, new TextContains(text));

   /// <summary>A condition that a whole text matches <paramref name="wildcardPattern"/>. See <see cref="TextMatchesWildcards"/>.</summary>
   public TextCondition MatchesWildcards(string wildcardPattern) => new(this, new TextMatchesWildcards(wildcardPattern));

   /// <summary>A condition that a text matches the regular expression <paramref name="regexPattern"/>. See <see cref="TextMatchesRegex"/>.</summary>
   public TextCondition MatchesRegex(string regexPattern) => new(this, new TextMatchesRegex(regexPattern));
}
