using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JamdictEx;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// Looking a text up in the dictionary as a word, or as a name if no word has it (<see cref="DictLookup.LookupWordOrName"/>): JMdict's words first, then<br/>
/// JMnedict's names. A vocab without readings is looked up so (<see cref="DictLookup.LookupVocabWordOrName"/>).
/// </summary>
public abstract class When_looking_up_a_word_or_name_in_the_dictionary : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly DictLookupResult Result;

   protected When_looking_up_a_word_or_name_in_the_dictionary(string text) => Result = GetService<DictLookup>().LookupWordOrName(text);

   /// <summary>田代島, an island: a name of JMnedict, and not a form of any word of JMdict.</summary>
   public class that_is_a_name_and_no_word() : When_looking_up_a_word_or_name_in_the_dictionary("田代島")
   {
      [XF] public void the_name_is_found() => Result.FoundWordsCount().Must().Be(1);
   }
}
