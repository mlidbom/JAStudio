using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Mcp.Tools.Corpus;

namespace JAStudio.Mcp.Tools.Reloading;

/// <summary>The notes whose files changed on disk while the collection held them, by note type, as reloading the collection finds them (<see cref="JPCollection.ReloadFromBackend"/>).</summary>
public record NotesChangedOnDisk(NotesOfOneType Vocab, NotesOfOneType Kanji, NotesOfOneType Sentences)
{
   /// <summary>The notes among <paramref name="notes"/> of each type, listing at most <paramref name="maxNotesListedPerType"/> of each.</summary>
   public static NotesChangedOnDisk Of(IReadOnlyList<JPNote> notes, int maxNotesListedPerType) =>
      new(NotesOfOneType.Of(notes.OfType<VocabNote>(), maxNotesListedPerType),
          NotesOfOneType.Of(notes.OfType<KanjiNote>(), maxNotesListedPerType),
          NotesOfOneType.Of(notes.OfType<SentenceNote>(), maxNotesListedPerType));
}
