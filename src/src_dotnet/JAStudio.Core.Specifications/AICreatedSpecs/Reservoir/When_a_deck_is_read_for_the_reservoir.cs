using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;
using Microsoft.Data.Sqlite;
using ZstdSharp;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>
/// A subs2srs deck, an Anki package, read for the reservoir with no Anki (<see cref="AnkiPackageDatabase"/>, <see cref="AnkiDeck"/>): its notes,<br/>
/// and in them the sentence, its translation, its audio and its screenshot, whichever of Anki's three formats the package holds.
/// </summary>
public abstract class When_a_deck_is_read_for_the_reservoir : IAIGeneratedSpec, IDisposable
{
   static readonly string[] FieldNames = ["SequenceMarker", "Audio", "Snapshot", "Expression", "Meaning", "Reading"];
   static readonly string[] FirstNote = ["01_0101", "[sound:show_01_0.01.mp3]", """<img src="show_01_0.01.jpg">""", "（夏目）友人帳を 寄越せ", "Hand it over!", "友人帳[ゆうじんちょう]を 寄越[よこ]せ"];
   static readonly string[] SecondNote = ["01_0102", "[sound:show_01_0.02.mp3]", """<img src="show_01_0.02.jpg">""", "いやだ", "No way.", "いやだ"];

   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-deck-spec-");
   readonly AnkiPackageDatabase _database;
   readonly AnkiDeck _deck;

   protected When_a_deck_is_read_for_the_reservoir(string collectionFileName)
   {
      var collection = new FileInfo(Path.Combine(_folder.FullName, "collection.sqlite"));
      CreateTheCollection(collection, collectionFileName);
      var package = new FileInfo(Path.Combine(_folder.FullName, "show.apkg"));
      using(var archive = ZipFile.Open(package.FullName, ZipArchiveMode.Create))
      {
         var entry = archive.CreateEntry(collectionFileName);
         using var stream = entry.Open();
         var bytes = File.ReadAllBytes(collection.FullName);
         stream.Write(collectionFileName == "collection.anki21b" ? new Compressor().Wrap(bytes).ToArray() : bytes);
      }

      _database = AnkiPackageDatabase.InPackage(package);
      _deck = new AnkiDeck(_database);
   }

   /// <summary>Its note type, read with its fields in their order.</summary>
   protected void TheNoteTypeIsReadWithItsFieldsInOrder() => _database.NoteTypes.Single().FieldNames.Must().SequenceEqual(FieldNames);

   /// <summary>Its fields, recognized by what they hold.</summary>
   protected void TheFieldsAreRecognized() =>
      _deck.FieldRoles.Single().ToString().Must().Be("subs2srs: Japanese Expression, translation Meaning, audio Audio, image Snapshot");

   /// <summary>Each note's sentence, cleaned, with its translation and its media.</summary>
   protected void EachNoteGivesItsSentence() =>
      _deck.Sentences().Select(it => $"{it.Position} {it.Text} | {it.Translation} | {it.AudioFile} | {it.ImageFile}").Must()
           .SequenceEqual(["note 1 友人帳を 寄越せ | Hand it over! | show_01_0.01.mp3 | show_01_0.01.jpg", "note 2 いやだ | No way. | show_01_0.02.mp3 | show_01_0.02.jpg"]);

   public void Dispose()
   {
      _database.Dispose();
      SqliteConnection.ClearAllPools();
      _folder.Delete(recursive: true);
   }

   public class in_the_format_of_collection_anki2() : When_a_deck_is_read_for_the_reservoir("collection.anki2")
   {
      [XF] public void its_note_type_is_read_with_its_fields_in_order() => TheNoteTypeIsReadWithItsFieldsInOrder();
      [XF] public void the_fields_are_recognized_by_what_they_hold() => TheFieldsAreRecognized();
      [XF] public void each_note_gives_its_sentence_cleaned_with_its_translation_and_media() => EachNoteGivesItsSentence();
   }

   public class in_the_format_of_collection_anki21b() : When_a_deck_is_read_for_the_reservoir("collection.anki21b")
   {
      [XF] public void its_note_type_is_read_with_its_fields_in_order() => TheNoteTypeIsReadWithItsFieldsInOrder();
      [XF] public void the_fields_are_recognized_by_what_they_hold() => TheFieldsAreRecognized();
      [XF] public void each_note_gives_its_sentence_cleaned_with_its_translation_and_media() => EachNoteGivesItsSentence();
   }

   /// <summary>
   /// A collection with one note type, subs2srs, and two notes of it: its note types as JSON in the <c>col</c> table, as the older formats keep<br/>
   /// them, or in tables of their own, as <c>collection.anki21b</c> does.
   /// </summary>
   static void CreateTheCollection(FileInfo file, string collectionFileName)
   {
      using var connection = new SqliteConnection($"Data Source={file.FullName};Pooling=False");
      connection.Open();
      Execute(collectionFileName == "collection.anki21b"
                 ? $"""
                    create table notetypes(id integer primary key, name text);
                    create table fields(ntid integer, ord integer, name text);
                    insert into notetypes values(1342697561419, 'subs2srs');
                    {string.Concat(FieldNames.Select((name, index) => $"insert into fields values(1342697561419, {index}, '{name}');"))}
                    """
                 : $"""
                    create table col(models text);
                    insert into col values('{TheNoteTypesAsJson()}');
                    """);
      Execute("create table notes(id integer primary key, mid integer, flds text, tags text)");
      Execute($"insert into notes values(1, 1342697561419, '{string.Join('\x1f', FirstNote)}', ' show_01 ')");
      Execute($"insert into notes values(2, 1342697561419, '{string.Join('\x1f', SecondNote)}', ' show_01 ')");
      return;

      // The JSON the older formats keep their note types as: by id, each with its name and its fields, each field with its name and its place.
      static string TheNoteTypesAsJson() =>
         """{"1342697561419": {"name": "subs2srs", "flds": [""" + string.Join(", ", FieldNames.Select((name, index) => $$"""{"name": "{{name}}", "ord": {{index}}}""")) + "]}}";

      void Execute(string sql)
      {
         using var command = connection.CreateCommand();
         command.CommandText = sql;
         command.ExecuteNonQuery();
      }
   }
}
