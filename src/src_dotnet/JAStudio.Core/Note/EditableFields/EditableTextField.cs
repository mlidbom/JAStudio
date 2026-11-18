using System;

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
   /// <param name="whatTheTextHolds">See <see cref="WhatTheTextHolds"/>.</param>
   public EditableTextField(string name, string label, TextFieldFormat format, Func<TNote, string> read, Action<TNote, string> write, bool changesHowSentencesParse = false,
                            string? whatTheTextHolds = null)
      : base(name, label, changesHowSentencesParse)
   {
      Format = format;
      _read = read;
      _write = write;
      WhatTheTextHolds = whatTheTextHolds;
   }

   /// <summary>
   /// What the text holds, when it's more than text to read, such as a match-condition as JSON: for whoever writes the field to know before the first<br/>
   /// try, as JAStudio's MCP server says what each field takes. Null for plain text.
   /// </summary>
   public string? WhatTheTextHolds { get; }

   /// <summary>Whether the field holds a <see cref="TextFieldFormat.Line"/> or <see cref="TextFieldFormat.Paragraphs"/>.</summary>
   public TextFieldFormat Format { get; }

   public override string Read(TNote note) => _read(note);
   protected override void WriteToTheEditedNote(TNote note, string value) => _write(note, value);
}
