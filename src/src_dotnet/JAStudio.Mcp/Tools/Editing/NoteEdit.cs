using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>What <see cref="NoteEditingTools.EditANote"/> reports.</summary>
/// <param name="Note">The note's question after the edit, to recognize it by.</param>
/// <param name="Changes">Each field written, in the order given.</param>
/// <param name="SentencesReparsed">Whether the sentences that may contain the vocab were reparsed, because a field written changes how sentences parse.</param>
public record NoteEdit(string Note, IReadOnlyList<FieldChange> Changes, bool SentencesReparsed);
