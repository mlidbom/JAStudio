using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// Filling a vocab's parts of speech from the dictionary (<see cref="VocabNotePartsOfSpeech.SetAutomaticallyFromDictionary"/>): the dictionary's parts of speech,<br/>
/// archaic ones included, become those JAStudio knows (<see cref="PartOfSpeech"/>).
/// </summary>
public abstract class When_filling_a_vocabs_parts_of_speech_from_the_dictionary : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabNote _vocab;

   protected When_filling_a_vocabs_parts_of_speech_from_the_dictionary(string question, string reading)
   {
      _vocab = CreateVocab(question, "", reading);
      _vocab.PartsOfSpeech.SetAutomaticallyFromDictionary();
   }

   /// <summary>The names of the vocab's parts of speech, in alphabetical order.</summary>
   protected List<string> Names => [.. _vocab.PartsOfSpeech.All().Select(it => it.Name)];

   public class for_a_transitive_godan_verb() : When_filling_a_vocabs_parts_of_speech_from_the_dictionary("書く", "かく")
   {
      [XF] public void it_is_a_transitive_godan_verb() => Names.Must().SequenceEqual(["godan verb", "transitive"]);
   }

   public class for_an_archaic_ku_adjective() : When_filling_a_vocabs_parts_of_speech_from_the_dictionary("難し", "かたし")
   {
      [XF] public void it_is_a_ku_adjective() => Names.Must().SequenceEqual(["ku-adjective"]);
   }

   public class for_an_archaic_nidan_verb() : When_filling_a_vocabs_parts_of_speech_from_the_dictionary("求む", "もとむ")
   {
      [XF] public void it_is_a_transitive_nidan_verb() => Names.Must().SequenceEqual(["nidan verb", "transitive"]);
   }

   public class for_the_archaic_form_of_a_na_adjective() : When_filling_a_vocabs_parts_of_speech_from_the_dictionary("寛", "ゆた")
   {
      [XF] public void it_is_a_nari_adjective() => Names.Must().SequenceEqual(["nari-adjective"]);
   }

   public class for_an_archaic_ri_verb() : When_filling_a_vocabs_parts_of_speech_from_the_dictionary("然り", "しかり")
   {
      [XF] public void it_is_an_intransitive_ri_verb_and_an_interjection() => Names.Must().SequenceEqual(["interjection", "intransitive", "ri verb"]);
   }
}
