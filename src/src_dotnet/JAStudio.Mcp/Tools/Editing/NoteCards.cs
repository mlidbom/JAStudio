using JAStudio.Core.Note;
using JAStudio.Mcp.Tools.Notes;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>A note, with whether it has cards that are studied and cards that are suspended in Anki, as <see cref="UniversalNoteActionTools"/> reports it.</summary>
/// <param name="Note">The note, as a list of notes shows it.</param>
/// <param name="HasActiveCards">Whether any of its cards is studied, not suspended.</param>
/// <param name="HasSuspendedCards">Whether any of its cards is suspended.</param>
public record NoteCards(NoteSummary Note, bool HasActiveCards, bool HasSuspendedCards)
{
   public static NoteCards Of(JPNote note) => new(NoteSummary.Of(note), note.HasActiveCards(), note.HasSuspendedCards());
}
