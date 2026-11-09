using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// Creating a vocab (<see cref="VocabNoteFactory"/>) that has no parts of speech gives it the dictionary's, and decides it as usually written in kana<br/>
/// where the dictionary says so. Only when it's created: parts of speech removed later stay removed.
/// </summary>
public class When_JAStudio_creates_a_vocab : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected VocabNote Create(string question, string answer, string reading) => GetService<VocabNoteFactory>().Create(question, answer, [reading]);

   static List<string> NamesOfThePartsOfSpeechOf(VocabNote vocab) => [.. vocab.PartsOfSpeech.All().Select(it => it.Name)];

   public class without_parts_of_speech_for_a_verb_the_dictionary_has : When_JAStudio_creates_a_vocab
   {
      readonly VocabNote _vocab;
      public without_parts_of_speech_for_a_verb_the_dictionary_has() => _vocab = Create("書く", "to write", "かく");

      [XF] public void it_gets_the_dictionarys_parts_of_speech() => NamesOfThePartsOfSpeechOf(_vocab).Must().SequenceEqual(["godan verb", "transitive"]);
      [XF] public void it_stays_undecided_whether_it_is_usually_written_in_kana() => _vocab.UsuallyWrittenInKana.Decision.Must().Be(UsuallyWrittenInKanaDecision.Undecided);

      public class after_its_parts_of_speech_are_removed_and_its_data_is_generated_again : without_parts_of_speech_for_a_verb_the_dictionary_has
      {
         public after_its_parts_of_speech_are_removed_and_its_data_is_generated_again()
         {
            _vocab.PartsOfSpeech.Set([]);
            _vocab.UpdateGeneratedData();
         }

         [XF] public void it_has_no_parts_of_speech() => _vocab.PartsOfSpeech.All().Must().BeEmpty();
      }
   }

   public class without_parts_of_speech_for_a_word_the_dictionary_says_is_usually_written_in_kana : When_JAStudio_creates_a_vocab
   {
      readonly VocabNote _vocab;
      public without_parts_of_speech_for_a_word_the_dictionary_says_is_usually_written_in_kana() => _vocab = Create("為る", "to do", "する");

      [XF] public void it_is_decided_as_usually_written_in_kana() => _vocab.UsuallyWrittenInKana.Decision.Must().Be(UsuallyWrittenInKanaDecision.Yes);
      [XF] public void it_gets_the_dictionarys_parts_of_speech() => _vocab.PartsOfSpeech.All().Must().NotBeEmpty();
   }

   public class with_parts_of_speech_given : When_JAStudio_creates_a_vocab
   {
      readonly VocabNote _vocab;

      public with_parts_of_speech_given() =>
         _vocab = GetService<VocabNoteFactory>().Create("書く", "to write", ["かく"], it => it.PartsOfSpeech.Set([PartOfSpeech.Noun]));

      [XF] public void it_keeps_them() => NamesOfThePartsOfSpeechOf(_vocab).Must().SequenceEqual(["noun"]);
   }
}
