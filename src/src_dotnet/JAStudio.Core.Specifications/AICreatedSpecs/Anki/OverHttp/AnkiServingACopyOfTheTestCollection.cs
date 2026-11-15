using System;
using System.IO;
using System.Text.Json;
using JAStudio.Anki.AnkisFunctions.OverHttp;

namespace JAStudio.Core.Specifications.AICreatedSpecs.Anki.OverHttp;

/// <summary>
/// Anki as a specification calls it over HTTP: the Anki add-on's own HTTP server for JAStudio, serving a copy of the test collection that no other<br/>
/// specification sees, opened with Anki's own library, without Anki's window. It serves until it's disposed, which also removes the copy.
/// </summary>
/// <remarks>
/// The test collection has 1,217 notes, of JAStudio's three note types, and 1,749 cards, none suspended. Its vocab note 215 has the tags<br/>
/// <c>-::vocab::has-no-studying-sentences</c>, <c>legacy_scheduling</c> and <c>source::wani::level::04</c>.
/// </remarks>
sealed class AnkiServingACopyOfTheTestCollection : IDisposable
{
   readonly int _id;

   /// <param name="started">Python's answer to the command <c>start</c>.</param>
   internal AnkiServingACopyOfTheTestCollection(JsonElement started)
   {
      _id = started.GetProperty("id").GetInt32();
      Access = new AnkisHttpServerAccess(started.GetProperty("port").GetInt32(), started.GetProperty("token").GetString() ?? "");
      Collection = new FileInfo(started.GetProperty("collection").GetString() ?? "");
   }

   /// <summary>How to reach the server.</summary>
   public AnkisHttpServerAccess Access { get; }

   /// <summary>The file of the copy of the test collection, which is the collection this Anki has open.</summary>
   public FileInfo Collection { get; }

   /// <summary>The folder the copy is in, which holds nothing else but what Anki keeps beside a collection, such as its media folder. It's removed with the copy.</summary>
   public DirectoryInfo Folder => Collection.Directory ?? throw new InvalidOperationException($"{Collection.FullName} is in no folder.");

   /// <summary>Anki's own sync, with AnkiWeb, starts: until it has finished, Anki refuses JAStudio's calls.</summary>
   public void ItsOwnSyncStarts() => AnkisHttpServersForSpecifications.AnswerTo($"ankis-own-sync-starts {_id}");

   /// <summary>Anki's own sync, with AnkiWeb, has finished: Anki answers JAStudio's calls again.</summary>
   public void ItsOwnSyncFinishes() => AnkisHttpServersForSpecifications.AnswerTo($"ankis-own-sync-finishes {_id}");

   public void Dispose() => AnkisHttpServersForSpecifications.AnswerTo($"stop {_id}");
}
