using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Tagging;

/// <summary>
/// A note's Anki tags: the tags its study-note has in Anki, each below <c>anki::</c> (<see cref="Tags.Anki"/>), such as <c>anki::marked</c>.<br/>
/// They're the student's, so JAStudio only reads them: as the collection loads (<see cref="Collection.JPCollection.ReloadFromBackend"/>),<br/>
/// held in memory, never stored in the corpus and never changed. A person browses and searches them with the note's own tags (<see cref="JPNote.AllTags"/>).
/// </summary>
/// <remarks>A change made to them in Anki shows once the collection loads again: at startup, after a sync, or when reloaded by hand.</remarks>
public sealed class AnkiTags : IEnumerable<Tag>
{
   IReadOnlyList<Tag> _tags = [];

   /// <summary>Takes in <paramref name="namesInAnki"/>, the tags the note's study-note has in Anki, in place of those read before.</summary>
   internal void ReadFrom(IEnumerable<string> namesInAnki) => _tags = [..namesInAnki.Select(Tags.Anki.For).Distinct()];

   /// <summary>Forgets them, after the note's study-note was deleted in Anki.</summary>
   internal void Forget() => _tags = [];

   public IEnumerator<Tag> GetEnumerator() => _tags.GetEnumerator();

   IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
