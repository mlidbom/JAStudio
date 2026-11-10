using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core;

/// <summary>
/// Loads what the collection needs from the student's Anki as it loads: which corpus note each Anki note is, the studying statuses of the cards,<br/>
/// and the study-notes' tags. Implemented by the integration layer (e.g. JAStudio.Anki).
/// </summary>
public interface IBackendDataLoader
{
   BackendData Load(TaskRunner taskRunner);
}

/// <summary>
/// Data loaded from the backend system that is needed to fully populate caches.
/// Contains the mapping between external IDs and domain NoteIds,
/// plus per-card studying status information, and the tags of each study-note in Anki.
/// </summary>
public class BackendData
{
   public Dictionary<long, NoteId> IdMappings { get; }
   public List<CardStudyingStatus> StudyingStatuses { get; }

   /// <summary>The tags each study-note has in Anki, by its Anki note id: what each note's <see cref="Note.Tagging.AnkiTags"/> are read from.</summary>
   public Dictionary<long, IReadOnlyList<string>> StudyNoteTags { get; }

   public BackendData(Dictionary<long, NoteId> idMappings, List<CardStudyingStatus> studyingStatuses, Dictionary<long, IReadOnlyList<string>> studyNoteTags)
   {
      IdMappings = idMappings;
      StudyingStatuses = studyingStatuses;
      StudyNoteTags = studyNoteTags;
   }
}
