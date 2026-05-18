using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_querying_collections;

public class for_vocabs : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_three_vocabs : for_vocabs
   {
      readonly VocabNote _vocab1;
      readonly VocabNote _vocab2;

      public given_three_vocabs()
      {
         _vocab1 = CreateVocab("食べる", "to eat", "たべる");
         _vocab2 = CreateVocab("本", "book", "ほん");
         CreateVocab("走る", "to run", "はしる");
      }

      [XF] public void WithQuestion_returns_a_single_result() =>
         GetService<VocabCollection>().WithQuestion("本").Count.Must().Be(1);

      [XF] public void WithQuestion_finds_the_matching_vocab() =>
         GetService<VocabCollection>().WithQuestion("本")[0].Must().Be(_vocab2);

      [XF] public void WithQuestion_returns_empty_for_a_nonexistent_question() =>
         GetService<VocabCollection>().WithQuestion("存在しない").Must().BeEmpty();

      [XF] public void WithIdOrNone_returns_the_correct_note() =>
         GetService<VocabCollection>().WithIdOrNone(_vocab1.GetId()).Must().Be(_vocab1);
   }

   public class given_a_vocab_with_a_form_added : for_vocabs
   {
      public given_a_vocab_with_a_form_added()
      {
         var vocab = CreateVocab("食べる", "to eat", "たべる");
         vocab.Forms.Add("食う");
      }

      [XF] public void WithQuestion_does_not_find_by_form_value() =>
         GetService<VocabCollection>().WithQuestion("食う").Must().BeEmpty();
   }
}
