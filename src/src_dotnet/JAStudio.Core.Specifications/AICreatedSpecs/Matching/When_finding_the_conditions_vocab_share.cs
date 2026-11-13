using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing.Candidates;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping.ConditionGroup;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// Six vocab: ない requires an irrealis form, a single token and no ichidan verb; せる an irrealis form and no ichidan verb; させる no ichidan verb, an<br/>
/// irrealis form and a masu stem. んだ requires a preceding token whose surface ends with ん; んです that and a single token; んでしょう that and a masu stem.<br/>
/// だの, である and でない forbid it. んだろう holds it twice: <c>AnyOf(AllOf(it, IsASingleToken), AllOf(it, a masu stem))</c>.
/// </summary>
public class When_finding_the_conditions_vocab_share : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   static readonly Condition<CandidateWordVariant> FollowsAnIrrealisForm = MatchingRules.Irrealis.WhenRequired;
   static readonly Condition<CandidateWordVariant> FollowsNoIchidanVerb = MatchingRules.Ichidan.WhenForbidden;
   static readonly Condition<CandidateWordVariant> FollowsASurfaceEndingWithん = PrecedingToken.Meets(TokenConditions.Surface.Meets(TextConditions.EndsWithOneOf("ん")));

   readonly IReadOnlyList<SharedConditionCandidate> _found;

   public When_finding_the_conditions_vocab_share()
   {
      VocabWith("ない", FollowsAnIrrealisForm, IsASingleToken, FollowsNoIchidanVerb);
      VocabWith("せる", FollowsAnIrrealisForm, FollowsNoIchidanVerb);
      VocabWith("させる", FollowsNoIchidanVerb, FollowsAnIrrealisForm, MatchingRules.MasuStem.WhenRequired);
      VocabWith("んだ", FollowsASurfaceEndingWithん);
      VocabWith("んです", FollowsASurfaceEndingWithん, IsASingleToken);
      VocabWith("んでしょう", FollowsASurfaceEndingWithん, MatchingRules.MasuStem.WhenRequired);
      VocabWith("だの", !FollowsASurfaceEndingWithん);
      VocabWith("である", !FollowsASurfaceEndingWithん);
      VocabWith("でない", !FollowsASurfaceEndingWithん);
      VocabWith("んだろう", AnyOf(AllOf(FollowsASurfaceEndingWithん, IsASingleToken), AllOf(FollowsASurfaceEndingWithん, MatchingRules.MasuStem.WhenRequired)));

      _found = SharedConditionCandidates.FoundIn(GetService<JPCollection>().Vocab.All(), heldByAtLeast: 3);
   }

   void VocabWith(string question, params Condition<CandidateWordVariant>[] conditions) =>
      CreateVocab(question, question, question).MatchingConfiguration.SetMatchCondition(AllOf(conditions));

   IReadOnlyList<string> Codes => [.. _found.Select(it => it.Code)];

   static IReadOnlyList<string> QuestionsOf(SharedConditionCandidate candidate) => [.. candidate.Vocab.Select(it => it.GetQuestion()).Order(StringComparer.Ordinal)];

   [XF] public void two_conditions_are_found_neither_a_name_alone_nor_a_negation_nor_a_part_of_the_first() => Codes.Must().HaveCount(2);

   [XF] public void the_first_held_by_the_most_vocab_is_the_preceding_tokens_surface_ending_with_ん() => _found[0].Code.Must().Be("""PrecedingToken.Surface.EndsWithOneOf("ん")""");

   [XF] public void the_first_is_held_by_the_three_requiring_it_the_three_forbidding_it_and_once_by_the_one_holding_it_twice() =>
      QuestionsOf(_found[0]).Must().SequenceEqual(["だの", "である", "でない", "んだ", "んだろう", "んでしょう", "んです"]);

   [XF] public void the_first_is_no_combination() => _found[0].IsACombination.Must().BeFalse();

   [XF] public void the_second_means_an_irrealis_form_and_no_ichidan_verb() =>
      Codes.Must().Satisfy(_ => _found[1].Condition.MeansTheSameAs(AllOf(FollowsAnIrrealisForm, FollowsNoIchidanVerb)));

   [XF] public void the_second_is_held_by_ない_せる_and_させる_whatever_the_order_of_the_two_conditions() => QuestionsOf(_found[1]).Must().SequenceEqual(["させる", "せる", "ない"]);

   [XF] public void the_second_is_on_a_word() => _found[1].Level.SubjectNameInCode.Must().Be("Word");
}
