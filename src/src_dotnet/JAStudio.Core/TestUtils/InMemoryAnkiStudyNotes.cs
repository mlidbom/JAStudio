using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Studying;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.Core.TestUtils;

/// <summary>
/// The study-notes in a pretend Anki, for specifications. It holds only what was written into it or put in it: unlike JAStudio inside Anki, the test<br/>
/// environment doesn't write a note's study-note when it updates the note.
/// </summary>
public class InMemoryAnkiStudyNotes : ISRSStudyNotes
{
   readonly StudyNoteIds _ankiNoteIds;
   readonly Dictionary<long, StudyNoteInTheSRS> _byAnkiNoteId = [];

   public InMemoryAnkiStudyNotes(StudyNoteIds ankiNoteIds) => _ankiNoteIds = ankiNoteIds;

   public IReadOnlyList<StudyNoteInTheSRS> ReadAll() => [.. _byAnkiNoteId.Values];

   public void Write(JPNote note) => Put(new StudyNoteInTheSRS(_ankiNoteIds.RequiredStudyNoteIdOf(note.GetId()), note.StudyNoteFields()));

   /// <summary>Puts <paramref name="studyNote"/> in the pretend Anki, in place of the study-note of the same Anki note, as a change made in Anki would.</summary>
   public void Put(StudyNoteInTheSRS studyNote) => _byAnkiNoteId[studyNote.StudyNoteId] = studyNote;
}
