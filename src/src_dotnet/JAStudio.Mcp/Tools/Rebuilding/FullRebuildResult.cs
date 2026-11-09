using JAStudio.Mcp.Tools.Corpus;

namespace JAStudio.Mcp.Tools.Rebuilding;

/// <summary>What <see cref="RebuildTools.FullRebuild"/> reports: how long the rebuild took, and the notes whose files differ from the corpus checkout's git <c>HEAD</c> after it.</summary>
public record FullRebuildResult(double SecondsTaken, ChangedNotes ChangedNotes);
