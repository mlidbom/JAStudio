using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Mcp.Tools.Corpus;

namespace JAStudio.Mcp.Tools.Studying;

/// <summary>Some notes, by type: how many of each, and the first of each by question.</summary>
public record NotesByType(NotesOfOneType Vocab, NotesOfOneType Kanji, NotesOfOneType Sentences)
{
   /// <summary><paramref name="notes"/> by type, listing at most <paramref name="maxNotesListedPerType"/> of each.</summary>
   public static NotesByType Of(IReadOnlyList<JPNote> notes, int maxNotesListedPerType) =>
      new(NotesOfOneType.Of(notes.OfType<VocabNote>(), maxNotesListedPerType),
          NotesOfOneType.Of(notes.OfType<KanjiNote>(), maxNotesListedPerType),
          NotesOfOneType.Of(notes.OfType<SentenceNote>(), maxNotesListedPerType));
}
