using System.Collections.Generic;
using JAStudio.Core.Anki;
using JAStudio.Core.Anki.StudyNotes;

namespace JAStudio.Mcp.Tools.StudyNotes;

/// <summary>
/// A study-note in Anki that belongs to no note of the collection (<see cref="StudyNoteComparison.StudyNotesOfNoNote"/>), as<br/>
/// <see cref="StudyNoteTools.ListTheStudyNotesThatDiffer"/> lists it: enough to find it in Anki's browser and to tell which note it was.
/// </summary>
/// <param name="AnkiNoteId">The id of its Anki note, which Anki's browser finds with the search <c>nid:</c> and the id.</param>
/// <param name="Question">Its question field, <see cref="AnkiFieldNames.Question"/>.</param>
/// <param name="JasNoteId">Its <see cref="AnkiFieldNames.JasNoteId"/> field: the id of the note it was written for, if any.</param>
public record StudyNoteOfNoNote(long AnkiNoteId, string Question, string JasNoteId)
{
   public static StudyNoteOfNoNote Of(StudyNoteInAnki studyNote) =>
      new(studyNote.AnkiNoteId, studyNote.Fields.GetValueOrDefault(AnkiFieldNames.Question, ""), studyNote.Fields.GetValueOrDefault(AnkiFieldNames.JasNoteId, ""));
}
