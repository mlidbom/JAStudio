using System;
using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.StandaloneHost;

/// <summary>The study-notes in Anki, as the standalone JAStudio has them: none, since it has no Anki.</summary>
sealed class NoAnkiStudyNotes : ISpacedRepetitionSystemStudyNotes
{
   public IReadOnlyList<StudyNoteInTheSpacedRepetitionSystem> ReadAll() => [];

   public void Write(JPNote note) => throw new InvalidOperationException($"The standalone JAStudio has no Anki, so {note.GetQuestion()} has no study-note to write.");
}
