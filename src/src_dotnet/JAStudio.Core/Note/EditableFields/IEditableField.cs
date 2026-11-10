using System;

namespace JAStudio.Core.Note.EditableFields;

/// <summary>
/// One field of a note type that a person edits, in the note's editor in the workspace or through the note's menus, such as a vocab's answer, one of<br/>
/// its matching flags or its synonyms: its <see cref="Name"/>, the <see cref="Label"/> its editor shows, and how its value is read from and written<br/>
/// to a note of the type.
/// </summary>
/// <remarks>
/// The note's editors are built from these, and so is JAStudio's MCP server's <c>edit_a_note</c>, so that the MCP server can edit whatever<br/>
/// a person can. A field is written through the note's own operations, as the editors and menus write it, never into the note's stored form.
/// </remarks>
/// <remarks>
/// This is the field as a value of a single <see cref="ValueType"/>, read and written as an <see cref="object"/>, for code that edits any field,<br/>
/// such as the MCP server's. <see cref="EditableField{TNote,TValue}"/> is the same field with its value typed.
/// </remarks>
public interface IEditableField<in TNote> where TNote : JPNote
{
   /// <summary>
   /// The field's path in the note as the corpus stores it, such as <c>corpusAnswer</c> or <c>matching.flags.isPoisonWord</c>. But for a vocab's matching rules,<br/>
   /// such as <c>matching.rules.masuStem</c>, which edit part of its match-condition, stored as a whole at <c>matching.condition</c>.
   /// </summary>
   string Name { get; }

   /// <summary>What the note's editor shows beside the field.</summary>
   string Label { get; }

   /// <summary>
   /// Whether changing the field changes which words of sentences match the note, as a vocab's forms and matching rules do,<br/>
   /// so that the sentences the note may be in must be parsed again.
   /// </summary>
   bool ChangesHowSentencesParse { get; }

   /// <summary>The type of the field's value, such as <see cref="string"/> for a text or <see cref="bool"/> for a flag.</summary>
   Type ValueType { get; }

   /// <summary>The field's value on <paramref name="note"/>, of the <see cref="ValueType"/>.</summary>
   object ReadValue(TNote note);

   /// <summary>Writes <paramref name="value"/>, of the <see cref="ValueType"/>, to the field on <paramref name="note"/>, which saves the note.</summary>
   void WriteValue(TNote note, object value);
}
