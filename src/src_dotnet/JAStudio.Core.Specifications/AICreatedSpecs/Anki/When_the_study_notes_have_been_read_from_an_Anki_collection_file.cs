using System.IO;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Specifications.Anki;
using JAStudio.Core.Specifications.Anki.BulkLoaderTests;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Anki;

/// <summary>
/// The headless JAStudio reads an Anki collection's file while Anki has it open and keeps changing it (<see cref="AnkiCollectionFile"/>), through<br/>
/// connections that tell SQLite the file never changes (<c>immutable=1</c>), the only way to read it past Anki's lock. So no connection may<br/>
/// outlive its read, whether left open or kept in a pool: a later read through it would use the pages it cached before Anki changed them, and<br/>
/// read wrong data, or fail with "database disk image is malformed" from then on, until Anki restarts. Here the study-notes are read from a copy<br/>
/// of the test collection, as the collection loads them, which is then deleted, as only a file no connection holds open can be.
/// </summary>
public sealed class When_the_study_notes_have_been_read_from_an_Anki_collection_file : SpecificationReadingAnAnkiCollectionFile, IAIGeneratedSpec
{
   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-spec-");
   readonly string _collection;

   public When_the_study_notes_have_been_read_from_an_Anki_collection_file()
   {
      _collection = Path.Combine(_folder.FullName, "collection.anki2");
      File.Copy(AnkiTestDb.Path, _collection);
      WhatTheCollectionLoadsOfTheStudyNotesIn(new AnkiCollectionFile(new FileInfo(_collection)));
      File.Delete(_collection);
   }

   [XF] public void no_connection_holds_the_file_open() => File.Exists(_collection).Must().BeFalse();

   public override void Dispose()
   {
      _folder.Delete(recursive: true);
      base.Dispose();
   }
}
