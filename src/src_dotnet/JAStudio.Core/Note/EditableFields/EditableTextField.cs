using System;
using JAStudio.Core.Note.NoteFields;

namespace JAStudio.Core.Note.EditableFields;

/// <summary>A text field of a note, such as a vocab's answer: a line or paragraphs of text (<see cref="Format"/>).</summary>
public sealed class EditableTextField<TNote> : EditableField<TNote, string> where TNote : JPNote
{
   readonly Func<TNote, string> _read;
   readonly Action<TNote, string> _write;

   /// <param name="name">The field's <see cref="IEditableField{TNote}.Name"/>: its path in the note as the corpus stores it.</param>
   /// <param name="label">What the note's editor shows beside the field.</param>
   /// <param name="format">Whether the field holds a line or paragraphs.</param>
   /// <param name="read">Reads the text from a note.</param>
   /// <param name="write">Writes a text to a note.</param>
   /// <param name="changesHowSentencesParse">See <see cref="IEditableField{TNote}.ChangesHowSentencesParse"/>.</param>
   public EditableTextField(string name, string label, TextFieldFormat format, Func<TNote, string> read, Action<TNote, string> write, bool changesHowSentencesParse = false)
      : base(name, label, changesHowSentencesParse)
   {
      Format = format;
      _read = read;
      _write = write;
   }

   /// <summary>A field held by a <see cref="WritableStringValue"/> of the note, which <paramref name="field"/> finds.</summary>
   public EditableTextField(string name, string label, TextFieldFormat format, Func<TNote, WritableStringValue> field)
      : this(name, label, format, it => field(it).Value, (it, text) => field(it).Set(text)) {}

   /// <summary>Whether the field holds a <see cref="TextFieldFormat.Line"/> or <see cref="TextFieldFormat.Paragraphs"/>.</summary>
   public TextFieldFormat Format { get; }

   public override string Read(TNote note) => _read(note);
   public override void Write(TNote note, string value) => _write(note, value);
}
