namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs;

/// <summary>How a note whose file differs between two versions of the corpus changed, as a <see cref="NoteDiff"/> reports it.</summary>
public enum NoteChangeKind
{
   /// <summary>Both versions have the note, and the notes read from them differ.</summary>
   Changed,

   /// <summary>Only the version compared to has the note.</summary>
   Added,

   /// <summary>Only the version compared from has the note.</summary>
   Deleted,

   /// <summary>Both versions have the note, and the notes read from them are the same: only how the file is written differs.</summary>
   ChangedOnlyInItsFile
}
