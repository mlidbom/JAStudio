using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping.ConditionGroup;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// The vocab だの's match-condition is <c>AllOf(!PrecedingToken.Surface.EndsWithOneOf("ん"), PrecedingToken.IsAGodanVerb)</c>, edited in the tree editor<br/>
/// (<see cref="EditableCondition"/>), and んだ's is <c>PrecedingToken.Surface.EndsWithOneOf("ん")</c>.
/// </summary>
public class When_making_a_shared_condition_of_a_line_of_the_tree_editor : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   static readonly Condition<CandidateWordVariant> FollowsASurfaceEndingWithん = PrecedingToken.Meets(TokenConditions.Surface.Meets(TextConditions.EndsWithOneOf("ん")));

   readonly VocabNote _だの;
   readonly VocabNote _んだ;
   readonly EditableCondition _treeOfだの;

   public When_making_a_shared_condition_of_a_line_of_the_tree_editor()
   {
      _だの = VocabWith("だの", !FollowsASurfaceEndingWithん, PrecedingToken.Meets(TokenConditions.IsAGodanVerb));
      _んだ = VocabWith("んだ", FollowsASurfaceEndingWithん);
      _treeOfだの = new EditableCondition(_だの.MatchingConfiguration.MatchCondition, Level);
   }

   VocabNote VocabWith(string question, params Condition<CandidateWordVariant>[] conditions)
   {
      var vocab = CreateVocab(question, question, question);
      vocab.MatchingConfiguration.SetMatchCondition(AllOf(conditions));
      return vocab;
   }

   SharedConditions SharedConditions => GetService<SharedConditions>();

   static ConditionPosition TheLineAt(int index) => ConditionPosition.Root.Child(index);

   static List<string> Offered(IEnumerable<ConditionWithItsLevel> conditions) => [.. conditions.Select(it => $"{it.Level.SubjectNameInCode}: {it.Condition}")];

   [XF] public void the_negated_line_offers_the_condition_inside_the_negation_on_a_word_then_what_it_holds_on_a_token_and_on_a_text() =>
      Offered(_treeOfだの.ConditionsToMakeASharedConditionOfAt(TheLineAt(0)))
         .Must().SequenceEqual(["""Word: PrecedingToken.Surface.EndsWithOneOf("ん")""", """Token: Surface.EndsWithOneOf("ん")""", """Text: EndsWithOneOf("ん")"""]);

   [XF] public void the_line_of_a_fact_on_the_preceding_token_offers_nothing_since_it_is_a_name_already() =>
      Offered(_treeOfだの.ConditionsToMakeASharedConditionOfAt(TheLineAt(1))).Must().HaveCount(0);

   [XF] public void the_root_of_two_conditions_offers_itself() =>
      Offered(_treeOfだの.ConditionsToMakeASharedConditionOfAt(ConditionPosition.Root))
         .Must().SequenceEqual(["""Word: AllOf(!PrecedingToken.Surface.EndsWithOneOf("ん"), PrecedingToken.IsAGodanVerb)"""]);

   [XF] public void the_root_of_one_condition_offers_nothing() =>
      Offered(new EditableCondition(_んだ.MatchingConfiguration.MatchCondition, Level).ConditionsToMakeASharedConditionOfAt(ConditionPosition.Root)).Must().HaveCount(0);

   [XF] public void a_name_in_code_on_a_subject_with_no_shared_condition_of_that_name_can_name_a_new_one() =>
      SharedConditions.WhyItCannotNameANewOne(TokenConditions.Level, "EndsInN").Must().Satisfy(why => why is null);

   [XF] public void a_name_not_in_code_cannot_name_a_new_one() => SharedConditions.WhyItCannotNameANewOne(TokenConditions.Level, "endsInN").Must().Satisfy(why => why is not null);

   public class after_making_Token_EndsInN_of_what_the_negated_line_holds_on_a_token : When_making_a_shared_condition_of_a_line_of_the_tree_editor
   {
      public after_making_Token_EndsInN_of_what_the_negated_line_holds_on_a_token() =>
         SharedConditions.MakeOf(_treeOfだの.ConditionsToMakeASharedConditionOfAt(TheLineAt(0))[1], "EndsInN");

      [XF] public void it_is_defined_as_that_condition() =>
         SharedConditions.All.Select(it => $"{it.FullName} = {it.Definition}").Must().SequenceEqual(["""Token.EndsInN = Surface.EndsWithOneOf("ん")"""]);

      [XF] public void it_is_saved_in_its_file() =>
         File.Exists(Path.Combine(GetService<IEnvironmentPaths>().DatabaseDir, "shared-conditions", "Token.EndsInN.json")).Must().BeTrue();

      [XF] public void the_vocab_whose_tree_it_was_made_from_uses_it_inside_the_negation() =>
         _だの.MatchingConfiguration.MatchCondition.ToString().Must().Be("AllOf(!PrecedingToken.EndsInN, PrecedingToken.IsAGodanVerb)");

      [XF] public void the_other_vocab_holding_its_definition_uses_it_too() => _んだ.MatchingConfiguration.MatchCondition.ToString().Must().Be("AllOf(PrecedingToken.EndsInN)");

      [XF] public void its_name_cannot_name_another_one_on_a_token() =>
         SharedConditions.WhyItCannotNameANewOne(TokenConditions.Level, "EndsInN").Must().Satisfy(why => why == "There's a shared-condition Token.EndsInN already.");
   }
}
