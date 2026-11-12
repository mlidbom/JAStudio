using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;
using JAStudio.Core.Storage;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping.ConditionGroup;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// The corpus has four shared-conditions:
/// <list type="bullet">
/// <item><c>Word.StartsInAGodanPotentialForm</c>, the godan potential rule's condition, <c>FirstToken.AnyOf(IsAGodanPotentialStem, IsAGodanPotentialInflection)</c>;</item>
/// <item><c>Word.FollowsAnIrrealisFormOfNoIchidanVerb</c>, a combination of two rules' conditions, <c>AllOf(PrecedingToken.IsInAnIrrealisForm, !PrecedingToken.IsAnIchidanVerb)</c>;</item>
/// <item><c>Token.IsAVerb</c>, <c>AnyOf(IsAGodanVerb, IsAnIchidanVerb)</c>;</item>
/// <item><c>Word.FollowsAVerb</c>, with <c>Token.IsAVerb</c>'s definition written out in it, <c>PrecedingToken.AnyOf(IsAGodanVerb, IsAnIchidanVerb)</c>.</item>
/// </list>
/// And vocab whose match-conditions hold their definitions written out, some in another order.
/// </summary>
public abstract class When_using_a_shared_condition_wherever_its_definition_is : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly SharedConditions _sharedConditions;
   protected readonly SharedCondition<CandidateWordVariant> _startsInAGodanPotentialForm;
   protected readonly SharedCondition<CandidateWordVariant> _followsAnIrrealisFormOfNoIchidanVerb;
   protected readonly SharedCondition<IAnalysisToken> _isAVerb;
   protected readonly SharedCondition<CandidateWordVariant> _followsAVerb;

   /// <summary>Godan potential required, its two facts the other way round: <c>FirstToken.AnyOf(IsAGodanPotentialInflection, IsAGodanPotentialStem)</c>.</summary>
   protected readonly VocabNote _godanPotentialInTheOtherOrder;
   /// <summary>Godan potential forbidden: <c>!FirstToken.AnyOf(IsAGodanPotentialStem, IsAGodanPotentialInflection)</c>.</summary>
   protected readonly VocabNote _godanPotentialForbidden;
   /// <summary>Irrealis required, single token required and ichidan forbidden, in that order.</summary>
   protected readonly VocabNote _irrealisSingleTokenAndNoIchidan;
   /// <summary>Irrealis required and ichidan forbidden, and nothing else: its whole match-condition is a shared-condition's definition.</summary>
   protected readonly VocabNote _irrealisAndNoIchidan;
   /// <summary><c>FollowingToken.AnyOf(IsAnIchidanVerb, IsAGodanVerb)</c>: <c>Token.IsAVerb</c>'s definition in the other order, under a token.</summary>
   protected readonly VocabNote _followedByAVerb;
   /// <summary>Masu stem required, which no shared-condition is.</summary>
   protected readonly VocabNote _masuStem;

   readonly Dictionary<VocabNote, AllOfGroup<CandidateWordVariant>> _matchConditionsBefore;
   readonly Dictionary<VocabNote, int> _weightsBefore;

   protected When_using_a_shared_condition_wherever_its_definition_is()
   {
      _sharedConditions = GetService<SharedConditions>();
      _startsInAGodanPotentialForm = _sharedConditions.Define(Level, "StartsInAGodanPotentialForm", MatchingRules.GodanPotential.WhenRequired);
      _followsAnIrrealisFormOfNoIchidanVerb = _sharedConditions.Define(Level, "FollowsAnIrrealisFormOfNoIchidanVerb", AllOf(MatchingRules.Irrealis.WhenRequired, MatchingRules.Ichidan.WhenForbidden));
      _isAVerb = _sharedConditions.Define(TokenConditions.Level, "IsAVerb", AnyOf(TokenConditions.IsAGodanVerb, TokenConditions.IsAnIchidanVerb));
      _followsAVerb = _sharedConditions.Define(Level, "FollowsAVerb", PrecedingToken.AnyOf(TokenConditions.IsAGodanVerb, TokenConditions.IsAnIchidanVerb));

      _godanPotentialInTheOtherOrder = VocabWith("える", FirstToken.AnyOf(TokenConditions.IsAGodanPotentialInflection, TokenConditions.IsAGodanPotentialStem));
      _godanPotentialForbidden = VocabWith("れる", MatchingRules.GodanPotential.WhenForbidden);
      _irrealisSingleTokenAndNoIchidan = VocabWith("ない", MatchingRules.Irrealis.WhenRequired, MatchingRules.SingleToken.WhenRequired, MatchingRules.Ichidan.WhenForbidden);
      _irrealisAndNoIchidan = VocabWith("せる", MatchingRules.Irrealis.WhenRequired, MatchingRules.Ichidan.WhenForbidden);
      _followedByAVerb = VocabWith("を", FollowingToken.AnyOf(TokenConditions.IsAnIchidanVerb, TokenConditions.IsAGodanVerb));
      _masuStem = VocabWith("ます", MatchingRules.MasuStem.WhenRequired);

      _matchConditionsBefore = TheVocab.ToDictionary(it => it, it => it.MatchingConfiguration.MatchCondition);
      _weightsBefore = TheVocab.ToDictionary(it => it, it => it.MatchingConfiguration.CustomRequirementsWeight);
   }

   IReadOnlyList<VocabNote> TheVocab => [_godanPotentialInTheOtherOrder, _godanPotentialForbidden, _irrealisSingleTokenAndNoIchidan, _irrealisAndNoIchidan, _followedByAVerb, _masuStem];

   VocabNote VocabWith(string question, params Condition<CandidateWordVariant>[] conditions)
   {
      var vocab = CreateVocab(question, question, question);
      vocab.MatchingConfiguration.SetMatchCondition(AllOf(conditions));
      return vocab;
   }

   protected static AllOfGroup<CandidateWordVariant> MatchConditionOf(VocabNote vocab) => vocab.MatchingConfiguration.MatchCondition;

   public class before_using_them : When_using_a_shared_condition_wherever_its_definition_is
   {
      [XF] public void godan_potentials_condition_with_its_facts_in_the_other_order_weighs_what_the_rule_does_since_it_means_the_same() =>
         _godanPotentialInTheOtherOrder.MatchingConfiguration.CustomRequirementsWeight.Must().Be(30);

      [XF] public void and_reads_as_the_rule_required() => _godanPotentialInTheOtherOrder.MatchingConfiguration.Rules.ChoiceOf(MatchingRules.GodanPotential).Must().Be(RequireForbidChoice.Required);

      [XF] public void the_combination_with_single_token_weighs_what_its_three_rules_do_50_30_and_1() =>
         _irrealisSingleTokenAndNoIchidan.MatchingConfiguration.CustomRequirementsWeight.Must().Be(81);
   }

   public class after_using_each_of_them : When_using_a_shared_condition_wherever_its_definition_is
   {
      public after_using_each_of_them()
      {
         foreach(var sharedCondition in new ISharedCondition[] { _isAVerb, _startsInAGodanPotentialForm, _followsAnIrrealisFormOfNoIchidanVerb, _followsAVerb })
            _sharedConditions.UseWhereverItsDefinitionIs(sharedCondition);
      }

      [XF] public void the_godan_potential_in_the_other_order_uses_its_shared_condition() =>
         MatchConditionOf(_godanPotentialInTheOtherOrder).Must().Be(AllOf(_startsInAGodanPotentialForm));

      [XF] public void the_forbidden_godan_potential_uses_its_shared_condition_negated() =>
         MatchConditionOf(_godanPotentialForbidden).Must().Be(AllOf(!_startsInAGodanPotentialForm));

      [XF] public void the_combination_among_others_is_its_shared_condition_in_the_place_of_its_first_condition() =>
         MatchConditionOf(_irrealisSingleTokenAndNoIchidan).Must().Be(AllOf(_followsAnIrrealisFormOfNoIchidanVerb, MatchingRules.SingleToken.WhenRequired));

      [XF] public void the_match_condition_that_is_the_whole_combination_is_its_shared_condition_alone() =>
         MatchConditionOf(_irrealisAndNoIchidan).Must().Be(AllOf(_followsAnIrrealisFormOfNoIchidanVerb));

      [XF] public void the_condition_on_a_token_is_used_under_the_following_token() => MatchConditionOf(_followedByAVerb).Must().Be(AllOf(FollowingToken.Meets(_isAVerb)));

      [XF] public void the_match_condition_holding_no_definition_is_as_it_was() => MatchConditionOf(_masuStem).Must().Be(AllOf(MatchingRules.MasuStem.WhenRequired));

      [XF] public void the_definition_of_FollowsAVerb_uses_IsAVerb() => _followsAVerb.Definition.Must().Be(PrecedingToken.Meets(_isAVerb));

      [XF] public void the_definition_of_IsAVerb_is_as_it_was() => _isAVerb.Definition.Must().Be(AnyOf(TokenConditions.IsAGodanVerb, TokenConditions.IsAnIchidanVerb));

      [XF] public void every_match_condition_means_the_same_as_before() =>
         TheVocab.Where(it => !MatchConditionOf(it).MeansTheSameAs(_matchConditionsBefore[it])).Select(it => it.GetQuestion()).Must().BeEmpty();

      [XF] public void every_vocab_weighs_what_it_did_before() =>
         TheVocab.Select(it => it.MatchingConfiguration.CustomRequirementsWeight).Must().SequenceEqual(TheVocab.Select(it => _weightsBefore[it]));

      [XF] public void the_godan_potential_reads_as_the_rule_required_still() =>
         _godanPotentialInTheOtherOrder.MatchingConfiguration.Rules.ChoiceOf(MatchingRules.GodanPotential).Must().Be(RequireForbidChoice.Required);

      [XF] public void the_vocab_as_stored_and_read_back_uses_the_shared_condition_by_name() =>
         MatchConditionOf(AsStoredAndReadBack(_irrealisAndNoIchidan)).Must().Be(AllOf(_followsAnIrrealisFormOfNoIchidanVerb));

      /// <summary><paramref name="vocab"/> as the corpus stores it, read back, as the collection reads it from its file. The specifications keep the notes in memory, so a reload doesn't.</summary>
      VocabNote AsStoredAndReadBack(VocabNote vocab)
      {
         var serializer = GetService<NoteSerializer>();
         return serializer.DeserializeVocab(serializer.Serialize(vocab));
      }

      public class and_reloading_the_collection : after_using_each_of_them
      {
         public and_reloading_the_collection() => GetService<JPCollection>().ReloadFromBackend();

         [XF] public void FollowsAVerb_is_read_back_from_its_file_using_IsAVerb() =>
            _sharedConditions.All.Single(it => it.FullName == "Word.FollowsAVerb").Definition.ToString().Must().Be("PrecedingToken.IsAVerb");
      }
   }
}
