using System.Linq;
using System.Text.Json;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;
using static JAStudio.Core.Specifications.WordReferenceNames;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A vocab's compound parts: the primary ones, the vocab split into words in order, and the alternative ones, a set of words from other ways of<br/>
/// understanding it. Here <c>忘れられない</c>, split into <c>忘れる</c> and <c>られない</c>, with the alternative parts entered as<br/>
/// <c>忘れられる</c>, <c>ない</c> and <c>られない</c>, the last of which is a primary part as well.
/// </summary>
public class When_a_vocab_has_alternative_compound_parts : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly ITaggregateReference<VocabNote> _vocabReference;
   VocabNote _vocab => _vocabReference.Current;

   public When_a_vocab_has_alternative_compound_parts()
   {
      _vocabReference = CreateVocab("忘れられない", "can't forget", "わすれられない").Reference();
      _vocab.Edit(it =>
      {
         it.CompoundParts.SetPrimary(Words("忘れる", "られない"));
         it.CompoundParts.SetAlternative(Words("忘れられる", "ない", "られない"));
      });
   }

   [XF] public void the_primary_parts_keep_their_order() => NamesOf(_vocab.CompoundParts.Primary()).Must().SequenceEqual(["忘れる", "られない"]);

   [XF] public void the_part_that_is_also_a_primary_part_is_not_an_alternative_part() => NamesOf(_vocab.CompoundParts.Alternative()).Must().Satisfy(it => !it.Contains("られない"));

   [XF] public void the_alternative_parts_are_in_a_fixed_order_whatever_order_they_were_entered_in() =>
      NamesOf(_vocab.CompoundParts.Alternative()).Must().SequenceEqual(["ない", "忘れられる"]);

   [XF] public void all_the_parts_are_the_primary_parts_followed_by_the_alternative_parts() =>
      NamesOf(_vocab.CompoundParts.All()).Must().SequenceEqual(["忘れる", "られない", "ない", "忘れられる"]);

   public class after_the_part_entered_as_both_stops_being_a_primary_part : When_a_vocab_has_alternative_compound_parts
   {
      public after_the_part_entered_as_both_stops_being_a_primary_part() => _vocab.Edit(it => it.CompoundParts.SetPrimary(Words("忘れる")));

      [XF] public void it_is_an_alternative_part_again_so_typing_the_primary_parts_never_drops_an_alternative_part_on_the_way() =>
         NamesOf(_vocab.CompoundParts.Alternative()).Must().Contain("られない");
   }

   public class after_saving_it_to_the_corpus_and_reading_it_back : When_a_vocab_has_alternative_compound_parts
   {
      readonly string _json;
      readonly ITaggregateReference<VocabNote> _readBackReference;
      VocabNote _readBack => _readBackReference.Current;

      public after_saving_it_to_the_corpus_and_reading_it_back()
      {
         var serializer = GetService<NoteSerializer>();
         _json = serializer.Serialize(_vocab);
         _readBackReference = serializer.DeserializeVocab(_json).Reference();
      }

      [XF] public void the_primary_parts_are_read_back_in_their_order() => NamesOf(_readBack.CompoundParts.Primary()).Must().SequenceEqual(["忘れる", "られない"]);
      [XF] public void the_alternative_parts_are_read_back() => NamesOf(_readBack.CompoundParts.Alternative()).Must().SequenceEqual(["ない", "忘れられる"]);

      [XF] public void the_corpus_json_stores_the_alternative_parts_as_entered_the_one_that_is_also_a_primary_part_included() =>
         CompoundPartsInTheJson("alternative").Must().SequenceEqual(["ない", "られない", "忘れられる"]);

      [XF] public void the_corpus_json_stores_the_primary_parts_in_their_order() =>
         CompoundPartsInTheJson("primary").Must().SequenceEqual(["忘れる", "られない"]);

      string[] CompoundPartsInTheJson(string kind) =>
      [
         .. JsonDocument.Parse(_json).RootElement.GetProperty("compoundParts").GetProperty(kind).EnumerateArray()
                        .Select(it => it.GetProperty("text").GetString() ?? "")
      ];
   }
}
