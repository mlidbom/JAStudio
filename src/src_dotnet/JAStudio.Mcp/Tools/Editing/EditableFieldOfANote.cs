using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.EditableFields;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>
/// One of a note's <see cref="IEditableField{TNote}"/>s, together with the note: the field read and written on that note, whatever its type.<br/>
/// What <see cref="NoteEditingTools"/> lists and edits.
/// </summary>
public sealed class EditableFieldOfANote
{
   readonly Func<object> _read;
   readonly Action<object> _write;

   EditableFieldOfANote(string name, string label, bool changesHowSentencesParse, Type valueType, Func<object> read, Action<object> write)
   {
      Name = name;
      Label = label;
      ChangesHowSentencesParse = changesHowSentencesParse;
      ValueType = valueType;
      _read = read;
      _write = write;
   }

   /// <summary>Each of <paramref name="fields"/> on <paramref name="note"/>.</summary>
   public static IReadOnlyList<EditableFieldOfANote> EachOf<TNote>(IEnumerable<IEditableField<TNote>> fields, TNote note) where TNote : JPNote =>
      [.. fields.Select(it => new EditableFieldOfANote(it.Name, it.Label, it.ChangesHowSentencesParse, it.ValueType, () => it.ReadValue(note), value => it.WriteValue(note, value)))];

   /// <summary>See <see cref="IEditableField{TNote}.Name"/>.</summary>
   public string Name { get; }

   /// <summary>See <see cref="IEditableField{TNote}.Label"/>.</summary>
   public string Label { get; }

   /// <summary>See <see cref="IEditableField{TNote}.ChangesHowSentencesParse"/>.</summary>
   public bool ChangesHowSentencesParse { get; }

   /// <summary>See <see cref="IEditableField{TNote}.ValueType"/>.</summary>
   public Type ValueType { get; }

   /// <summary>The field's value on the note, of the <see cref="ValueType"/>.</summary>
   public object Read() => _read();

   /// <summary>Writes <paramref name="value"/>, of the <see cref="ValueType"/>, to the field on the note, which saves the note.</summary>
   public void Write(object value) => _write(value);
}
