using System;
using System.Collections.Generic;
using JAStudio.Core.Anki.StudyNotes;
using JAStudio.Core.Note;

namespace JAStudio.Anki;

/// <summary>The study-notes in the collection Anki has open: read straight from its database, and written through Anki.</summary>
public class AnkiStudyNotes : IAnkiStudyNotes
{
   readonly ExternalNoteIdMap _ankiNoteIds;

   public AnkiStudyNotes(ExternalNoteIdMap ankiNoteIds) => _ankiNoteIds = ankiNoteIds;

   public IReadOnlyList<StudyNoteInAnki> ReadAll() =>
      StudyNotesInAnkisDatabase.ReadAll(AnkiFacade.Col.DbFilePath() ?? throw new InvalidOperationException("Anki has no collection open to read the study-notes from."));

   public void Write(JPNote note) => AnkiFacade.StudyNotes.Write(_ankiNoteIds.RequireExternalId(note.GetId()), note);
}
