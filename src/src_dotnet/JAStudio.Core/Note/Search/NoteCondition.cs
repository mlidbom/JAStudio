using System;
using System.Collections.Generic;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search.Grouping;

namespace JAStudio.Core.Note.Search;

/// <summary>
/// A condition that a note either matches or doesn't, such as "the question contains 行く".<br/>
/// A whole search is one: conditions combine into a tree through <see cref="ConditionGroup"/>, and the tree's root is the search.<br/>
/// <see cref="JPCollection.NotesMatching"/> finds the notes that match one.
/// </summary>
/// <remarks>
/// Conditions are values: two built from the same parts, in the same order, are equal.<br/>
/// So a spec can compare the search an operation produced with one written out, and an undo can keep earlier searches.<br/>
/// See <c>dev_docs/note-search.md</c>.
/// </remarks>
public abstract record NoteCondition
{
   const string Indentation = "   ";

   /// <summary>Whether <paramref name="note"/> matches this condition. An empty condition (<see cref="IsEmpty"/>) matches every note.</summary>
   public abstract bool Matches(JPNote note);

   /// <summary>
   /// Whether this condition adds nothing to a search: a group with no conditions in it, or only empty ones, or the negation of one.<br/>
   /// The group it's in ignores it, and an empty search finds every note.
   /// </summary>
   public virtual bool IsEmpty => false;

   /// <summary>The condition as the search editor shows it: one line per condition, with each group's conditions indented under it.</summary>
   public abstract IEnumerable<string> DescriptionLines();

   public sealed override string ToString() => string.Join(Environment.NewLine, DescriptionLines());

   /// <summary><paramref name="line"/> indented one level, for a condition shown under the group it's in.</summary>
   protected static string Indented(string line) => Indentation + line;
}
