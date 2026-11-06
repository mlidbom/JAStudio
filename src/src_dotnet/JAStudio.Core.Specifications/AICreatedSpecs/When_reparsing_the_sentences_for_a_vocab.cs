using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Batches;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

public class When_reparsing_the_sentences_for_a_vocab : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabNote _vocab;

   protected When_reparsing_the_sentences_for_a_vocab()
   {
      _vocab = CreateVocab("走る", "to-run", "はしる");
      CreateSentence("毎日走る。");
   }

   LocalNoteUpdater LocalNoteUpdater => GetService<LocalNoteUpdater>();

   public class whose_only_sentence_matches_it : When_reparsing_the_sentences_for_a_vocab
   {
      public whose_only_sentence_matches_it() => LocalNoteUpdater.ReparseSentencesForVocab(_vocab);

      [XF] public void the_vocab_matches_the_sentence() => _vocab.Sentences.All().Must().HaveCount(1);
      [XF] public void the_stored_sentence_count_is_1() => _vocab.MetaData.SentenceCount.Must().Be(1);

      public class after_requiring_the_vocab_to_start_the_sentence_which_it_does_not : whose_only_sentence_matches_it
      {
         public after_requiring_the_vocab_to_start_the_sentence_which_it_does_not()
         {
            _vocab.MatchingConfiguration.RequiresForbids.SentenceStart.SetRequired(true);
            LocalNoteUpdater.ReparseSentencesForVocab(_vocab);
         }

         [XF] public void the_vocab_no_longer_matches_the_sentence() => _vocab.Sentences.All().Must().BeEmpty();
         [XF] public void the_stored_sentence_count_is_0() => _vocab.MetaData.SentenceCount.Must().Be(0);
      }
   }
}
