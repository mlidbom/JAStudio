using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki.StudyNotes;

namespace JAStudio.Mcp.Tools.StudyNotes;

/// <summary>The notes of one type whose study-note differs: how many, how many of them only in the markup of their audio, and the first of them by question, each with what differs.</summary>
/// <param name="Count">How many notes there are, listed or not.</param>
/// <param name="OnlyInTheMarkupOfTheirAudio">How many of them differ only in the markup of their audio (<see cref="NoteWhoseStudyNoteDiffers.OnlyTheMarkupOfItsAudioDiffers"/>).</param>
/// <param name="Notes">The notes listed, at most as many as asked for, in order of question.</param>
public record StudyNotesThatDifferOfOneType(int Count, int OnlyInTheMarkupOfTheirAudio, IReadOnlyList<StudyNoteThatDiffers> Notes)
{
   /// <summary>The report of <paramref name="notes"/>, listing at most <paramref name="maxNotesListed"/> of them, and none that <paramref name="isLeftOutOfTheList"/>.</summary>
   public static StudyNotesThatDifferOfOneType Of(IReadOnlyList<NoteWhoseStudyNoteDiffers> notes, int maxNotesListed, Func<NoteWhoseStudyNoteDiffers, bool> isLeftOutOfTheList) =>
      new(notes.Count,
          notes.Count(it => it.OnlyTheMarkupOfItsAudioDiffers),
          [.. notes.Where(it => !isLeftOutOfTheList(it)).OrderBy(it => it.Note.GetQuestion(), StringComparer.Ordinal).Take(maxNotesListed).Select(StudyNoteThatDiffers.Of)]);
}
