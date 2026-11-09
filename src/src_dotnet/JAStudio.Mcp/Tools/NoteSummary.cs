using JAStudio.Core.Note;
using JAStudio.Core.Note.Search;

namespace JAStudio.Mcp.Tools;

/// <summary>A note as a list of notes shows it: enough to recognize it, and its id to get the rest with <see cref="NoteTools.GetNote"/>.</summary>
/// <param name="NoteType">Whether it's a vocab, a kanji or a sentence, named as a search names it: see <see cref="NoteConditionJson.NoteTypeNameOf"/>.</param>
public record NoteSummary(string Id, string NoteType, string Question, string Answer)
{
   public static NoteSummary Of(JPNote note) =>
      new(note.GetId().ToString(), NoteConditionJson.NoteTypeNameOf(note.GetType()), note.GetQuestion(), note.GetAnswer());
}
