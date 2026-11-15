namespace JAStudio.Core.Note.Collection;

/// <summary>
/// What deleting a note in JAStudio (<see cref="JPCollection.Delete"/>) does to the note's study-note in the SRS, the spaced repetition system:<br/>
/// in Anki its Anki note, which holds the note's cards and their review history. The history can't be kept without the study-note. A person<br/>
/// chooses, since only the spaced repetition system holds the history, and JAStudio is the master of the notes, not the spaced repetition system.
/// </summary>
/// <remarks>review-naming:studying: ItsStudyNote was ItsAnkiNote (Claude, 2026-11-15).</remarks>
public enum ItsStudyNote
{
   /// <summary>The study-note is left as it is, with its cards and their review history. Its cards then show that the note no longer exists in JAStudio.</summary>
   IsKept,

   /// <summary>The study-note is deleted from the spaced repetition system too, with its cards and their review history, which can't be undone.</summary>
   IsDeleted
}
