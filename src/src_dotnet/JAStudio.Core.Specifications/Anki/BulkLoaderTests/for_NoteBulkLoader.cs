using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Anki;
using JAStudio.Core.Note;
using Xunit;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.Anki.BulkLoaderTests;

public class for_vocab_notes
{
   public class after_loading_all : for_vocab_notes
   {
      readonly AnkiBulkLoadResult _result = NoteBulkLoader.LoadAllNotesOfType(AnkiTestDb.Path, NoteTypes.Vocab, g => new VocabId(g));

      [XF] public void the_result_is_not_empty() => _result.Notes.Must().NotBeEmpty();
      [XF] public void every_note_has_an_id() => _result.Notes.All(n => n.Id != null).Must().BeTrue();
      [XF] public void every_note_has_fields() => _result.Notes.All(n => n.Fields.Count > 0).Must().BeTrue();
      [XF] public void the_first_note_has_a_Q_field() => _result.Notes.First().Fields.ContainsKey("Q").Must().BeTrue();
      [XF] public void the_first_note_has_an_A_field() => _result.Notes.First().Fields.ContainsKey("A").Must().BeTrue();
      [XF] public void the_first_note_has_a_Reading_field() => _result.Notes.First().Fields.ContainsKey("Reading").Must().BeTrue();
   }
}

public class for_kanji_notes
{
   public class after_loading_all : for_kanji_notes
   {
      readonly AnkiBulkLoadResult _result = NoteBulkLoader.LoadAllNotesOfType(AnkiTestDb.Path, NoteTypes.Kanji, g => new KanjiId(g));

      [XF] public void the_result_is_not_empty() => _result.Notes.Must().NotBeEmpty();
      [XF] public void every_note_has_an_id() => _result.Notes.All(n => n.Id != null).Must().BeTrue();
      [XF] public void the_first_note_has_a_Q_field() => _result.Notes.First().Fields.ContainsKey("Q").Must().BeTrue();
   }
}

public class for_sentence_notes
{
   public class after_loading_all : for_sentence_notes
   {
      readonly AnkiBulkLoadResult _result = NoteBulkLoader.LoadAllNotesOfType(AnkiTestDb.Path, NoteTypes.Sentence, g => new SentenceId(g));

      [XF] public void the_result_is_not_empty() => _result.Notes.Must().NotBeEmpty();
      [XF] public void every_note_has_an_id() => _result.Notes.All(n => n.Id != null).Must().BeTrue();
      [XF] public void the_first_note_has_a_Q_field() => _result.Notes.First().Fields.ContainsKey("Q").Must().BeTrue();
   }
}

public class for_an_unknown_note_type
{
   [XF] public void loading_throws_KeyNotFoundException() =>
      Assert.Throws<KeyNotFoundException>(() =>
         NoteBulkLoader.LoadAllNotesOfType(AnkiTestDb.Path, "NonExistentNoteType", g => new VocabId(g)));
}
