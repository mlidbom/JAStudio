using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Editing.Fields;

/// <summary>What <see cref="NoteEditingTools.EditANote"/> reports.</summary>
/// <param name="Note">The note's question after the edit, to recognize it by.</param>
/// <param name="Changes">
/// Each field of the note whose value the edit changed, in the order of the note's fields: those written, and any that writing them changed besides,<br/>
/// such as a vocab's synonyms when its perfect synonyms are written.
/// </param>
/// <param name="SentencesReparsed">Whether the sentences that may contain the vocab were reparsed, because a field written changes how sentences parse.</param>
public record NoteEdit(string Note, IReadOnlyList<FieldChange> Changes, bool SentencesReparsed);
