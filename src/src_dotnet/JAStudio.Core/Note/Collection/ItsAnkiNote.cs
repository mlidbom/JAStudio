namespace JAStudio.Core.Note.Collection;

/// <summary>
/// What deleting a note in JAStudio (<see cref="JPCollection.Delete"/>) does to the note's Anki note, which holds the note's cards and their review<br/>
/// history. The history can't be kept without the Anki note. A person chooses, since only Anki holds the history, and JAStudio is the master of the<br/>
/// notes, not Anki.
/// </summary>
public enum ItsAnkiNote
{
   /// <summary>The Anki note is left as it is, with its cards and their review history. Its cards then show that the note no longer exists in JAStudio.</summary>
   IsKept,

   /// <summary>The Anki note is deleted from Anki too, with its cards and their review history, which can't be undone.</summary>
   IsDeleted
}
