using JAStudio.Mcp.Tools.Corpus;

namespace JAStudio.Mcp.Tools.Reloading;

/// <summary>
/// What <see cref="ReloadTools.FastForwardTheCorpusCheckout"/> reports: the commits the corpus checkout was at before and is at after, by their hashes, how<br/>
/// long the fast-forward and the reload took, and the notes whose files the fast-forward changed.
/// </summary>
public record FastForwardResult(string CommitBefore, string CommitAfter, double SecondsTaken, ChangedNotes ChangedNotes);
