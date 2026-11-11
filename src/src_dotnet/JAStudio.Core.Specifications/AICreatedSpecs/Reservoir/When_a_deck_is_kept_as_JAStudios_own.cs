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
using JAStudio.Core.Reservoir.Keeping;
using JAStudio.Core.Reservoir.Keeping.Media;
using Microsoft.Data.Sqlite;

using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>
/// A small subs2srs deck kept as JAStudio's own (<see cref="ReservoirKeeping"/>): two notes, each with a screenshot, the first with audio its<br/>
/// package lacks; three image files, two of them the same content under different names.
/// </summary>
public class When_a_deck_is_kept_as_JAStudios_own : IAIGeneratedSpec, IDisposable
{
   static readonly byte[] AScreenshot = Encoding.ASCII.GetBytes("a screenshot");
   static readonly byte[] AnotherScreenshot = Encoding.ASCII.GetBytes("another screenshot");

   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-keeping-spec-");
   readonly DirectoryInfo _deckFolder;
   readonly DirectoryInfo _mediaFolder;
   readonly FileInfo _package;
   readonly KeptDeck _kept;

   public When_a_deck_is_kept_as_JAStudios_own()
   {
      _package = CreateThePackage();
      _mediaFolder = _folder.CreateSubdirectory("media");
      _kept = KeepingIn("text").Keep(_package);
      _deckFolder = new DirectoryInfo(Path.Combine(_folder.FullName, "text", "decks", "show"));
   }

   [XF] public void the_manifest_gives_the_formats_version() => Json("text", "manifest.json").GetProperty("formatVersion").GetInt32().Must().Be(ReservoirKeeping.FormatVersion);

   [XF] public void the_collection_database_is_kept_as_the_package_has_it() => File.Exists(Path.Combine(_deckFolder.FullName, "collection.anki2")).Must().BeTrue();

   [XF] public void the_companion_file_maps_the_note_types_fields() =>
      Json("text", "decks", "show", "deck.json").GetProperty("noteTypes")[0].GetProperty("japaneseField").Text().Must().Be("Expression");

   [XF] public void every_note_is_a_line_with_its_fields_as_they_are() =>
      NoteLines().Select(it => it.GetProperty("fields").GetProperty("Expression").Text()).Must().SequenceEqual(["（夏目）友人帳を 寄越せ", "いやだ、渡さない"]);

   [XF] public void a_notes_line_gives_its_sentence_cleaned() => NoteLines()[0].GetProperty("sourceData").GetProperty("question").Text().Must().Be("友人帳を 寄越せ");

   [XF] public void a_media_reference_has_the_hash_its_file_is_kept_under() =>
      NoteLines()[0].GetProperty("sourceData").GetProperty("screenshots")[0].GetProperty("hash").Text().Must().Be(Sha256Of(AScreenshot));

   [XF] public void a_reference_to_a_file_the_package_lacks_has_no_hash() =>
      NoteLines()[0].GetProperty("sourceData").GetProperty("audio")[0].TryGetProperty("hash", out _).Must().BeFalse();

   [XF] public void the_same_content_under_two_names_is_stored_once() => $"{_kept.Media.Files.Count} files, {_kept.Media.FilesNewToTheStore} new".Must().Be("3 files, 2 new");

   [XF] public void a_file_is_stored_under_its_hash_in_a_folder_by_the_hashs_first_two_bytes()
   {
      var hash = Sha256Of(AnotherScreenshot);
      File.Exists(Path.Combine(_mediaFolder.FullName, hash[..2], hash[2..4], hash + ".jpg")).Must().BeTrue();
   }

   [XF] public void the_reference_to_the_audio_the_package_lacks_is_counted() => _kept.MediaReferencesToFilesThePackageLacks.Must().Be(1);

   [XF] public void keeping_the_deck_again_gives_the_same_files()
   {
      KeepingIn("text-again").Keep(_package);
      string[] files = [Path.Combine("decks", "show", "deck.json"), Path.Combine("decks", "show", "notes.jsonl")];
      files.Select(it => File.ReadAllText(Path.Combine(_folder.FullName, "text-again", it)))
           .Must().SequenceEqual(files.Select(it => File.ReadAllText(Path.Combine(_folder.FullName, "text", it))));
   }

   [XF] public void its_lines_end_with_a_line_feed_alone_on_every_platform() =>
      new[] { "deck.json", "notes.jsonl" }.Any(it => File.ReadAllText(Path.Combine(_deckFolder.FullName, it)).Contains('\r')).Must().BeFalse();

   [XF] public void another_package_named_as_it_is_is_refused_rather_than_kept_over_it()
   {
      var anotherPackage = new FileInfo(Path.Combine(_folder.CreateSubdirectory("another uploader").FullName, _package.Name));
      _package.CopyTo(anotherPackage.FullName);
      using(var archive = ZipFile.Open(anotherPackage.FullName, ZipArchiveMode.Update)) archive.WriteEntry("readme.txt", Encoding.ASCII.GetBytes("another upload"));
      Invoking(() => KeepingIn("text").Keep(anotherPackage)).Must().Throw<StateAssertionFailedException>();
   }

   public void Dispose()
   {
      SqliteConnection.ClearAllPools();
      _folder.Delete(recursive: true);
   }

   ReservoirKeeping KeepingIn(string textFolder) => new(_folder.CreateSubdirectory(textFolder), new MediaStore(_mediaFolder));

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
