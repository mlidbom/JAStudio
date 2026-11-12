using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

public class When_a_vocab_has_a_form_starting_with_the_dictionary_form_ending_placeholder : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   static readonly HashSet<string> TheFormStartingWithEachOtherDictionaryFormEnding = ["く限り", "ぐ限り", "す限り", "つ限り", "ぬ限り", "ぶ限り", "む限り", "る限り"];

   readonly VocabNote _vocab;

   protected When_a_vocab_has_a_form_starting_with_the_dictionary_form_ending_placeholder() => _vocab = CreateVocab("う限り", "as-long-as", "うかぎり");

   static IEnumerable<string> TheFormStartingWithEachOtherDictionaryFormEndingFollowedBy(string suffix) => TheFormStartingWithEachOtherDictionaryFormEnding.Select(form => form + suffix);

   VocabCollection VocabCollection => GetService<VocabCollection>();

   bool TheVocabCollectionFindsTheVocabByEachOfTheFormsItExpandsTo => TheFormStartingWithEachOtherDictionaryFormEnding.All(form => VocabCollection.WithForm(form).Contains(_vocab));
   bool TheVocabCollectionFindsTheVocabByAnyOfTheFormsItExpandsTo => TheFormStartingWithEachOtherDictionaryFormEnding.Any(form => VocabCollection.WithForm(form).Contains(_vocab));

   public class that_does_not_require_the_dictionary_form_start : When_a_vocab_has_a_form_starting_with_the_dictionary_form_ending_placeholder
   {
      [XF] public void the_matched_forms_are_the_question_and_the_entered_forms() => _vocab.Forms.MatchedFormsSet().Must().Satisfy(it => it.SetEquals(_vocab.Forms.QuestionAndEnteredFormsList()));
      [XF] public void the_vocab_collection_does_not_find_the_vocab_by_any_of_the_forms_starting_with_another_dictionary_form_ending() => TheVocabCollectionFindsTheVocabByAnyOfTheFormsItExpandsTo.Must().BeFalse();
   }

   public class that_requires_the_dictionary_form_start : When_a_vocab_has_a_form_starting_with_the_dictionary_form_ending_placeholder
   {
      public that_requires_the_dictionary_form_start() => _vocab.MatchingConfiguration.RequiresForbids.DictionaryFormStart.Choose(RequireForbidChoice.Required);

      [XF] public void the_question_and_the_entered_forms_are_only_the_form_starting_with_the_placeholder() => _vocab.Forms.QuestionAndEnteredFormsList().Must().SequenceEqual(["う限り"]);
      [XF] public void no_forms_are_stored_since_the_form_starting_with_the_placeholder_is_the_question() => _vocab.Forms.EnteredForms().Must().BeEmpty();
      [XF] public void the_matched_forms_are_the_form_starting_with_the_placeholder_and_the_form_starting_with_each_other_dictionary_form_ending() =>
         _vocab.Forms.MatchedFormsSet().Must().Satisfy(it => it.SetEquals(TheFormStartingWithEachOtherDictionaryFormEnding.Append("う限り")));
      [XF] public void the_vocab_collection_finds_the_vocab_by_the_form_starting_with_each_other_dictionary_form_ending() => TheVocabCollectionFindsTheVocabByEachOfTheFormsItExpandsTo.Must().BeTrue();
      [XF] public void the_form_starting_with_each_other_dictionary_form_ending_is_an_owned_form_because_the_question_is_an_owned_form() => TheFormStartingWithEachOtherDictionaryFormEnding.Must().Satisfy(it => it.All(_vocab.Forms.IsOwnedForm));

      public class and_a_plain_entered_form_starting_with_the_placeholder : that_requires_the_dictionary_form_start
      {
         public and_a_plain_entered_form_starting_with_the_placeholder() => _vocab.Forms.Add("う限りでは");

         [XF] public void the_form_starting_with_each_other_dictionary_form_ending_is_matched() => TheFormStartingWithEachOtherDictionaryFormEndingFollowedBy("では").Must().Satisfy(it => it.All(_vocab.Forms.MatchedFormsSet().Contains));
         [XF] public void none_of_the_forms_starting_with_another_dictionary_form_ending_is_an_owned_form() => TheFormStartingWithEachOtherDictionaryFormEndingFollowedBy("では").Must().Satisfy(it => !it.Any(_vocab.Forms.IsOwnedForm));
      }

      public class and_an_owned_entered_form_starting_with_the_placeholder : that_requires_the_dictionary_form_start
      {
         public and_an_owned_entered_form_starting_with_the_placeholder()
         {
            _vocab.Forms.Add("う限りでは");
            _vocab.Forms.Own("う限りでは");
         }

         [XF] public void the_form_starting_with_each_other_dictionary_form_ending_is_an_owned_form() => TheFormStartingWithEachOtherDictionaryFormEndingFollowedBy("では").Must().Satisfy(it => it.All(_vocab.Forms.IsOwnedForm));
      }

      public class and_an_entered_form_starting_with_another_dictionary_form_ending : that_requires_the_dictionary_form_start
      {
         public and_an_entered_form_starting_with_another_dictionary_form_ending() => _vocab.Forms.Add("る限りでは");

         [XF] public void that_form_is_matched() => _vocab.Forms.MatchedFormsSet().Must().Contain("る限りでは");
         [XF] public void that_form_is_not_expanded_to_the_other_dictionary_form_endings() => _vocab.Forms.MatchedFormsSet().Must().Satisfy(it => !it.Contains("く限りでは"));
      }

      public class after_the_requirement_is_removed_again : that_requires_the_dictionary_form_start
      {
         public after_the_requirement_is_removed_again() => _vocab.MatchingConfiguration.RequiresForbids.DictionaryFormStart.Choose(RequireForbidChoice.Unset);

         [XF] public void the_matched_forms_are_the_question_and_the_entered_forms() => _vocab.Forms.MatchedFormsSet().Must().Satisfy(it => it.SetEquals(_vocab.Forms.QuestionAndEnteredFormsList()));
         [XF] public void the_vocab_collection_no_longer_finds_the_vocab_by_any_of_the_forms_starting_with_another_dictionary_form_ending() => TheVocabCollectionFindsTheVocabByAnyOfTheFormsItExpandsTo.Must().BeFalse();
         [XF] public void none_of_the_forms_starting_with_another_dictionary_form_ending_is_an_owned_form_any_longer() => TheFormStartingWithEachOtherDictionaryFormEnding.Must().Satisfy(it => !it.Any(_vocab.Forms.IsOwnedForm));
      }

      public class after_creating_a_suffix_version_of_it : that_requires_the_dictionary_form_start
      {
         readonly VocabNote _suffixVersion;

         public after_creating_a_suffix_version_of_it() => _suffixVersion = _vocab.Cloner.CreateSuffixVersion("では").Vocab;

         [XF] public void the_question_and_the_entered_forms_of_the_suffix_version_are_only_the_question_with_the_suffix_appended() => _suffixVersion.Forms.QuestionAndEnteredFormsList().Must().SequenceEqual(["う限りでは"]);
         [XF] public void the_suffix_version_matches_the_form_starting_with_each_other_dictionary_form_ending_with_the_suffix_appended() =>
            TheFormStartingWithEachOtherDictionaryFormEndingFollowedBy("では").Must().Satisfy(it => it.All(_suffixVersion.Forms.MatchedFormsSet().Contains));
      }
   }
}
