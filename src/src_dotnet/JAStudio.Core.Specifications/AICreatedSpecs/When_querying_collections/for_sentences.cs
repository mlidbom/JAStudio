using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_querying_collections;

public class for_sentences : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_a_sentence : for_sentences
   {
      readonly SentenceNote _sentence;

      public given_a_sentence() => _sentence = CreateTestSentence("これは本です。", "This is a book.");

      [XF] public void All_contains_the_sentence() =>
         GetService<SentenceCollection>().All().Must().Contain(_sentence);
   }
}
