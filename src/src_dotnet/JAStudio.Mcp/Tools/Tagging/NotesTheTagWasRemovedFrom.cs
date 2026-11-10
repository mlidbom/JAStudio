using System.Collections.Generic;
using JAStudio.Mcp.Tools.Notes;

namespace JAStudio.Mcp.Tools.Tagging;

/// <summary>The notes <see cref="TagTools.RemoveATagFromTheNotesFound"/> removed the tag from: how many, and the first of them.</summary>
/// <param name="NotesChanged">How many notes had the tag and no longer have it, listed or not.</param>
/// <param name="Notes">The notes listed, at most as many as asked for.</param>
public record NotesTheTagWasRemovedFrom(int NotesChanged, IReadOnlyList<NoteSummary> Notes);
