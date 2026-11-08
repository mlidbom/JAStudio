using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// What JAStudio writes into a note's Anki note, its study-note: exactly the fields the trimmed Anki note types have. A field the note type no longer<br/>
/// has would make every write of the note fail.
/// </summary>
public class When_a_note_is_written_to_its_Anki_study_note : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   static string[] FieldsWrittenFor(JPNote note) => [.. note.GetData().Fields.Keys.Order()];

   [XF] public void a_vocab_writes_its_question_its_three_audio_fields_and_its_corpus_note_id() =>
      FieldsWrittenFor(CreateVocab("走る", "to run", "はしる")).Must().SequenceEqual(new[] { "Audio_TTS", "Audio_b", "Audio_g", "Q", "jas_note_id" }.Order());

   [XF] public void a_kanji_writes_its_question_and_its_corpus_note_id() =>
      FieldsWrittenFor(CreateKanji("川", "river", "セン", "かわ")).Must().SequenceEqual(new[] { "Q", "jas_note_id" }.Order());

   [XF] public void a_sentence_writes_its_question_its_audio_and_its_corpus_note_id() =>
      FieldsWrittenFor(CreateTestSentence("川に行く", "go to the river")).Must().SequenceEqual(new[] { "Audio Sentence", "Q", "jas_note_id" }.Order());

   [XF] public void a_vocab_with_a_disambiguation_writes_its_disambiguation_name_as_its_question() =>
      CreateVocabWithTheDisambiguation("入る", "masu").GetData().Fields["Q"].Must().Be("入る:masu");

   VocabNote CreateVocabWithTheDisambiguation(string question, string disambiguation)
   {
      var vocab = CreateVocab(question, "to enter", "いる");
      vocab.Question.SetDisambiguation(disambiguation);
      return vocab;
   }
}
