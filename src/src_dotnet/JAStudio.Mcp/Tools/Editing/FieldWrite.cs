using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>
/// One of the writes <c>edit_a_note</c> makes: a field of the note (<see cref="EditableFieldOfANote"/>) and the value to write to it, read from the JSON<br/>
/// the tool was given. Creating one checks both, so that <see cref="EachIn"/> checks every write before any is made.
/// </summary>
sealed class FieldWrite
{
   readonly object _value;

   FieldWrite(EditableFieldOfANote field, JsonNode? value)
   {
      Field = field;
      _value = FieldValueJson.ValueFor(field, value);
   }

   /// <summary>
   /// A write for each field of <paramref name="note"/> that <paramref name="changes"/> names, with the value it gives, in the order given,<br/>
   /// or, at the first name the note has no field of or value the field can't take, a failure saying so.
   /// </summary>
   public static IReadOnlyList<FieldWrite> EachIn(JsonObject changes, EditableNote note) => [.. changes.Select(it => new FieldWrite(note.FieldNamed(it.Key), it.Value))];

   public EditableFieldOfANote Field { get; }

   /// <summary>Writes the value to the field on the note, which saves the note.</summary>
   public void Write() => Field.Write(_value);
}
