using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Teventive.TaggregateStores;
using JAStudio.Core.Note;
using JAStudio.Core.Note.EditableFields;

namespace JAStudio.Mcp.Tools.Editing.Fields;

/// <summary>
/// One of a note's <see cref="IEditableField{TNote}"/>s, together with the note: the field read and written on that note, whatever its type.<br/>
/// What <see cref="NoteEditingTools"/> lists and edits.
/// </summary>
public sealed class EditableFieldOfANote
{
   readonly Func<object> _read;
   readonly Action<object> _write;

   EditableFieldOfANote(string name, string label, bool changesHowSentencesParse, Type valueType, string takes, Func<object> read, Action<object> write)
   {
      Name = name;
      Label = label;
      ChangesHowSentencesParse = changesHowSentencesParse;
      ValueType = valueType;
      Takes = takes;
      _read = read;
      _write = write;
   }

   /// <summary>Each of <paramref name="fields"/> on <paramref name="note"/>.</summary>
   public static IReadOnlyList<EditableFieldOfANote> EachOf<TNote>(IEnumerable<IEditableField<TNote>> fields, ITaggregateReference<TNote> note) where TNote : JPNote =>
      [.. fields.Select(it => new EditableFieldOfANote(it.Name, it.Label, it.ChangesHowSentencesParse, it.ValueType, WhatItTakes(it), () => it.ReadValue(note.Current), value => it.WriteValue(note.Current, value)))];

   /// <summary>What <paramref name="field"/> takes, in JSON: what its text holds, for a text that holds more than text, or else what its value's type is.</summary>
   static string WhatItTakes<TNote>(IEditableField<TNote> field) where TNote : JPNote =>
      field is EditableTextField<TNote> { WhatTheTextHolds: {} itHolds } ? $"a text: {itHolds}" : FieldValueJson.DescriptionOf(field.ValueType);

   /// <summary>What the field's value is, in JSON, such as "a list of texts": what <c>get_the_editable_fields_of_a_note</c> reports, and a failed write says.</summary>
   public string Takes { get; }

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
