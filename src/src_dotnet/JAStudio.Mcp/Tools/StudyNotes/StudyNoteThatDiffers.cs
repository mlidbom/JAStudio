using System.Collections.Generic;
using JAStudio.Core.Anki.StudyNotes;
using JAStudio.Mcp.Tools.Notes;

namespace JAStudio.Mcp.Tools.StudyNotes;

/// <summary>A note whose study-note in Anki differs from what JAStudio would write for it now, and how (<see cref="StudyNoteDifferences"/>).</summary>
public record StudyNoteThatDiffers(NoteSummary Note, IReadOnlyList<StudyNoteFieldDifference> Fields, IReadOnlyList<string> TagsOnlyInAnki, IReadOnlyList<string> TagsOnlyJAStudioWrites)
{
   public static StudyNoteThatDiffers Of(NoteWhoseStudyNoteDiffers note) =>
      new(NoteSummary.Of(note.Note), note.Differences.Fields, note.Differences.TagsOnlyInAnki, note.Differences.TagsOnlyJAStudioWrites);
}
