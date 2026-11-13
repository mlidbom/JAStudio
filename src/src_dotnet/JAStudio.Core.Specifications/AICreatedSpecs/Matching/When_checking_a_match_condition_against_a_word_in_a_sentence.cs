using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.Parsing;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Checking;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// The vocab ます, whose match-condition is <c>AllOf(PrecedingToken.AnyOf(IsAGodanVerb, IsAnIchidanVerb), !PrecedingToken.IsAGodanVerb)</c>, checked<br/>
/// against the word ます in a sentence (<see cref="CheckedCondition"/>): each of its conditions, as each line of the tree editor shows it, met or not by<br/>
/// the subject it's on, the conditions on the preceding token by the token before ます.
/// </summary>
public class When_checking_a_match_condition_against_a_word_in_a_sentence : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   static readonly ConditionPosition PrecedingTokenIsAGodanOrIchidanVerb = ConditionPosition.Root.Child(0);
   static readonly ConditionPosition IsAGodanOrIchidanVerb = PrecedingTokenIsAGodanOrIchidanVerb.Child(0);
   static readonly ConditionPosition PrecedingTokenIsNotAGodanVerb = ConditionPosition.Root.Child(1);
   static readonly ConditionPosition PrecedingTokenIsAGodanVerb = PrecedingTokenIsNotAGodanVerb.Child(0);

   readonly VocabNote _ます;

   public When_checking_a_match_condition_against_a_word_in_a_sentence()
   {
      _ます = CreateVocab("ます", "polite ending", "ます");
      _ます.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(PrecedingToken.Meets(ConditionGroup.AnyOf(TokenConditions.IsAGodanVerb, TokenConditions.IsAnIchidanVerb)),
                                                                        !PrecedingToken.Meets(TokenConditions.IsAGodanVerb)));
   }

   /// <summary>The match-condition checked against the word ます in <paramref name="sentence"/>, its only match there.</summary>
   protected CheckedCondition TheCheckIn(string sentence) =>
      _ます.MatchingConfiguration.MatchCondition.CheckedAgainst(TextAnalysis.FromText(GetService<AnalysisServices>(), sentence).MatchesOf(_ます).Single().Variant);

   public class in_食べます_after_the_ichidan_verb_食べる : When_checking_a_match_condition_against_a_word_in_a_sentence
   {
      readonly CheckedCondition _check;

      public in_食べます_after_the_ichidan_verb_食べる() => _check = TheCheckIn("食べます");

      [XF] public void the_match_condition_is_met() => _check.IsMet.Must().BeTrue();
      [XF] public void the_preceding_token_being_a_godan_or_ichidan_verb_is_met() => _check.At(PrecedingTokenIsAGodanOrIchidanVerb).IsMet.Must().BeTrue();

      [XF] public void of_the_two_only_being_an_ichidan_verb_is_met_by_the_preceding_token() =>
         _check.At(IsAGodanOrIchidanVerb).Children.Select(it => it.IsMet).Must().SequenceEqual([false, true]);

      [XF] public void the_preceding_token_not_being_a_godan_verb_is_met() => _check.At(PrecedingTokenIsNotAGodanVerb).IsMet.Must().BeTrue();
      [XF] public void what_that_negates_the_preceding_token_being_a_godan_verb_is_not_met() => _check.At(PrecedingTokenIsAGodanVerb).IsMet.Must().BeFalse();
   }

   public class in_読みます_after_the_godan_verb_読む : When_checking_a_match_condition_against_a_word_in_a_sentence
   {
      readonly CheckedCondition _check;

      public in_読みます_after_the_godan_verb_読む() => _check = TheCheckIn("読みます");

      [XF] public void the_match_condition_is_not_met() => _check.IsMet.Must().BeFalse();
      [XF] public void the_preceding_token_being_a_godan_or_ichidan_verb_is_met() => _check.At(PrecedingTokenIsAGodanOrIchidanVerb).IsMet.Must().BeTrue();
      [XF] public void the_preceding_token_not_being_a_godan_verb_is_what_is_not_met() => _check.At(PrecedingTokenIsNotAGodanVerb).IsMet.Must().BeFalse();
   }
}
