using System;
using System.IO;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Anki;
using JAStudio.Core.Specifications.Anki.BulkLoaderTests;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Anki;

/// <summary>
/// JAStudio reads Anki's database file while Anki has it open and keeps changing it, through connections that tell SQLite the file never changes<br/>
/// (<c>immutable=1</c>), the only way to read it past Anki's lock. So no connection may outlive its read, whether left open or kept in a pool: a<br/>
/// later read through it would use the pages it cached before Anki changed them, and read wrong data, or fail with "database disk image is<br/>
/// malformed" from then on, until Anki restarts. Here the study-notes' tags are read from a copy of the test collection, which is then deleted, as<br/>
/// only a file no connection holds open can be.
/// </summary>
public sealed class When_the_study_notes_tags_have_been_read_from_Ankis_database : IAIGeneratedSpec, IDisposable
{
   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-spec-");
   readonly string _collection;

   public When_the_study_notes_tags_have_been_read_from_Ankis_database()
   {
      _collection = Path.Combine(_folder.FullName, "collection.anki2");
      File.Copy(AnkiTestDb.Path, _collection);
      StudyNoteTagsLoader.Load(_collection);
      File.Delete(_collection);
   }

   [XF] public void no_connection_holds_the_database_file_open() => File.Exists(_collection).Must().BeFalse();

   public void Dispose() => _folder.Delete(recursive: true);
}
