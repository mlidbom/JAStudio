using System;
using System.IO;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Reservoir.Reporting;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;
using Microsoft.Data.Sqlite;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>
/// A deck nobody has looked at, checked before filling the reservoir from it (<see cref="WhatADeckGives"/>, <see cref="DeckCheckReport"/>): one note<br/>
/// type of sentences, one of words in a field named as a sentence's often is, as a vocabulary deck can have, and one whose notes hold no<br/>
/// Japanese at all.
/// </summary>
public class When_checking_what_decks_would_give_the_reservoir : IAIGeneratedSpec, IDisposable
{
   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-deck-check-spec-");
   readonly WhatADeckGives _deck;

   public When_checking_what_decks_would_give_the_reservoir()
   {
      var decksFolder = _folder.CreateSubdirectory("decks");
      var deckFolder = decksFolder.CreateSubdirectory("a deck");
      CreateTheCollection(new FileInfo(Path.Combine(deckFolder.FullName, "collection.anki2")));
      _deck = WhatADeckGives.Of(DeckOnDisk.EveryDeckIn(decksFolder).Single());
   }

   [XF] public void each_note_type_with_notes_is_checked() => _deck.NoteTypes.Select(it => it.Roles.NoteType.Name).Order().Must().SequenceEqual(["basic", "sentences", "words"]);

   [XF] public void the_note_type_of_sentences_gives_each_notes_sentence() =>
      TheNoteType("sentences").Examples.Select(it => it.Text).Must().SequenceEqual(["友人帳を寄越せ", "いやだ、絶対に渡さない"]);

   [XF] public void the_note_type_of_sentences_is_not_taken_for_one_of_words() => TheNoteType("sentences").GivesWordsRatherThanSentences.Must().BeFalse();

   [XF] public void the_note_type_of_words_is_taken_for_one() => TheNoteType("words").GivesWordsRatherThanSentences.Must().BeTrue();

   [XF] public void the_note_type_with_no_japanese_gives_nothing_from_its_notes() => TheNoteType("basic").GivesNothingFromItsNotes.Must().BeTrue();

   [XF] public void the_report_lists_every_note_type_in_its_file()
   {
      var reportFolder = _folder.CreateSubdirectory("report");
      DeckCheckReport.WriteTo(reportFolder, _folder, [_deck]);
      File.ReadAllLines(Path.Combine(reportFolder.FullName, "note-types.tsv")).Skip(1).Select(it => it.Split('\t')[1]).Order().Must().SequenceEqual(["basic", "sentences", "words"]);
   }

   WhatANoteTypeGives TheNoteType(string name) => _deck.NoteTypes.Single(it => it.Roles.NoteType.Name == name);

   public void Dispose()
   {
      SqliteConnection.ClearAllPools();
      _folder.Delete(recursive: true);
   }

   /// <summary>A collection in the format of <c>collection.anki2</c>, its note types as JSON in the <c>col</c> table, with three note types and their notes.</summary>
   static void CreateTheCollection(FileInfo file)
   {
      using var connection = new SqliteConnection($"Data Source={file.FullName};Pooling=False");
      connection.Open();
      Execute("""
              create table col(models text);
              insert into col values('{"1": {"name": "sentences", "flds": [{"name": "Expression", "ord": 0}, {"name": "Meaning", "ord": 1}]},
                                       "2": {"name": "words", "flds": [{"name": "Expression", "ord": 0}, {"name": "Meaning", "ord": 1}]},
                                       "3": {"name": "basic", "flds": [{"name": "Front", "ord": 0}, {"name": "Back", "ord": 1}]}}');
              create table notes(id integer primary key, mid integer, flds text, tags text);
              """);
      AddNote(1, 1, "友人帳を寄越せ", "Hand it over!");
      AddNote(2, 1, "いやだ、絶対に渡さない", "No, I will never give it to you.");
      AddNote(3, 2, "猫", "cat");
      AddNote(4, 2, "犬", "dog");
      AddNote(5, 2, "たべる", "to eat");
      AddNote(6, 3, "Hello", "World");
      return;

      void AddNote(int id, int noteTypeId, string first, string second) => Execute($"insert into notes values({id}, {noteTypeId}, '{first}\x1f{second}', '')");

      void Execute(string sql)
      {
         using var command = connection.CreateCommand();
         command.CommandText = sql;
         command.ExecuteNonQuery();
      }
   }
}
