using JAStudio.Core;
using JAStudio.Core.TaskRunners;

namespace JAStudio.StandaloneHost;

/// <summary>
/// What the standalone JAStudio loads from Anki of the study-notes: nothing, since it has no Anki. So no note has a study-note, nor its cards' studying<br/>
/// statuses, nor Anki tags: every note shows as not studied.
/// </summary>
sealed class NoAnkiData : IBackendDataLoader
{
   public BackendData Load(TaskRunner taskRunner) => new([], [], []);
}
