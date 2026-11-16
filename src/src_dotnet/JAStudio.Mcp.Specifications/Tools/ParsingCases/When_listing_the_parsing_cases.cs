using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Mcp.Tools.ParsingCases;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.ParsingCases;

/// <summary>
/// The vocab 走る and ている, the sentence 毎日走っている, marked as a parsing case named "the progressive" with the why "ている was matched as て and いる",<br/>
/// and the sentence 毎日走る, which is no parsing case.
/// </summary>
public class When_listing_the_parsing_cases : McpServerSpecification
{
   protected readonly SentenceNote Sentence;

   public When_listing_the_parsing_cases()
   {
      CreateVocab("走る", "to-run", "はしる");
      CreateVocab("ている", "is-doing", "ている");
      CreateSentence("毎日走る");
      Sentence = CreateSentence("毎日走っている");
      Sentence.Edit(it =>
      {
         it.MarkAsAParsingCase();
         it.NameTheParsingCase("the progressive");
         it.SayWhyItIsAParsingCase("ている was matched as て and いる");
      });
   }

   protected ParsingCasesListed List(bool includingThoseThatParseAsExpected = false, int maxDifferencesListedForEachCase = ParsingCaseTools.DifferencesListedForEachCaseByDefault) =>
      ResultOf<ParsingCasesListed>(CallTool("list_the_parsing_cases",
                                            new Dictionary<string, object?>
                                            {
                                               ["includingThoseThatParseAsExpected"] = includingThoseThatParseAsExpected,
                                               ["maxDifferencesListedForEachCase"] = maxDifferencesListedForEachCase
                                            }));

   public class while_the_one_case_parses_as_expected : When_listing_the_parsing_cases
   {
      [XF] public void one_parsing_case_is_counted() => List().ParsingCases.Must().Be(1);
      [XF] public void it_is_counted_as_parsing_as_expected() => List().ParsingAsExpected.Must().Be(1);
      [XF] public void no_case_is_listed() => List().Cases.Must().BeEmpty();

      public class including_those_that_parse_as_expected : while_the_one_case_parses_as_expected
      {
         readonly ParsingCaseListed _case;
         public including_those_that_parse_as_expected() => _case = List(includingThoseThatParseAsExpected: true).Cases.Single();

         [XF] public void the_case_is_listed_with_the_id_of_its_sentence() => _case.SentenceId.Must().Be(Sentence.GetId().ToString());
         [XF] public void the_case_is_listed_with_its_sentence() => _case.Sentence.Must().Be("毎日走っている");
         [XF] public void the_case_is_listed_with_its_name() => _case.Name.Must().Be("the progressive");
         [XF] public void the_case_is_listed_with_its_why() => _case.Why.Must().Be("ている was matched as て and いる");
         [XF] public void the_case_is_listed_as_unverified() => _case.VerdictOnTheDisplayedVocab.Must().Be(Verdict.Unverified);
         [XF] public void the_case_is_listed_as_parsing_as_expected() => _case.Outcome.Must().Be(ParsingCaseOutcome.ParsesAsExpected);
         [XF] public void the_case_is_listed_without_differences() => _case.Differences.Must().BeEmpty();
      }
   }

   public class after_a_vocab_is_added_that_the_breakdown_displays : When_listing_the_parsing_cases
   {
      public after_a_vocab_is_added_that_the_breakdown_displays() => CreateVocab("毎日", "every-day", "まいにち");

      [XF] public void the_case_is_counted_as_to_review() => List().ToReview.Must().Be(1);
      [XF] public void the_case_is_listed_as_to_review() => List().Cases.Single().Outcome.Must().Be(ParsingCaseOutcome.IsToReview);
      [XF] public void the_case_is_listed_with_a_difference_naming_the_vocab_added() => List().Cases.Single().Differences.Must().Satisfy(differences => differences.Any(it => it.Contains("毎日")));
      [XF] public void every_difference_is_counted() => List().Cases.Single().Must().Satisfy(it => it.DifferencesCounted == it.Differences.Count && it.DifferencesCounted > 1);
      [XF] public void asked_for_one_difference_for_each_case_one_is_listed() => List(maxDifferencesListedForEachCase: 1).Cases.Single().Differences.Must().HaveCount(1);
      [XF] public void asked_for_one_difference_for_each_case_every_difference_is_still_counted() => List(maxDifferencesListedForEachCase: 1).Cases.Single().DifferencesCounted.Must().BeGreaterThan(1);
   }

   public class after_its_displayed_vocab_are_verified_and_then_a_vocab_is_added_that_the_breakdown_displays : When_listing_the_parsing_cases
   {
      public after_its_displayed_vocab_are_verified_and_then_a_vocab_is_added_that_the_breakdown_displays()
      {
         Sentence.Edit(it => it.SetTheVerdictOnTheDisplayedVocab(Verdict.Verified));
         CreateVocab("毎日", "every-day", "まいにち");
      }

      [XF] public void the_case_is_counted_as_failing() => List().Failing.Must().Be(1);
      [XF] public void the_case_is_listed_as_failing() => List().Cases.Single().Outcome.Must().Be(ParsingCaseOutcome.Fails);
      [XF] public void the_case_is_listed_as_verified() => List().Cases.Single().VerdictOnTheDisplayedVocab.Must().Be(Verdict.Verified);
   }
}
