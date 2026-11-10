using JAStudio.Mcp.Tools.Corpus;

namespace JAStudio.Mcp.Tools.Saving;

/// <summary>
/// What <see cref="SaveTools.SaveEveryNote"/> reports: how many notes it wrote, how long that took, and the notes whose files differ from the corpus<br/>
/// checkout's git <c>HEAD</c> after it.
/// </summary>
public record EveryNoteSaved(int NotesWritten, double SecondsTaken, ChangedNotes ChangedNotes);
