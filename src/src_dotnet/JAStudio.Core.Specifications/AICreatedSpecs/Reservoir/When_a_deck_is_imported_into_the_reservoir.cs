using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Compze.Must;
using Compze.Contracts.Exceptions;
using Compze.xUnitBDD;
using JAStudio.Core.Reservoir.Importing;
using JAStudio.Core.Reservoir.Storage.Format;
using JAStudio.Core.Reservoir.Storage.Media;
using Microsoft.Data.Sqlite;

using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>
/// A small subs2srs deck imported into the reservoir (<see cref="DeckImporter"/>): two notes, each with a screenshot, the first with audio its<br/>
/// package lacks; three image files, two of them the same content under different names.
/// </summary>
public class When_a_deck_is_imported_into_the_reservoir : IAIGeneratedSpec, IDisposable
{
   static readonly byte[] AScreenshot = Encoding.ASCII.GetBytes("a screenshot");
   static readonly byte[] AnotherScreenshot = Encoding.ASCII.GetBytes("another screenshot");

   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-importing-spec-");
   readonly DirectoryInfo _deckFolder;
   readonly DirectoryInfo _mediaFolder;
   readonly FileInfo _package;
   readonly ImportedDeck _imported;

   public When_a_deck_is_imported_into_the_reservoir()
   {
      _package = CreateThePackage();
      _mediaFolder = _folder.CreateSubdirectory("media");
      _imported = ImporterInto("text").Import(_package);
      _deckFolder = new DirectoryInfo(Path.Combine(_folder.FullName, "text", "decks", "show"));
   }

   [XF] public void the_manifest_gives_the_formats_version() => Json("text", "manifest.json").GetProperty("formatVersion").GetInt32().Must().Be(Manifest.CurrentFormatVersion);

   [XF] public void the_collection_database_is_stored_as_the_package_has_it() => Path.Combine(_deckFolder.FullName, "collection.anki2").Must().Satisfy(File.Exists);

   [XF] public void the_companion_file_maps_the_note_types_fields() =>
      Json("text", "decks", "show", "deck.json").GetProperty("noteTypes")[0].GetProperty("japaneseField").Text().Must().Be("Expression");

   [XF] public void every_note_is_a_line_with_its_fields_as_they_are() =>
      NoteLines().Select(it => it.GetProperty("fields").GetProperty("Expression").Text()).Must().SequenceEqual(["（夏目）友人帳を 寄越せ", "いやだ、渡さない"]);

   [XF] public void a_notes_line_gives_its_sentence_cleaned() => NoteLines()[0].GetProperty("sourceData").GetProperty("question").Text().Must().Be("友人帳を 寄越せ");

   [XF] public void a_media_reference_has_the_hash_its_file_is_stored_under() =>
      NoteLines()[0].GetProperty("sourceData").GetProperty("screenshots")[0].GetProperty("hash").Text().Must().Be(Sha256Of(AScreenshot));

   [XF] public void a_reference_to_a_file_the_package_lacks_has_no_hash() =>
      NoteLines()[0].GetProperty("sourceData").GetProperty("audio")[0].TryGetProperty("hash", out _).Must().BeFalse(); //todo:compze: Satisfy on the JSON once Must's failure message can serialize System.Text.Json values

   [XF] public void the_same_content_under_two_names_is_stored_once() => $"{_imported.Media.Files.Count} files, {_imported.Media.FilesNewToTheStore} new".Must().Be("3 files, 2 new");

   [XF] public void a_file_is_stored_under_its_hash_in_a_folder_by_the_hashs_first_two_bytes()
   {
      var hash = Sha256Of(AnotherScreenshot);
      Path.Combine(_mediaFolder.FullName, hash[..2], hash[2..4], hash + ".jpg").Must().Satisfy(File.Exists);
   }

   [XF] public void the_reference_to_the_audio_the_package_lacks_is_counted() => _imported.MediaReferencesToFilesThePackageLacks.Must().Be(1);

   [XF] public void each_notes_line_has_an_id_of_its_own() => NoteLines().Select(it => it.GetProperty("id").GetGuid()).Distinct().Count().Must().Be(2);

   [XF] public void its_lines_end_with_a_line_feed_alone_on_every_platform() =>
      new[] { "deck.json", "notes.jsonl" }.Must().Satisfy(files => !files.Any(it => File.ReadAllText(Path.Combine(_deckFolder.FullName, it)).Contains('\r')));

   [XF] public void the_deck_is_imported_already_for_a_later_run() => ImporterInto("text").HasImported(_package).Must().BeTrue();

   [XF] public void another_deck_is_not_imported_yet() => ImporterInto("text").HasImported(_package.CopyTo(Path.Combine(_folder.FullName, "another show.apkg"))).Must().BeFalse();

   [XF] public void importing_it_again_is_refused_as_a_stored_decks_files_are_never_written_again_from_its_package() =>
      Invoking(() => ImporterInto("text").Import(_package)).Must().Throw<StateAssertionFailedException>();

   [XF] public void a_deck_whose_import_stopped_before_its_companion_file_was_written_is_imported_anew()
   {
      File.Delete(Path.Combine(_deckFolder.FullName, "deck.json"));
      ImporterInto("text").Import(_package);
      ImporterInto("text").HasImported(_package).Must().BeTrue();
   }

   [XF] public void another_package_named_as_it_is_is_refused_rather_than_taken_for_it()
   {
      var anotherPackage = new FileInfo(Path.Combine(_folder.CreateSubdirectory("another uploader").FullName, _package.Name));
      _package.CopyTo(anotherPackage.FullName);
      using(var archive = ZipFile.Open(anotherPackage.FullName, ZipArchiveMode.Update)) archive.WriteEntry("readme.txt", Encoding.ASCII.GetBytes("another upload"));
      Invoking(() => ImporterInto("text").HasImported(anotherPackage)).Must().Throw<StateAssertionFailedException>();
   }

   public void Dispose()
   {
      SqliteConnection.ClearAllPools();
      _folder.Delete(recursive: true);
   }

   DeckImporter ImporterInto(string textFolder) => new(_folder.CreateSubdirectory(textFolder), new MediaStore(_mediaFolder));

   static string Sha256Of(byte[] content) => Convert.ToHexStringLower(SHA256.HashData(content));

   JsonElement Json(params string[] path) => JsonDocument.Parse(File.ReadAllText(Path.Combine([_folder.FullName, .. path]))).RootElement;

   JsonElement[] NoteLines() => [.. File.ReadAllLines(Path.Combine(_deckFolder.FullName, "notes.jsonl")).Select(it => JsonDocument.Parse(it).RootElement)];

   /// <summary>
   /// The package <c>show.apkg</c>: a <c>collection.anki2</c> with the note type subs2srs and two notes, and three images, numbered as Anki numbers<br/>
   /// them, with the JSON media map of the older packages.
   /// </summary>
   FileInfo CreateThePackage()
   {
      var collection = new FileInfo(Path.Combine(_folder.FullName, "collection.anki2"));
      using(var connection = new SqliteConnection($"Data Source={collection.FullName};Pooling=False"))
      {
         connection.Open();
         using var command = connection.CreateCommand();
         command.CommandText = """
                               create table col(models text);
                               insert into col values('{"1": {"name": "subs2srs", "flds": [{"name": "Expression", "ord": 0}, {"name": "Meaning", "ord": 1}, {"name": "Audio", "ord": 2}, {"name": "Snapshot", "ord": 3}]}}');
                               create table notes(id integer primary key, mid integer, flds text, tags text);
                               """
                             + "insert into notes values(1, 1, '（夏目）友人帳を 寄越せ\x1fHand it over!\x1f[sound:show_01.mp3]\x1f<img src=\"show_01.jpg\">', ' show ');"
                             + "insert into notes values(2, 1, 'いやだ、渡さない\x1fNo, I will not.\x1f\x1f<img src=\"show_02.jpg\">', ' show ');";
         command.ExecuteNonQuery();
      }

      var package = new FileInfo(Path.Combine(_folder.FullName, "show.apkg"));
      using(var archive = ZipFile.Open(package.FullName, ZipArchiveMode.Create))
      {
         archive.CreateEntryFromFile(collection.FullName, "collection.anki2");
         archive.WriteEntry("media", Encoding.UTF8.GetBytes("""{"0": "show_01.jpg", "1": "show_02.jpg", "2": "show_01_again.jpg"}"""));
         archive.WriteEntry("0", AScreenshot);
         archive.WriteEntry("1", AnotherScreenshot);
         archive.WriteEntry("2", AScreenshot);
      }

      package.Refresh();
      return package;
   }
}
