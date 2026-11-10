using System.Collections.Generic;
using JAStudio.Core.Note;

namespace JAStudio.Core.Anki.StudyNotes;

/// <summary>A note whose study-note in Anki differs from the one JAStudio would write for it now, with the fields that differ.</summary>
public record NoteWhoseStudyNoteDiffers(JPNote Note, IReadOnlyList<StudyNoteFieldDifference> Fields);
