using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_querying_collections;

public class for_sentences : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_a_sentence : for_sentences
   {
      readonly ITaggregateReference<SentenceNote> _sentenceReference;
      SentenceNote _sentence => _sentenceReference.Current;

      public given_a_sentence() => _sentenceReference = CreateTestSentence("これは本です。", "This is a book.").Reference();

      [XF] public void All_contains_the_sentence() =>
         GetService<SentenceCollection>().All().Must().Contain(_sentence);
   }
}
