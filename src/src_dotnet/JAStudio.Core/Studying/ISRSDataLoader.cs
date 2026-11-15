using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.Studying;

/// <summary>
/// Loads what the collection needs of the study-notes from the SRS, the spaced repetition system (<see cref="SRSData"/>), as the collection<br/>
/// loads and whenever it reads the study-notes again: which note each study-note belongs to, the studying statuses of its cards, and its<br/>
/// tags. The integration with a spaced repetition system implements it: for Anki, <c>JAStudio.Anki</c>.
/// </summary>
/// <remarks>
/// review-naming:studying: ISRSDataLoader was IBackendDataLoader (Claude, 2026-11-15): "the SRS" for "the backend", and no more than that. What it<br/>
/// loads is the study-notes, without their fields.
/// </remarks>
public interface ISRSDataLoader
{
   /// <summary>Reads it from the spaced repetition system now, showing the progress through <paramref name="taskRunner"/>.</summary>
   SRSData Load(TaskRunner taskRunner);
}
