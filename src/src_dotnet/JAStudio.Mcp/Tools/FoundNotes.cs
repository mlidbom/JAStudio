using System.Collections.Generic;

namespace JAStudio.Mcp.Tools;

/// <summary>What <see cref="NoteTools.SearchNotes"/> found: how many notes, and the first of them.</summary>
/// <param name="NotesFound">How many notes the search found, listed or not.</param>
/// <param name="Notes">The notes listed, at most as many as asked for.</param>
public record FoundNotes(int NotesFound, IReadOnlyList<NoteSummary> Notes);
