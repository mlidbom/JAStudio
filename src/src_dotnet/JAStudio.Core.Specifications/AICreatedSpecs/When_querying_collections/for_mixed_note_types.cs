using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_querying_collections;

public class for_mixed_note_types : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_one_of_each_note_type : for_mixed_note_types
   {
      public given_one_of_each_note_type()
      {
         CreateKanji("食", "eat", "ショク", "た");
         CreateVocab("食べる", "to eat", "たべる");
         CreateTestSentence("食べる", "to eat");
      }

      [XF] public void the_kanji_collection_has_one_note() =>
         GetService<KanjiCollection>().All().Count.Must().Be(1);

      [XF] public void the_vocab_collection_has_one_note() =>
         GetService<VocabCollection>().All().Count.Must().Be(1);

      [XF] public void the_sentence_collection_has_one_note() =>
         GetService<SentenceCollection>().All().Count.Must().Be(1);
   }
}
