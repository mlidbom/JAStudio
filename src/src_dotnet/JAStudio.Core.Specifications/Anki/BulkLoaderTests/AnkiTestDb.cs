using System;
using System.IO;
using JAStudio.Anki.CollectionDatabase;

namespace JAStudio.Core.Specifications.Anki.BulkLoaderTests;

static class AnkiTestDb
{
   internal static readonly string Path =
      Environment.GetEnvironmentVariable("ANKI_TEST_DB_PATH")
   ?? System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "collection.anki2"));

   /// <summary>The test collection's database, read from its file.</summary>
   internal static AnkiCollectionFile Database => new(new FileInfo(Path));
}
