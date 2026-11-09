using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>What <see cref="NoteEditingTools.GetTheEditableFieldsOfANote"/> reports: each of a note's editable fields, in the order of its editor's tabs.</summary>
/// <param name="Note">The note's question, to recognize it by.</param>
public record EditableFieldsReported(string Note, IReadOnlyList<EditableFieldReported> Fields)
{
   public static EditableFieldsReported Of(EditableNote note) => new(note.Note.GetQuestion(), [.. note.Fields.Select(EditableFieldReported.Of)]);
}
