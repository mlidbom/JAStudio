using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Core.Importing.Checking;

/// <summary>What an <see cref="ImportRun"/> does with a <see cref="RawNote"/>, as its check decided.</summary>
public enum RawNoteOutcome
{
   /// <summary>It becomes a corpus sentence, and its media goes into the media layer.</summary>
   WillBeImported,

   /// <summary>It's tagged <see cref="RawNote.ImportedTag"/>: an earlier run imported it. It's left as it is. Removing the tag in Anki makes it importable again.</summary>
   ImportedByAnEarlierRun,

   /// <summary>It's skipped, and left in Anki: a sentence has its external id, or a note uses one of its media files.</summary>
   AlreadyInJAStudio,

   /// <summary>Something about it stops the whole run, such as a media file missing from Anki's media folder.</summary>
   Problem
}
