using JAStudio.Mcp.Tools.Corpus;
using JAStudio.Mcp.Tools.Timing;

namespace JAStudio.Mcp.Tools.Rebuilding;

/// <summary>
/// What <see cref="RebuildTools.FullRebuild"/> reports: how long the rebuild took, the processor time it and the machine's other processes used<br/>
/// meanwhile, and the notes whose files differ from the corpus checkout's git <c>HEAD</c> after it.
/// </summary>
public record FullRebuildResult(double SecondsTaken, ProcessorTimeUsed ProcessorTime, ChangedNotes ChangedNotes);
