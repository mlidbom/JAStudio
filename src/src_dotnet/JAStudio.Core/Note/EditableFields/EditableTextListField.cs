using System;
using System.Collections.Generic;

namespace JAStudio.Core.Note.EditableFields;

/// <summary>
/// A field of a note that holds a list of texts in an order that matters, such as a vocab's readings, the primary one first.<br/>
/// The note's editor edits it as one line of text, the texts separated by commas (<see cref="TextFieldFormat.CommaSeparated"/>).
/// </summary>
public sealed class EditableTextListField<TNote> : EditableField<TNote, IReadOnlyList<string>> where TNote : JPNote
{
   readonly Func<TNote, IReadOnlyList<string>> _read;
   readonly Action<TNote, List<string>> _write;

   /// <param name="name">The field's <see cref="IEditableField{TNote}.Name"/>: its path in the note as the corpus stores it.</param>
   /// <param name="label">What the note's editor shows beside the field.</param>
   /// <param name="read">Reads the texts from a note.</param>
   /// <param name="write">Writes all the texts to a note, replacing those it has.</param>
   public EditableTextListField(string name, string label, Func<TNote, IReadOnlyList<string>> read, Action<TNote, List<string>> write)
      : base(name, label, changesHowSentencesParse: false)
   {
      _read = read;
      _write = write;
   }

   public override IReadOnlyList<string> Read(TNote note) => _read(note);
   public override void Write(TNote note, IReadOnlyList<string> value) => _write(note, [.. value]);
}
