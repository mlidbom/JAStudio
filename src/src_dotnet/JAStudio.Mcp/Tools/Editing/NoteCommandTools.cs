using System.ComponentModel;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Mcp.Tools.Editing.Commands;
using JAStudio.Mcp.Tools.Notes;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>
/// The tools that run the commands of a note's menus on the note, as the menus do (<see cref="EditableNote.Commands"/>): what a person does to a note<br/>
/// from its menus other than editing a field, such as accepting a vocab's meaning or creating its て-form.
/// </summary>
[McpServerToolType]
public class NoteCommandTools
{
   readonly JPCollection _collection;

   public NoteCommandTools(JPCollection collection) => _collection = collection;

   [McpServerTool(Name = "get_the_commands_of_a_note", ReadOnly = true, Idempotent = true)]
   [Description("""
                Lists the commands of a note's menus that run_a_command_on_a_note runs: what a person does to the note from its menus, other than
                editing a field, which edit_a_note does. Such as accepting a vocab's meaning, generating its answer, creating a vocab from it, its
                する-verb, its て-form or one with a text suffixed onto its い-stem, or splitting a word of a sentence with a word-break tag. Each with
                its name, the label its menu shows, what text it takes, if any, and whether it creates a vocab rather than changing the note.
                """)]
   public NoteCommandsReported GetTheCommandsOfANote([Description("The note's id, as search_notes lists it.")] string noteId) =>
      NoteCommandsReported.Of(EditableNote.WithTheId(noteId, _collection));

   [McpServerTool(Name = "run_a_command_on_a_note", Destructive = true)]
   [Description("""
                Runs a command of a note's menus on the note, as the menu does, named as get_the_commands_of_a_note lists them, with the text it
                takes, if any. A command that changes the note saves it to the corpus checkout and updates its Anki note, as any edit does; one that
                creates a vocab adds the vocab to the collection the same way, as the menu does, and the MCP server has no tool that deletes a note.
                Like the menu, it neither reparses sentences nor updates the note's generated data. Reports each field of the note that the command
                changed, before and after, and each vocab it added to the collection: the one it's for creating, if any, and any it created
                besides, such as autogenerating a vocab's compound parts does for each part that has none.
                """)]
   public NoteCommandRun RunACommandOnANote(
      [Description("The note's id, as search_notes lists it.")] string noteId,
      [Description("The command's name, as get_the_commands_of_a_note lists it, such as \"acceptMeaning\" or \"createTeForm\".")] string command,
      [Description("The text the command takes, if it takes one, such as the form to clone a vocab to. None for a command that takes no text.")] string? text = null)
   {
      var note = EditableNote.WithTheId(noteId, _collection);
      var commandOfTheNote = note.CommandNamed(command);
      var valuesBefore = NoteFieldValues.Of(note);
      var vocabBefore = _collection.Vocab.All().ToHashSet();

      commandOfTheNote.Run(text);

      return new NoteCommandRun(note.Note.GetQuestion(),
                                commandOfTheNote.Name,
                                valuesBefore.ChangesTo(NoteFieldValues.Of(note)),
                                [.. _collection.Vocab.All().Where(it => !vocabBefore.Contains(it)).Select(NoteSummary.Of)]);
   }
}
