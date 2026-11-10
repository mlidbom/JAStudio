using System.Collections.Generic;
using JAStudio.Core.Anki.StudyNotes;
using JAStudio.Mcp.Tools.Notes;

namespace JAStudio.Mcp.Tools.StudyNotes;

/// <summary>
/// A note whose study-note in Anki differs from what JAStudio would write for it now, with the fields that differ, and whether only the markup of its<br/>
/// audio differs (<see cref="NoteWhoseStudyNoteDiffers.OnlyTheMarkupOfItsAudioDiffers"/>).
/// </summary>
public record StudyNoteThatDiffers(NoteSummary Note, bool OnlyTheMarkupOfItsAudioDiffers, IReadOnlyList<StudyNoteFieldDifference> Fields)
{
   public static StudyNoteThatDiffers Of(NoteWhoseStudyNoteDiffers note) => new(NoteSummary.Of(note.Note), note.OnlyTheMarkupOfItsAudioDiffers, note.Fields);
}
