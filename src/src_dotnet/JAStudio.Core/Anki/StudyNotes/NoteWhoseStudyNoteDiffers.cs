using JAStudio.Core.Note;

namespace JAStudio.Core.Anki.StudyNotes;

/// <summary>A note whose study-note in Anki differs from the one JAStudio would write for it now, and how.</summary>
public record NoteWhoseStudyNoteDiffers(JPNote Note, StudyNoteDifferences Differences);
