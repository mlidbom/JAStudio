using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using JAStudio.Mcp.Tools.Editing.Fields;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>
/// The value of each editable field of a note (<see cref="EditableNote"/>) at one moment, in JSON: what an edit or a command changed in the note is<br/>
/// the difference between its values before and after (<see cref="ChangesTo"/>).
/// </summary>
sealed class NoteFieldValues
{
   readonly IReadOnlyList<FieldValue> _values;

   NoteFieldValues(IReadOnlyList<FieldValue> values) => _values = values;

   /// <summary>The value of each editable field of <paramref name="note"/> now.</summary>
   public static NoteFieldValues Of(EditableNote note) => new([.. note.Fields.Select(it => new FieldValue(it.Name, FieldValueJson.JsonOf(it.Read())))]);

   /// <summary>Each field whose value in <paramref name="later"/>, the values of the same note at a later moment, differs from its value here, in the order of the note's fields.</summary>
   public IReadOnlyList<FieldChange> ChangesTo(NoteFieldValues later) =>
      [.. _values.Zip(later._values)
                 .Where(it => !JsonNode.DeepEquals(it.First.Value, it.Second.Value))
                 .Select(it => new FieldChange(it.First.Field, it.First.Value, it.Second.Value))];

   /// <summary>One field's value, in JSON.</summary>
   record FieldValue(string Field, JsonNode? Value);
}
