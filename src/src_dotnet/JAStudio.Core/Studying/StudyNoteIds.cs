using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;

namespace JAStudio.Core.Studying;

/// <summary>
/// Which study-note each note has in the SRS, the spaced repetition system, by id: the id of a note's study-note, and the id of the note a study-note<br/>
/// belongs to. A study-note's id is the one the spaced repetition system gives it: in Anki, the id of its Anki note. A note that isn't in the<br/>
/// spaced repetition system has none.
/// </summary>
/// <remarks>Any number of threads read and change it at once.</remarks>
/// <remarks>
/// review-naming:studying: StudyNoteIds was ExternalNoteIdMap, and a study-note id was an external id (Claude, 2026-11-15). NoteIdOfTheStudyNote was<br/>
/// FromExternalId, StudyNoteIdOf was ToExternalId, and RequiredStudyNoteIdOf was RequireExternalId.
/// </remarks>
public class StudyNoteIds
{
   readonly ConcurrentDictionary<NoteId, long> _studyNoteIdOfEachNote = new();
   readonly ConcurrentDictionary<long, NoteId> _noteIdOfEachStudyNote = new();

   /// <summary>Records that the study-note with the id <paramref name="studyNoteId"/> is that of the note with the id <paramref name="noteId"/>.</summary>
   public void Register(long studyNoteId, NoteId noteId)
   {
      _noteIdOfEachStudyNote[studyNoteId] = noteId;
      _studyNoteIdOfEachNote[noteId] = studyNoteId;
   }

   /// <summary>The id of the note that the study-note with the id <paramref name="studyNoteId"/> belongs to, or null when no note is known to have it.</summary>
   public NoteId? NoteIdOfTheStudyNote(long studyNoteId) => _noteIdOfEachStudyNote.GetValueOrDefault(studyNoteId);

   /// <summary>The id of the study-note of the note with the id <paramref name="noteId"/>, or null when the note has none.</summary>
   public long? StudyNoteIdOf(NoteId noteId) => _studyNoteIdOfEachNote.TryGetValue(noteId, out var studyNoteId) ? studyNoteId : null;

   /// <summary>The ids of the study-notes of <paramref name="notes"/>: of those of them that have one.</summary>
   public IReadOnlyList<long> StudyNoteIdsOf(IEnumerable<JPNote> notes) => [.. notes.Select(it => StudyNoteIdOf(it.GetId())).OfType<long>()];

   /// <summary>The id of the study-note of the note with the id <paramref name="noteId"/>, which must have one.</summary>
   public long RequiredStudyNoteIdOf(NoteId noteId) => _studyNoteIdOfEachNote.TryGetValue(noteId, out var studyNoteId)
                                                          ? studyNoteId
                                                          : throw new KeyNotFoundException($"The note {noteId} has no study-note.");

   /// <summary>Forgets the study-note with the id <paramref name="studyNoteId"/>, as when it's deleted in the spaced repetition system: its note then has none.</summary>
   public void Unregister(long studyNoteId)
   {
      if(_noteIdOfEachStudyNote.TryRemove(studyNoteId, out var noteId))
      {
         _studyNoteIdOfEachNote.TryRemove(noteId, out _);
      }
   }

   /// <summary>Forgets every study-note.</summary>
   public void Clear()
   {
      _studyNoteIdOfEachNote.Clear();
      _noteIdOfEachStudyNote.Clear();
   }

   /// <summary>
   /// The id of every study-note's note as held now, by the study-note's id: to give to <see cref="TakeIn"/> once the study-notes are read from the<br/>
   /// spaced repetition system again.
   /// </summary>
   public IReadOnlyDictionary<long, NoteId> Snapshot() => new Dictionary<long, NoteId>(_noteIdOfEachStudyNote);

   /// <summary>
   /// Takes in <paramref name="readFromTheSRS"/>, the id of every study-note's note as read from the spaced repetition system again,<br/>
   /// in place of <paramref name="heldWhenTheReadBegan"/>, those held when that read began (<see cref="Snapshot"/>), one study-note at a time, so that<br/>
   /// this is never empty meanwhile. A study-note registered since the read began, as when one was created meanwhile, is kept, since the read can't<br/>
   /// have seen it.
   /// </summary>
   /// <returns>
   /// The notes whose study-note is gone from the spaced repetition system: held when the read began, and no longer in<br/>
   /// <paramref name="readFromTheSRS"/>.
   /// </returns>
   public IReadOnlyList<NoteId> TakeIn(IReadOnlyDictionary<long, NoteId> readFromTheSRS, IReadOnlyDictionary<long, NoteId> heldWhenTheReadBegan)
   {
      List<NoteId> notesWhoseStudyNoteIsGone = [];
      foreach(var (studyNoteId, noteId) in heldWhenTheReadBegan)
      {
         if(readFromTheSRS.ContainsKey(studyNoteId)) continue;

         Unregister(studyNoteId);
         notesWhoseStudyNoteIsGone.Add(noteId);
      }

      foreach(var (studyNoteId, noteId) in readFromTheSRS)
         Register(studyNoteId, noteId);

      return notesWhoseStudyNoteIsGone;
   }
}
