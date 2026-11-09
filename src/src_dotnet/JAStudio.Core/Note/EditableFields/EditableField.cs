using System;

namespace JAStudio.Core.Note.EditableFields;

/// <summary>An <see cref="IEditableField{TNote}"/> whose value is a <typeparamref name="TValue"/>: each kind of field, such as a text or a flag, is one of these.</summary>
public abstract class EditableField<TNote, TValue> : IEditableField<TNote> where TNote : JPNote where TValue : notnull
{
   /// <param name="name">The field's <see cref="Name"/>: its path in the note as the corpus stores it.</param>
   /// <param name="label">What the note's editor shows beside the field.</param>
   /// <param name="changesHowSentencesParse">See <see cref="ChangesHowSentencesParse"/>.</param>
   protected EditableField(string name, string label, bool changesHowSentencesParse)
   {
      Name = name;
      Label = label;
      ChangesHowSentencesParse = changesHowSentencesParse;
   }

   public string Name { get; }
   public string Label { get; }
   public bool ChangesHowSentencesParse { get; }
   public Type ValueType => typeof(TValue);

   /// <summary>The field's value on <paramref name="note"/>.</summary>
   public abstract TValue Read(TNote note);

   /// <summary>Writes <paramref name="value"/> to the field on <paramref name="note"/>, which saves the note.</summary>
   public abstract void Write(TNote note, TValue value);

   public object ReadValue(TNote note) => Read(note);
   public void WriteValue(TNote note, object value) => Write(note, (TValue)value);

   public override string ToString() => Name;
}
