using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki.StudyNotes;
using JAStudio.Core.Note;

namespace JAStudio.Core.TestUtils;

/// <summary>
/// The study-notes in a pretend Anki, for specifications. It holds only what was written into it or put in it: unlike JAStudio inside Anki, the test<br/>
/// environment doesn't write a note's study-note when it updates the note.
/// </summary>
public class InMemoryAnkiStudyNotes : IAnkiStudyNotes
{
   readonly ExternalNoteIdMap _ankiNoteIds;
   readonly Dictionary<long, StudyNoteInAnki> _byAnkiNoteId = [];

   public InMemoryAnkiStudyNotes(ExternalNoteIdMap ankiNoteIds) => _ankiNoteIds = ankiNoteIds;

   public IReadOnlyList<StudyNoteInAnki> ReadAll() => [.. _byAnkiNoteId.Values];

   public void Write(JPNote note)
   {
      var written = note.GetData();
      Put(new StudyNoteInAnki(_ankiNoteIds.RequireExternalId(note.GetId()), written.Fields.ToDictionary(), written.Tags));
   }

   /// <summary>Puts <paramref name="studyNote"/> in the pretend Anki, in place of the study-note of the same Anki note, as a change made in Anki would.</summary>
   public void Put(StudyNoteInAnki studyNote) => _byAnkiNoteId[studyNote.AnkiNoteId] = studyNote;
}
