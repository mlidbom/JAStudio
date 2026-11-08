using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Storage;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>A sentence whose source name is set, written as the corpus's JSON and read back, as JAStudio saves and loads the corpus.</summary>
public class When_a_sentence_with_a_source_name_is_saved_to_the_corpus : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly SentenceNote _readBack;

   public When_a_sentence_with_a_source_name_is_saved_to_the_corpus()
   {
      var sentence = CreateTestSentence("ありがとう", "Thank you");
      sentence.SourceName.Set("Shirokuma Cafe");
      var serializer = GetService<NoteSerializer>();
      _readBack = serializer.DeserializeSentence(serializer.Serialize(sentence));
   }

   [XF] public void it_is_read_back_with_its_source_name() => _readBack.SourceName.Value.Must().Be("Shirokuma Cafe");
}
