namespace JAStudio.Core.Note.EditableFields;

/// <summary>What kind of text a note's text field holds, which decides how its editor edits it as text.</summary>
public enum TextFieldFormat
{
   /// <summary>A single line of text.</summary>
   Line,

   /// <summary>Paragraphs of text: Enter starts a new line, as the empty text box says.</summary>
   Paragraphs,

   /// <summary>A list of values on one line, separated by commas: how an <see cref="EditableTextListField{TNote}"/> is edited as text.</summary>
   CommaSeparated
}
