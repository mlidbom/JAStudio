using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Mcp.Tools.Editing.Commands;

/// <summary>What <see cref="NoteCommandTools.GetTheCommandsOfANote"/> reports: the note's question, to recognize it by, and each of its commands, in order.</summary>
public record NoteCommandsReported(string Note, IReadOnlyList<NoteCommandReported> Commands)
{
   public static NoteCommandsReported Of(EditableNote note) => new(note.Note.GetQuestion(), [.. note.Commands.Select(NoteCommandReported.Of)]);
}
