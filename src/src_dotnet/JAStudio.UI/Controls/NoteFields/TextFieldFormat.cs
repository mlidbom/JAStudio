namespace JAStudio.UI.Controls.NoteFields;

/// <summary>What kind of text a note's text field holds, which decides how its <see cref="TextFieldEditorViewModel"/> edits it.</summary>
enum TextFieldFormat
{
   /// <summary>A single line of text.</summary>
   Line,

   /// <summary>Paragraphs of text: Enter starts a new line, as the empty text box says.</summary>
   Paragraphs,

   /// <summary>A list of values on one line, separated by commas.</summary>
   CommaSeparated
}
