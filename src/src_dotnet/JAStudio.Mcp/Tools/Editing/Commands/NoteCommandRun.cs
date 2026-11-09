using System.Collections.Generic;
using JAStudio.Mcp.Tools.Notes;

namespace JAStudio.Mcp.Tools.Editing.Commands;

/// <summary>What <see cref="NoteCommandTools.RunACommandOnANote"/> reports.</summary>
/// <param name="Note">The note's question after the command, to recognize it by.</param>
/// <param name="Command">The command's name.</param>
/// <param name="Changes">Each field of the note whose value the command changed, in the order of the note's fields.</param>
/// <param name="VocabCreated">
/// Each vocab the command added to the collection: the one it's for creating, if it <see cref="CommandOfANote.CreatesAVocab"/>, and any it created<br/>
/// besides, such as autogenerating a vocab's compound parts does for each part that has none.
/// </param>
public record NoteCommandRun(string Note, string Command, IReadOnlyList<FieldChange> Changes, IReadOnlyList<NoteSummary> VocabCreated);
