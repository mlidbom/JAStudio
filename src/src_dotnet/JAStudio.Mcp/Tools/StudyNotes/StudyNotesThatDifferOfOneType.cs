using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki.StudyNotes;

namespace JAStudio.Mcp.Tools.StudyNotes;

/// <summary>The notes of one type whose study-note differs: how many, and the first of them by question, each with what differs.</summary>
/// <param name="Count">How many notes there are, listed or not.</param>
/// <param name="Notes">The notes listed, at most as many as asked for, in order of question.</param>
public record StudyNotesThatDifferOfOneType(int Count, IReadOnlyList<StudyNoteThatDiffers> Notes)
{
   public static StudyNotesThatDifferOfOneType Of(IEnumerable<NoteWhoseStudyNoteDiffers> notes, int maxNotesListed)
   {
      var notesByQuestion = notes.OrderBy(it => it.Note.GetQuestion(), StringComparer.Ordinal).ToList();
      return new StudyNotesThatDifferOfOneType(notesByQuestion.Count, [.. notesByQuestion.Take(maxNotesListed).Select(StudyNoteThatDiffers.Of)]);
   }
}
