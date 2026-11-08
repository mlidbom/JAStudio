using System;
using System.Threading.Tasks;
using JAStudio.Core;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Anki;

/// <summary>
/// Loads Anki-specific user data (ID mappings and studying statuses) from Anki's SQLite database.
/// </summary>
public class AnkiBackendDataLoader : IBackendDataLoader
{
   public BackendData Load(TaskRunner taskRunner)
   {
      var dbPath = AnkiFacade.Col.DbFilePath()
                ?? throw new InvalidOperationException("Anki collection database is not initialized yet");

      using var runner = taskRunner.Current("Loading user data from Anki");

      var ankiIdMapTask = runner.RunIndeterminateAsync("Loading Anki ID mappings", () => AnkiNoteIdMapLoader.Load(dbPath));
      var studyingStatusesTask = runner.RunIndeterminateAsync("Fetching studying statuses from Anki", () => CardStudyingStatusLoader.FetchAll(dbPath));

      Task.WaitAll(ankiIdMapTask, studyingStatusesTask);

      return new BackendData(ankiIdMapTask.Result, studyingStatusesTask.Result);
   }
}
