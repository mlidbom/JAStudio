using System.Linq;
using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Core.Storage;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.ParsingCases;

public class When_a_sentence_is_marked_as_a_parsing_case : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly SentenceNote _sentence;

   protected When_a_sentence_is_marked_as_a_parsing_case()
   {
      CreateVocab("走る", "to-run", "はしる");
      CreateVocab("ている", "is-doing", "ている");
      _sentence = CreateSentence("毎日走っている");
      _sentence.MarkAsAParsingCase();
   }

   ParsingCase TheCase => _sentence.ParsingCase!;
   ParsingCaseComparison Comparison => _sentence.CompareWithWhatTheParsingCaseExpects();

   public class at_first : When_a_sentence_is_marked_as_a_parsing_case
   {
      [XF] public void it_is_a_parsing_case() => _sentence.ParsingCase.Must().NotBeNull();
      [XF] public void nobody_has_named_it() => TheCase.Name.Must().Be("");
      [XF] public void nobody_has_said_why() => TheCase.Why.Must().Be("");
      [XF] public void its_displayed_vocab_are_unverified() => TheCase.VerdictOnTheDisplayedVocab.Must().Be(Verdict.Unverified);
      [XF] public void it_expects_what_the_sentence_parses_to() => Comparison.Differences.Must().BeEmpty();
      [XF] public void it_expects_the_words_the_sentence_parses_to() => TheCase.Expected.ParsedWords.Must().NotBeEmpty();
      [XF] public void it_expects_a_breakdown_displaying_the_two_vocab() => TheCase.Expected.DisplayedVocab.Words.Count(it => it.VocabId != null).Must().Be(2);
      [XF] public void it_holds_each_vocab_by_its_id_alone() => TheCase.Expected.ParsedWords.Must().Satisfy(words => words.All(it => it.Vocab == null));
      [XF] public void marking_it_again_fails() => Invoking(() => _sentence.MarkAsAParsingCase()).Must().Throw<StateAssertionFailedException>();
   }

   public class and_named_and_given_its_why : When_a_sentence_is_marked_as_a_parsing_case
   {
      public and_named_and_given_its_why()
      {
         _sentence.NameTheParsingCase("the progressive");
         _sentence.SayWhyItIsAParsingCase("ている was matched as て and いる");
      }

      [XF] public void it_has_the_name() => TheCase.Name.Must().Be("the progressive");
      [XF] public void it_has_the_why() => TheCase.Why.Must().Be("ている was matched as て and いる");
      [XF] public void it_still_expects_what_the_sentence_parses_to() => Comparison.Differences.Must().BeEmpty();
   }

   public class and_written_as_the_corpus_stores_it_and_read_back : When_a_sentence_is_marked_as_a_parsing_case
   {
      readonly SentenceNote _readBack;

      public and_written_as_the_corpus_stores_it_and_read_back()
      {
         _sentence.NameTheParsingCase("the progressive");
         _sentence.SetTheVerdictOnTheDisplayedVocab(Verdict.Broken);
         var serializer = GetService<NoteSerializer>();
         _readBack = serializer.DeserializeSentence(serializer.Serialize(_sentence));
      }

      [XF] public void it_has_its_name() => _readBack.ParsingCase!.Name.Must().Be("the progressive");
      [XF] public void it_has_its_verdict() => _readBack.ParsingCase!.VerdictOnTheDisplayedVocab.Must().Be(Verdict.Broken);
      [XF] public void it_expects_the_same_result() => TheCase.Expected.HowItDiffersFromThisExpectedResult(_readBack.ParsingCase!.Expected, GetService<JPCollection>()).Must().BeEmpty();
      [XF] public void it_parses_as_expected() => _readBack.CompareWithWhatTheParsingCaseExpects().Differences.Must().BeEmpty();
   }

   public class a_sentence_that_is_not_one_written_as_the_corpus_stores_it : When_a_sentence_is_marked_as_a_parsing_case
   {
      [XF] public void has_nothing_about_a_parsing_case() => GetService<NoteSerializer>().Serialize(CreateSentence("毎日走る")).Must().NotContain("parsingCase");
   }

   public class after_a_vocab_is_added_that_the_breakdown_displays : When_a_sentence_is_marked_as_a_parsing_case
   {
      public after_a_vocab_is_added_that_the_breakdown_displays() => CreateVocab("毎日", "every-day", "まいにち");

      [XF] public void it_no_longer_parses_as_expected() => Comparison.Differences.Must().NotBeEmpty();
      [XF] public void the_displayed_vocab_differ() => Comparison.TheDisplayedVocabDiffer.Must().BeTrue();
      [XF] public void it_does_not_fail_since_nobody_verified_the_displayed_vocab() => Comparison.Fails.Must().BeFalse();
      [XF] public void it_is_to_review() => Comparison.IsToReview.Must().BeTrue();

      public class and_what_it_parses_to_is_accepted : after_a_vocab_is_added_that_the_breakdown_displays
      {
         public and_what_it_parses_to_is_accepted() => _sentence.AcceptWhatItParsesToAsAParsingCase();

         [XF] public void it_parses_as_expected_again() => Comparison.Differences.Must().BeEmpty();
         [XF] public void its_displayed_vocab_are_still_unverified() => TheCase.VerdictOnTheDisplayedVocab.Must().Be(Verdict.Unverified);
      }
   }

   public class whose_displayed_vocab_a_person_has_verified : When_a_sentence_is_marked_as_a_parsing_case
   {
      public whose_displayed_vocab_a_person_has_verified() => _sentence.SetTheVerdictOnTheDisplayedVocab(Verdict.Verified);

      [XF] public void they_are_verified() => TheCase.VerdictOnTheDisplayedVocab.Must().Be(Verdict.Verified);
      [XF] public void it_does_not_fail() => Comparison.Fails.Must().BeFalse();

      public class and_then_a_vocab_is_added_that_the_breakdown_displays : whose_displayed_vocab_a_person_has_verified
      {
         public and_then_a_vocab_is_added_that_the_breakdown_displays() => CreateVocab("毎日", "every-day", "まいにち");

         [XF] public void it_fails() => Comparison.Fails.Must().BeTrue();
         [XF] public void it_is_not_merely_to_review() => Comparison.IsToReview.Must().BeFalse();

         public class and_what_it_then_parses_to_is_accepted : and_then_a_vocab_is_added_that_the_breakdown_displays
         {
            public and_what_it_then_parses_to_is_accepted() => _sentence.AcceptWhatItParsesToAsAParsingCase();

            [XF] public void it_no_longer_fails() => Comparison.Fails.Must().BeFalse();
            [XF] public void its_displayed_vocab_are_still_verified() => TheCase.VerdictOnTheDisplayedVocab.Must().Be(Verdict.Verified);
         }
      }

      public class and_then_a_vocab_is_added_that_matches_but_is_not_displayed : whose_displayed_vocab_a_person_has_verified
      {
         public and_then_a_vocab_is_added_that_matches_but_is_not_displayed() => CreateVocab("いる", "to-be", "いる");

         [XF] public void it_parses_otherwise_than_expected() => Comparison.Differences.Must().NotBeEmpty();
         [XF] public void the_displayed_vocab_are_the_same() => Comparison.TheDisplayedVocabDiffer.Must().BeFalse();
         [XF] public void it_still_does_not_fail() => Comparison.Fails.Must().BeFalse();
         [XF] public void it_is_to_review() => Comparison.IsToReview.Must().BeTrue();
      }
   }

   public class and_then_stops_being_one : When_a_sentence_is_marked_as_a_parsing_case
   {
      public and_then_stops_being_one() => _sentence.StopBeingAParsingCase();

      [XF] public void it_is_no_parsing_case() => _sentence.ParsingCase.Must().BeNull();
      [XF] public void comparing_it_fails() => Invoking(() => _sentence.CompareWithWhatTheParsingCaseExpects()).Must().Throw<StateAssertionFailedException>();
      [XF] public void accepting_what_it_parses_to_fails() => Invoking(() => _sentence.AcceptWhatItParsesToAsAParsingCase()).Must().Throw<StateAssertionFailedException>();
   }
}
