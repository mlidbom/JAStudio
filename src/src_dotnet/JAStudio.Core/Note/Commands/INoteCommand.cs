using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.Vocabulary.Commands;

namespace JAStudio.Core.Note.Commands;

/// <summary>
/// One command of a note type's menus, such as accepting a vocab's meaning or creating its て-form: what a person does to a note from its menus,<br/>
/// other than editing one of its fields (<see cref="IEditableField{TNote}"/>). Its <see cref="Name"/>, the <see cref="Label"/> its menu shows, whether it<br/>
/// takes a text, as creating a vocab with a text suffixed onto the note's question does, and whether it creates a vocab or changes the note.
/// </summary>
/// <remarks>
/// The notes' menus are built from these, and JAStudio's MCP server runs them, so that the MCP server can do whatever a person can from the menus.
/// </remarks>
/// <remarks>
/// This is the command as code that runs any command sees it, such as the MCP server, its text optional.<br/>
/// The menus use the typed commands that implement it: <see cref="NoteChangingCommand{TNote}"/>, <see cref="NoteChangingCommandTakingAText{TNote}"/>,<br/>
/// <see cref="VocabCreatingCommand"/> and <see cref="VocabCreatingCommandTakingAText"/>.
/// </remarks>
public interface INoteCommand<in TNote> where TNote : JPNote
{
   /// <summary>The command's name, in camel case, such as <c>acceptMeaning</c> or <c>createTeForm</c>.</summary>
   string Name { get; }

   /// <summary>What the note's menu shows for the command.</summary>
   string Label { get; }

   /// <summary>What the text the command takes is, such as "the form to clone the vocab to", or null for a command that takes no text.</summary>
   string? TextTaken { get; }

   /// <summary>
   /// Whether the command is for creating a vocab from the note, such as its て-form, rather than for changing the note. A command that changes the<br/>
   /// note can create vocab too: autogenerating a vocab's compound parts creates a vocab for each part that has none.
   /// </summary>
   bool CreatesAVocab { get; }

   /// <summary>
   /// Runs the command on <paramref name="note"/>, with <paramref name="text"/>, which must be given if the command takes a text (<see cref="TextTaken"/>)<br/>
   /// and must not be if it doesn't.
   /// </summary>
   void RunOn(TNote note, string? text);
}
