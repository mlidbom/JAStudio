using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Mcp.Tools.Corpus;

namespace JAStudio.Mcp.Tools.StudyNotes;

/// <summary>What <see cref="StudyNoteTools.WriteTheStudyNotesThatDiffer"/> reports: how long it took, the notes whose study-notes it wrote, by type, and how many study-notes differ afterwards.</summary>
public record StudyNotesWritten(double SecondsTaken, NotesOfOneType Vocab, NotesOfOneType Kanji, NotesOfOneType Sentences, int StudyNotesThatDifferAfterwards)
{
   /// <summary>The report of writing the study-notes of <paramref name="notesWritten"/>, listing at most <paramref name="maxNotesListedPerType"/> of each type.</summary>
   public static StudyNotesWritten Of(double secondsTaken, IReadOnlyList<JPNote> notesWritten, int maxNotesListedPerType, int studyNotesThatDifferAfterwards) =>
      new(secondsTaken,
          NotesOfOneType.Of(notesWritten.OfType<VocabNote>(), maxNotesListedPerType),
          NotesOfOneType.Of(notesWritten.OfType<KanjiNote>(), maxNotesListedPerType),
          NotesOfOneType.Of(notesWritten.OfType<SentenceNote>(), maxNotesListedPerType),
          studyNotesThatDifferAfterwards);
}
