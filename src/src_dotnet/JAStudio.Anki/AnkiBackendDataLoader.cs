using System;
using System.IO;
using System.Threading.Tasks;
using JAStudio.Core;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Anki;

/// <summary>
/// Loads Anki-specific user data (ID mappings and studying statuses) from an Anki collection's SQLite database:<br/>
/// inside Anki, the collection Anki has open; in the headless JAStudio, the collection file of an Anki profile, read while Anki may be running.
/// </summary>
public class AnkiBackendDataLoader : IBackendDataLoader
{
   readonly Func<string> _findTheCollectionDatabaseFile;

   AnkiBackendDataLoader(Func<string> findTheCollectionDatabaseFile) => _findTheCollectionDatabaseFile = findTheCollectionDatabaseFile;

   /// <summary>Loads from the collection Anki has open when <see cref="Load"/> runs, which changes as Magnus switches Anki profiles.</summary>
   public static AnkiBackendDataLoader ForTheCollectionAnkiHasOpen() =>
      new(() => AnkiFacade.Col.DbFilePath() ?? throw new InvalidOperationException("Anki collection database is not initialized yet"));

   /// <summary>
   /// Loads from the Anki collection database <paramref name="collectionDatabaseFile"/>, an Anki profile's <c>collection.anki2</c>.<br/>
   /// Anki's latest changes may still be in the database's write-ahead log while Anki runs, and reading the file doesn't see them.
   /// </summary>
   public static AnkiBackendDataLoader ForTheCollectionIn(FileInfo collectionDatabaseFile) => new(() => collectionDatabaseFile.FullName);

   public BackendData Load(TaskRunner taskRunner)
   {
      var dbPath = _findTheCollectionDatabaseFile();

      using var runner = taskRunner.Current("Loading user data from Anki");

      var ankiIdMapTask = runner.RunIndeterminateAsync("Loading Anki ID mappings", () => AnkiNoteIdMapLoader.Load(dbPath));
      var studyingStatusesTask = runner.RunIndeterminateAsync("Fetching studying statuses from Anki", () => CardStudyingStatusLoader.FetchAll(dbPath));

      Task.WaitAll(ankiIdMapTask, studyingStatusesTask);

      return new BackendData(ankiIdMapTask.Result, studyingStatusesTask.Result);
   }
}
