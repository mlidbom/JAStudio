using System.Collections.Generic;
using JAStudio.Core.Anki;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.UI.ViewModels.Studying;

/// <summary>A study-note in Anki that belongs to no note, as the Studying window lists it: enough to find it in Anki's browser, and to tell which note it was.</summary>
/// <param name="AnkiNoteId">The id of its Anki note, which Anki's browser finds with the search <c>nid:</c> and the id.</param>
/// <param name="Question">Its question field, <see cref="AnkiFieldNames.Question"/>.</param>
/// <param name="JasNoteId">Its <see cref="AnkiFieldNames.JasNoteId"/> field: the id of the note it was written for, if any.</param>
/// <param name="WhyItBelongsToNoNote">Why it belongs to no note, in words for a person.</param>
record StudyNoteOfNoNoteRow(long AnkiNoteId, string Question, string JasNoteId, string WhyItBelongsToNoNote)
{
   public static StudyNoteOfNoNoteRow Of(StudyNoteInTheSRS studyNote, string whyItBelongsToNoNote) =>
      new(studyNote.StudyNoteId, studyNote.Fields.GetValueOrDefault(AnkiFieldNames.Question, ""), studyNote.Fields.GetValueOrDefault(AnkiFieldNames.JasNoteId, ""), whyItBelongsToNoNote);
}
