using System.Text.Json;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A vocab's <see cref="VocabNoteUsuallyWrittenInKana.Decision"/>, written as the corpus's JSON and read back, as JAStudio saves and loads the corpus:<br/>
/// <c>usuallyWrittenInKana</c> is true, false, or null while it's undecided. It isn't stored as a tag.
/// </summary>
public class When_whether_a_vocab_is_usually_written_in_kana_is_saved_to_the_corpus : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly JsonElement _json;
   readonly VocabNote _readBack;

   protected When_whether_a_vocab_is_usually_written_in_kana_is_saved_to_the_corpus(UsuallyWrittenInKanaDecision decision)
   {
      var vocab = CreateVocab("毎月", "every month", "まいつき");
      vocab.UsuallyWrittenInKana.Decide(decision);
      var serializer = GetService<NoteSerializer>();
      var json = serializer.Serialize(vocab);
      _json = JsonDocument.Parse(json).RootElement;
      _readBack = serializer.DeserializeVocab(json);
   }

   JsonValueKind Stored => _json.GetProperty("usuallyWrittenInKana").ValueKind;
   UsuallyWrittenInKanaDecision ReadBack => _readBack.UsuallyWrittenInKana.Decision;

   public class while_it_is_undecided() : When_whether_a_vocab_is_usually_written_in_kana_is_saved_to_the_corpus(UsuallyWrittenInKanaDecision.Undecided)
   {
      [XF] public void the_corpus_json_stores_null() => Stored.Must().Be(JsonValueKind.Null);
      [XF] public void it_is_read_back_undecided() => ReadBack.Must().Be(UsuallyWrittenInKanaDecision.Undecided);
   }

   public class decided_as_yes() : When_whether_a_vocab_is_usually_written_in_kana_is_saved_to_the_corpus(UsuallyWrittenInKanaDecision.Yes)
   {
      [XF] public void the_corpus_json_stores_true() => Stored.Must().Be(JsonValueKind.True);
      [XF] public void it_is_read_back_as_yes() => ReadBack.Must().Be(UsuallyWrittenInKanaDecision.Yes);
      [XF] public void the_corpus_json_stores_no_tags() => _json.GetProperty("tags").EnumerateArray().Must().BeEmpty();
   }

   public class decided_as_no() : When_whether_a_vocab_is_usually_written_in_kana_is_saved_to_the_corpus(UsuallyWrittenInKanaDecision.No)
   {
      [XF] public void the_corpus_json_stores_false() => Stored.Must().Be(JsonValueKind.False);
      [XF] public void it_is_read_back_as_no() => ReadBack.Must().Be(UsuallyWrittenInKanaDecision.No);
   }
}
