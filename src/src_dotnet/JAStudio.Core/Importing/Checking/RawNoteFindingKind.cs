namespace JAStudio.Core.Importing.Checking;

/// <summary>What a <see cref="RawNoteFinding"/> means for its raw note's <see cref="RawNoteOutcome"/>.</summary>
public enum RawNoteFindingKind
{
   /// <summary>Worth knowing, but changes nothing, such as a media file the import reuses.</summary>
   Information,

   /// <summary>Shows the raw note is in JAStudio already, so it's skipped: <see cref="RawNoteOutcome.AlreadyInJAStudio"/>.</summary>
   AlreadyInJAStudio,

   /// <summary>Stops the whole run: <see cref="RawNoteOutcome.Problem"/>.</summary>
   Problem
}
