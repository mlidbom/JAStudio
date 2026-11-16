using System.Linq;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_querying_collections;

public class for_vocabs : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_three_vocabs : for_vocabs
   {
      readonly ITaggregateReference<VocabNote> _vocab1Reference;
      VocabNote _vocab1 => _vocab1Reference.Current;
      readonly ITaggregateReference<VocabNote> _vocab2Reference;
      VocabNote _vocab2 => _vocab2Reference.Current;

      public given_three_vocabs()
      {
         _vocab1Reference = CreateVocab("食べる", "to eat", "たべる").Reference();
         _vocab2Reference = CreateVocab("本", "book", "ほん").Reference();
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
         vocab.Edit(it => it.Forms.Add("食う"));
      }

      [XF] public void WithQuestion_does_not_find_by_form_value() =>
         GetService<VocabCollection>().WithQuestion("食う").Must().BeEmpty();
   }
}
