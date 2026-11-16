using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Studying;

namespace JAStudio.Core.Note.Tagging;

/// <summary>
/// A note's Anki tags: the tags its study-note has in Anki, each below <c>anki::</c> (<see cref="Tags.Anki"/>), such as <c>anki::marked</c>.<br/>
/// They're the student's, stored only in Anki, so they're held as last read from Anki, by <see cref="WhatTheSRSHoldsOfTheStudyNotes"/>: as the collection<br/>
/// loads (<see cref="Collection.JPCollection.ReloadFromBackend"/>), and when it reads the study-notes from Anki again, as a sync with Anki does<br/>
/// (<see cref="JPCollection.ReadTheStudyNotesFromTheSRSAgain"/>); held in memory, never stored in the corpus. A person browses and searches them with<br/>
/// the note's own tags (<see cref="JPNote.AllTags"/>), and changes them in Anki (<see cref="AnkiTagChanges"/>), after which they're read again.
/// </summary>
public sealed class AnkiTags : IEnumerable<Tag>
{
   /// <summary>The Anki tags of a note whose study-note has none, or that has no study-note.</summary>
   internal static readonly AnkiTags None = new([]);

   readonly IReadOnlyList<Tag> _tags;

   /// <summary>The Anki tags of a study-note whose tags in Anki are <paramref name="namesInAnki"/>.</summary>
   internal AnkiTags(IEnumerable<string> namesInAnki) => _tags = [..namesInAnki.Select(Tags.Anki.For).Distinct()];

   /// <summary>Whether the note's study-note has <paramref name="tag"/>, an Anki tag such as <c>anki::marked</c>.</summary>
   public bool Contains(Tag tag) => _tags.Contains(tag);

   /// <summary>Whether these are the same tags, in the same order, as <paramref name="other"/>.</summary>
   internal bool AreTheSameAs(AnkiTags other) => _tags.SequenceEqual(other._tags);

   public IEnumerator<Tag> GetEnumerator() => _tags.GetEnumerator();

   IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
