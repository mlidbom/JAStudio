using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JAStudio.Anki.AnkisFunctions.OverHttp;

namespace JAStudio.Core.Specifications.AICreatedSpecs.Anki.OverHttp;

/// <summary>
/// Anki as a specification calls it over HTTP: the Anki add-on's own serving of JAStudio (Python's <c>ServingJAStudio</c>), of a copy of the test<br/>
/// collection that no other specification sees, opened with Anki's own library, without Anki's window. It serves until it's disposed, which also<br/>
/// removes the copy.
/// </summary>
/// <remarks>
/// The test collection has 1,217 notes, of JAStudio's three note types, and 1,749 cards, none suspended. Its vocab note 215 has the tags<br/>
/// <c>-::vocab::has-no-studying-sentences</c>, <c>legacy_scheduling</c> and <c>source::wani::level::04</c>.
/// </remarks>
sealed class AnkiServingACopyOfTheTestCollection : IDisposable
{
   readonly int _id;
   bool _hasStoppedServing;

   /// <param name="started">Python's answer to the command <c>start</c>.</param>
   internal AnkiServingACopyOfTheTestCollection(JsonElement started)
   {
      _id = started.GetProperty("id").GetInt32();
      AccessFile = new AnkisHttpServerAccessFile(new FileInfo(started.GetProperty("accessFile").GetString() ?? ""));
      Collection = new FileInfo(started.GetProperty("collection").GetString() ?? "");
   }

   /// <summary>The access file the add-on wrote as it started serving, in a folder of its own, which is removed when the serving stops.</summary>
   public AnkisHttpServerAccessFile AccessFile { get; }

   /// <summary>How to reach the server, as JAStudio learns it: read from <see cref="AccessFile"/>.</summary>
   public AnkisHttpServerAccess Access => AccessFile.AccessOrNullWhileNoAnkiServes() ?? throw new InvalidOperationException("This Anki has stopped serving, so it has no access file.");

   /// <summary>The file of the copy of the test collection, which is the collection this Anki has open.</summary>
   public FileInfo Collection { get; }

   /// <summary>The folder the copy is in, which holds nothing else but what Anki keeps beside a collection, such as its media folder. It's removed with the copy.</summary>
   public DirectoryInfo Folder => Collection.Directory ?? throw new InvalidOperationException($"{Collection.FullName} is in no folder.");

   /// <summary>Anki's own sync, with AnkiWeb, starts: until it has finished, Anki refuses JAStudio's calls.</summary>
   public void ItsOwnSyncStarts() => AnkisHttpServersForSpecifications.AnswerTo($"ankis-own-sync-starts {_id}");

   /// <summary>Anki's own sync, with AnkiWeb, has finished: Anki answers JAStudio's calls again.</summary>
   public void ItsOwnSyncFinishes() => AnkisHttpServersForSpecifications.AnswerTo($"ankis-own-sync-finishes {_id}");

   /// <summary>
   /// What this Anki's windows were asked for, in order, each in words, such as <c>open the browser on: nid:215</c>: it has no windows, so it<br/>
   /// records each call where Anki's windows would carry it out (Python's <c>WindowsThatRecordWhatTheyAreAskedFor</c>).
   /// </summary>
   public IReadOnlyList<string> WhatItsWindowsWereAskedFor() =>
      [.. AnkisHttpServersForSpecifications.AnswerTo($"what-its-windows-were-asked-for {_id}").GetProperty("askedFor").EnumerateArray().Select(it => it.GetString() ?? "")];

   /// <summary>The add-on stops serving JAStudio, as when Anki's profile closes: its access file and its copy of the collection are removed.</summary>
   public void StopsServing()
   {
      if(_hasStoppedServing) return;

      _hasStoppedServing = true;
      AnkisHttpServersForSpecifications.AnswerTo($"stop {_id}");
   }

   public void Dispose() => StopsServing();
}
