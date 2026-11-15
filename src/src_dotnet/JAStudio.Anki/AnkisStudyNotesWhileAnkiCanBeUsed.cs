using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Studying;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.Anki;

/// <summary>
/// The study-notes in Anki (<see cref="ISRSStudyNotes"/>) for a JAStudio that is a program of its own, beside which Anki may not be running:<br/>
/// while Anki can't be used (<see cref="SRSAvailability.CanBeUsedNow"/>), there are none to read. So JAStudio loads its collection with or without<br/>
/// Anki. Without, every note shows as not studied, until a sync reads the study-notes once Anki can be used.
/// </summary>
/// <remarks>
/// Only the read gives way. Changing a study-note while Anki can't be used fails, as any call to Anki then does.
/// </remarks>
public sealed class AnkisStudyNotesWhileAnkiCanBeUsed(ISRSStudyNotes ankisStudyNotes, SRSAvailability ankisAvailability) : ISRSStudyNotes
{
   readonly ISRSStudyNotes _ankisStudyNotes = ankisStudyNotes;
   readonly SRSAvailability _ankisAvailability = ankisAvailability;

   public IReadOnlyList<StudyNoteInTheSRS> ReadAll() => _ankisAvailability.CanBeUsedNow ? _ankisStudyNotes.ReadAll() : [];

   public void Write(JPNote note) => _ankisStudyNotes.Write(note);
   public void SuspendAllCardsForNote(NoteId noteId) => _ankisStudyNotes.SuspendAllCardsForNote(noteId);
   public void UnsuspendAllCardsForNote(NoteId noteId) => _ankisStudyNotes.UnsuspendAllCardsForNote(noteId);
   public void DeleteTheStudyNoteOf(NoteId noteId) => _ankisStudyNotes.DeleteTheStudyNoteOf(noteId);
}
