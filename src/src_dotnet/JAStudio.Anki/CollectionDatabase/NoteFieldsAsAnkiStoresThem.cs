using System.Linq;

namespace JAStudio.Anki.CollectionDatabase;

/// <summary>
/// The values of a note's fields as an Anki collection's database stores them, in the <c>flds</c> column of its <c>notes</c> table: one text<br/>
/// of the values, in the order of the note type's fields (<see cref="NoteTypeInAnki.FieldNames"/>), each separated from the next by<br/>
/// <see cref="Separator"/>.
/// </summary>
public sealed class NoteFieldsAsAnkiStoresThem
{
   /// <summary>The control character Anki separates the values with, the unit separator.</summary>
   const char Separator = '\x1f';

   readonly string[] _values;

   /// <param name="fldsColumn">The text of the note's <c>flds</c> column.</param>
   public NoteFieldsAsAnkiStoresThem(string fldsColumn) => _values = fldsColumn.Split(Separator);

   /// <summary>
   /// The value of the field at <paramref name="ordinal"/>, counted from 0, or an empty text when the note holds no value there, as Anki's own SQL<br/>
   /// function <c>field_at_index</c> gives it.
   /// </summary>
   public string ValueAt(int ordinal) => _values.ElementAtOrDefault(ordinal) ?? "";
}
