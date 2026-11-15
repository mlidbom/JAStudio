using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.Studying;

/// <summary>
/// Loads what the collection needs of the study-notes from the spaced repetition system (<see cref="SpacedRepetitionSystemData"/>), as the<br/>
/// collection loads and whenever it reads the study-notes again: which note each study-note belongs to, the studying statuses of its cards, and<br/>
/// its tags. The integration with a spaced repetition system implements it: for Anki, <c>JAStudio.Anki</c>.
/// </summary>
public interface ISpacedRepetitionSystemDataLoader
{
   //review-naming:studying: ISpacedRepetitionSystemDataLoader was IBackendDataLoader (Claude, 2026-11-15): "the spaced repetition system" for
   //   "the backend", and no more than that. What it loads is the study-notes, without their fields.
   /// <summary>Reads it from the spaced repetition system now, showing the progress through <paramref name="taskRunner"/>.</summary>
   SpacedRepetitionSystemData Load(TaskRunner taskRunner);
}
