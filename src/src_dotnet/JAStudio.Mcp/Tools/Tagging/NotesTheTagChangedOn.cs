using System.Collections.Generic;
using JAStudio.Mcp.Tools.Notes;

namespace JAStudio.Mcp.Tools.Tagging;

/// <summary>
/// The notes <see cref="TagTools.AddATagToTheNotesFound"/> added the tag to, or <see cref="TagTools.RemoveATagFromTheNotesFound"/> removed it from:<br/>
/// how many, and the first of them. A note found that already had the tag, or didn't have it, isn't changed, so isn't among them.
/// </summary>
/// <param name="NotesChanged">How many notes the tag was added to or removed from, listed or not.</param>
/// <param name="Notes">The notes listed, at most as many as asked for.</param>
public record NotesTheTagChangedOn(int NotesChanged, IReadOnlyList<NoteSummary> Notes);
