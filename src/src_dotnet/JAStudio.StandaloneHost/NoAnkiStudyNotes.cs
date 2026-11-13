using System;
using System.Collections.Generic;
using JAStudio.Core.Anki.StudyNotes;
using JAStudio.Core.Note;

namespace JAStudio.StandaloneHost;

/// <summary>The study-notes in Anki, as the standalone JAStudio has them: none, since it has no Anki.</summary>
sealed class NoAnkiStudyNotes : IAnkiStudyNotes
{
   public IReadOnlyList<StudyNoteInAnki> ReadAll() => [];

   public void Write(JPNote note) => throw new InvalidOperationException($"The standalone JAStudio has no Anki, so {note.GetQuestion()} has no study-note to write.");
}
