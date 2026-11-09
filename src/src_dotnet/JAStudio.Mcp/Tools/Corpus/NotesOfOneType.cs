using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Mcp.Tools.Notes;

namespace JAStudio.Mcp.Tools.Corpus;

/// <summary>Some notes of one type: how many, and the first of them by question.</summary>
/// <param name="Count">How many notes there are, listed or not.</param>
/// <param name="Notes">The notes listed, at most as many as asked for, in order of question.</param>
public record NotesOfOneType(int Count, IReadOnlyList<NoteSummary> Notes)
{
   public static NotesOfOneType Of(IEnumerable<JPNote> notes, int maxNotesListed)
   {
      var notesByQuestion = notes.OrderBy(it => it.GetQuestion(), StringComparer.Ordinal).ToList();
      return new NotesOfOneType(notesByQuestion.Count, [.. notesByQuestion.Take(maxNotesListed).Select(NoteSummary.Of)]);
   }
}
