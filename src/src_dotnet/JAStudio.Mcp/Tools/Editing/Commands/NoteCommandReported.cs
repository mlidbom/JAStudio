namespace JAStudio.Mcp.Tools.Editing.Commands;

/// <summary>One of a note's commands, as <see cref="NoteCommandTools.GetTheCommandsOfANote"/> reports it.</summary>
/// <param name="Name">The name <c>run_a_command_on_a_note</c> takes.</param>
/// <param name="Label">What the note's menu shows for the command.</param>
/// <param name="TextTaken">What the text the command takes is, or null for a command that takes no text.</param>
/// <param name="CreatesAVocab">Whether the command creates a vocab, rather than changing the note.</param>
public record NoteCommandReported(string Name, string Label, string? TextTaken, bool CreatesAVocab)
{
   public static NoteCommandReported Of(CommandOfANote command) => new(command.Name, command.Label, command.TextTaken, command.CreatesAVocab);
}
