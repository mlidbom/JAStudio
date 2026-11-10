using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Tagging;

/// <summary>
/// A note's Anki tags: the tags its study-note has in Anki, each below <c>anki::</c> (<see cref="Tags.Anki"/>), such as <c>anki::marked</c>.<br/>
/// They're the student's, so JAStudio only reads them: as the collection loads (<see cref="Collection.JPCollection.ReloadFromBackend"/>), and when it reads<br/>
/// the study-notes from Anki again, as after an Anki operation that changed notes (<see cref="Collection.JPCollection.ReadTheStudyNotesFromAnkiAgain"/>);<br/>
/// held in memory, never stored in the corpus and never changed. A person browses and searches them with the note's own tags (<see cref="JPNote.AllTags"/>).
/// </summary>
public sealed class AnkiTags : IEnumerable<Tag>
{
   IReadOnlyList<Tag> _tags = [];

   /// <summary>Takes in <paramref name="namesInAnki"/>, the tags the note's study-note has in Anki, in place of those read before: whether they differ from those.</summary>
   internal bool ReadFrom(IEnumerable<string> namesInAnki)
   {
      var before = _tags;
      _tags = [..namesInAnki.Select(Tags.Anki.For).Distinct()];
      return !_tags.SequenceEqual(before);
   }

   /// <summary>The names of the tags in <paramref name="tagsAsAnkiStoresThem"/>, a study-note's tags as Anki stores them, separated by spaces.</summary>
   public static IReadOnlyList<string> NamesIn(string tagsAsAnkiStoresThem) => tagsAsAnkiStoresThem.Split(' ', StringSplitOptions.RemoveEmptyEntries);

   public IEnumerator<Tag> GetEnumerator() => _tags.GetEnumerator();

   IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
