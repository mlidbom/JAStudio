namespace JAStudio.UI.Controls.NoteFields;

/// <summary>What kind of text a note's text field holds, which decides how its <see cref="TextFieldEditorViewModel"/> edits it.</summary>
enum TextFieldFormat
{
   /// <summary>A single line of text.</summary>
   Line,

   /// <summary>Paragraphs of text: the text box wraps its lines, and Enter starts a new one.</summary>
   Paragraphs,

   /// <summary>A list of values on one line, separated by commas.</summary>
   CommaSeparated,

   /// <summary>A comma separated list of readings, the primary ones marked as <c>&lt;primary&gt;reading&lt;/primary&gt;</c>.</summary>
   CommaSeparatedReadings
}
