using System.Linq;
using Compze.Internals.Logging;
using Compze.Tessaging.TessageBus;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// Logs the notes each unit of work saved, once it has committed, when the settings say to log flushing notes<br/>
/// (<see cref="Configuration.Settings.LogWhenFlushingNotes"/>): for a developer following what JAStudio writes to the corpus.
/// </summary>
static class SavedNotesLog
{
   static readonly ILogger Log = CompzeLogger.For(typeof(SavedNotesLog));

   /// <summary>Declares, at composition, the observer of each unit of work's <see cref="INoteTevent"/>s that logs the notes it saved.</summary>
   internal static void LogTheNotesEachUnitOfWorkSaved(TeventObservationRegistrar observe) =>
      observe.ForTeventsOfEachUnitOfWork<INoteTevent, JPCollection>((tevents, collection) =>
      {
         if(!collection.NoteServices.Settings.LogWhenFlushingNotes()) return;

         foreach(var noteId in tevents.Select(it => it.NoteId).Distinct())
            Log.Info(collection.NoteFromNoteId(noteId) is {} note ? $"Saved the {note.GetType().Name} {note.GetQuestion()}" : $"Deleted the note {noteId}");
      });
}
