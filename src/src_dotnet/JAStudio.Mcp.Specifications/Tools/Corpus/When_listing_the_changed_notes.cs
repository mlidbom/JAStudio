using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Mcp.Tools.Corpus;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Corpus;

/// <summary>
/// The corpus repository has committed files for the vocab 行く and 来る, the kanji 行 and the sentence 明日学校に行く. Since then, 行く's file has<br/>
/// changed, 行's file has been deleted, and a file has been added for a note the collection doesn't have.
/// </summary>
public class When_listing_the_changed_notes : CorpusRepositorySpecification
{
   readonly Guid _idOfANoteTheCollectionDoesNotHave = Guid.NewGuid();
   readonly ChangedNotes _changedNotes;

   public When_listing_the_changed_notes()
   {
      var vocab行く = CreateVocab("行く", "to go", "いく");
      var vocab来る = CreateVocab("来る", "to come", "くる");
      var kanji行 = CreateKanji("行", "go", "コウ", "い");
      var sentence = CreateTestSentence("明日学校に行く", "I'll go to school tomorrow");
      foreach(var note in new JPNote[] { vocab行く, vocab来る, kanji行, sentence }) Write(FileOf(note), "{}");
      CommitEverything();

      Write(FileOf(vocab行く), """{"changed": true}""");
      FileOf(kanji行).Delete();
      Write(FileOfTheNoteWithTheId(_idOfANoteTheCollectionDoesNotHave, "vocab"), "{}");

      _changedNotes = ResultOf<ChangedNotes>(CallTool("list_changed_notes", new Dictionary<string, object?>()));
   }

   [XF] public void the_changed_vocab_is_the_one_vocab_listed() => _changedNotes.Vocab.Notes.Select(it => it.Question).Must().SequenceEqual(["行く"]);
   [XF] public void the_vocab_whose_file_did_not_change_is_not_counted() => _changedNotes.Vocab.Count.Must().Be(1);
   [XF] public void the_kanji_whose_file_was_deleted_is_listed() => _changedNotes.Kanji.Notes.Select(it => it.Question).Must().SequenceEqual(["行"]);
   [XF] public void no_sentence_is_counted() => _changedNotes.Sentences.Count.Must().Be(0);

   [XF] public void the_added_file_of_a_note_the_collection_does_not_have_is_listed_by_its_id() =>
      _changedNotes.IdsOfNotesNotInTheCollection.Must().SequenceEqual([_idOfANoteTheCollectionDoesNotHave.ToString()]);
}
