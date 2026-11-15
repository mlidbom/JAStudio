using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;

namespace JAStudio.Core.Studying;

/// <summary>
/// What the collection loads of the study-notes from the spaced repetition system (<see cref="ISpacedRepetitionSystemDataLoader"/>): which note<br/>
/// each study-note belongs to, the studying statuses of its cards, and the tags the student gave it there. The collection gives each note its<br/>
/// part of it.
/// </summary>
public class SpacedRepetitionSystemData
{
   //review-naming:studying: SpacedRepetitionSystemData was BackendData, and NoteIdOfEachStudyNote was IdMappings (Claude, 2026-11-15).
   readonly ILookup<long, CardStudyingStatus> _studyingStatusesByStudyNoteId;

   public SpacedRepetitionSystemData(Dictionary<long, NoteId> noteIdOfEachStudyNote, List<CardStudyingStatus> studyingStatuses, Dictionary<long, IReadOnlyList<string>> studyNoteTags)
   {
      NoteIdOfEachStudyNote = noteIdOfEachStudyNote;
      StudyingStatuses = studyingStatuses;
      StudyNoteTags = studyNoteTags;
      _studyingStatusesByStudyNoteId = studyingStatuses.ToLookup(it => it.StudyNoteId);
   }

   /// <summary>The id of the note each study-note belongs to, by the study-note's id (<see cref="StudyNoteIds"/>).</summary>
   public Dictionary<long, NoteId> NoteIdOfEachStudyNote { get; }

   /// <summary>The studying status of every card of the study-notes.</summary>
   public List<CardStudyingStatus> StudyingStatuses { get; }

   /// <summary>The tags each study-note has in the spaced repetition system, by the study-note's id: what each note's <see cref="JPNote.AnkiTags"/> are read from.</summary>
   public Dictionary<long, IReadOnlyList<string>> StudyNoteTags { get; }

   /// <summary>The studying statuses of the cards of the study-note with the id <paramref name="studyNoteId"/>.</summary>
   public IEnumerable<CardStudyingStatus> StudyingStatusesOf(long studyNoteId) => _studyingStatusesByStudyNoteId[studyNoteId];

   /// <summary>The tags of the study-note with the id <paramref name="studyNoteId"/>: none when it has none.</summary>
   public IReadOnlyList<string> TagsOf(long studyNoteId) => StudyNoteTags.GetValueOrDefault(studyNoteId) ?? [];
}
