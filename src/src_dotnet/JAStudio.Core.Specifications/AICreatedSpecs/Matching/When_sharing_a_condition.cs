using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;
using JAStudio.Core.Storage;
using static Compze.Must.MustActions;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// The corpus has two shared-conditions, one using the other: <c>Token.IsAVerb</c>, <c>AnyOf(IsAGodanVerb, IsAnIchidanVerb)</c>, and <c>Word.FollowsAVerb</c>,<br/>
/// <c>PrecedingToken.IsAVerb</c>.
/// </summary>
public abstract class When_sharing_a_condition : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly SharedConditions _sharedConditions;
   protected readonly SharedCondition<IAnalysisToken> _isAVerb;
   protected readonly SharedCondition<CandidateWordVariant> _followsAVerb;

   protected When_sharing_a_condition()
   {
      _sharedConditions = GetService<SharedConditions>();
      _isAVerb = _sharedConditions.Define(TokenConditions.Level, "IsAVerb", ConditionGroup.AnyOf(TokenConditions.IsAGodanVerb, TokenConditions.IsAnIchidanVerb));
      _followsAVerb = _sharedConditions.Define(WordConditions.Level, "FollowsAVerb", PrecedingToken.Meets(_isAVerb));
   }

   protected string TheFolderOfTheSharedConditions => Path.Combine(GetService<IEnvironmentPaths>().DatabaseDir, "shared-conditions");

   protected IReadOnlyList<string> FullNamesOf(IEnumerable<ISharedCondition> sharedConditions) => [.. sharedConditions.Select(it => it.FullName)];

   public class as_defined : When_sharing_a_condition
   {
      [XF] public void the_shared_conditions_are_listed_those_on_a_word_first() => FullNamesOf(_sharedConditions.All).Must().SequenceEqual(["Word.FollowsAVerb", "Token.IsAVerb"]);

      [XF] public void the_one_on_a_token_is_offered_where_a_condition_on_a_token_goes() => FullNamesOf(_sharedConditions.On(TokenConditions.Level)).Must().SequenceEqual(["Token.IsAVerb"]);

      [XF] public void the_one_on_a_word_is_offered_where_a_condition_on_a_word_goes() => FullNamesOf(_sharedConditions.On(WordConditions.Level)).Must().SequenceEqual(["Word.FollowsAVerb"]);

      [XF] public void each_is_kept_in_a_file_of_its_own_in_the_corpus_named_by_its_full_name() =>
         new DirectoryInfo(TheFolderOfTheSharedConditions).GetFiles().Select(it => it.Name).Order(StringComparer.Ordinal).Must().SequenceEqual(["Token.IsAVerb.json", "Word.FollowsAVerb.json"]);

      [XF] public void the_one_on_a_word_is_written_as_code_by_its_full_name_with_its_definition_under_it() =>
         ConditionCode.AsText(ConditionCode.LinesOfASharedCondition(_followsAVerb)).Must().Be($"Word.FollowsAVerb ={Environment.NewLine}   PrecedingToken.IsAVerb");

      [XF] public void the_one_on_a_word_uses_the_one_on_a_token() => FullNamesOf(_followsAVerb.Definition.SharedConditionsUsed()).Must().SequenceEqual(["Token.IsAVerb"]);

      [XF] public void defining_another_with_a_name_one_has_already_on_the_same_subject_throws() =>
         Invoking(() => _sharedConditions.Define(WordConditions.Level, "FollowsAVerb", IsASingleToken)).Must().Throw<ArgumentAssertionFailedException>();

      [XF] public void defining_one_with_the_name_of_a_fact_on_the_same_subject_throws() =>
         Invoking(() => _sharedConditions.Define(TokenConditions.Level, "IsAGodanVerb", TokenConditions.IsAnAdverb)).Must().Throw<ArgumentAssertionFailedException>();

      [XF] public void defining_one_whose_name_is_no_name_in_code_throws() =>
         Invoking(() => _sharedConditions.Define(WordConditions.Level, "follows a verb", IsASingleToken)).Must().Throw<ArgumentAssertionFailedException>();
   }

   public class tested_on_a_word_in_its_sentence : When_sharing_a_condition
   {
      bool Meets(string word, string sentence) =>
         _followsAVerb.IsMetBy(CreateSentence(sentence).CreateAnalysis().Locations.SelectMany(it => it.CandidateWords).Single(it => it.SurfaceForm == word).SurfaceVariant);

      [XF] public void ます_after_the_godan_verb_読み_in_読みます_meets_it() => Meets("ます", "読みます").Must().BeTrue();

      [XF] public void ない_after_the_ichidan_verb_食べ_in_食べない_meets_it() => Meets("ない", "食べない").Must().BeTrue();

      [XF] public void 本_at_the_start_of_本を読みます_does_not_meet_it() => Meets("本", "本を読みます").Must().BeFalse();
   }

   /// <summary>The vocab ます's match-condition is <c>FollowsAVerb</c>.</summary>
   public class used_in_a_vocabs_match_condition : When_sharing_a_condition
   {
      protected readonly VocabNote _vocab;

      public used_in_a_vocabs_match_condition()
      {
         _vocab = CreateVocab("ます", "polite ending", "ます");
         _vocab.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf<CandidateWordVariant>(_followsAVerb));
      }

      [XF] public void the_match_condition_is_written_as_code_with_the_shared_conditions_name() =>
         ConditionCode.AsText(ConditionCode.LinesOfAMatchCondition(_vocab.MatchingConfiguration.MatchCondition))
                      .Must().Be($"// {ConditionCode.MatchConditionHeading}{Environment.NewLine}FollowsAVerb");

      [XF] public void the_match_condition_uses_the_shared_condition() => FullNamesOf(_vocab.MatchingConfiguration.MatchCondition.SharedConditionsUsed()).Must().SequenceEqual(["Word.FollowsAVerb"]);

      /// <summary>The vocab as the corpus stores it, read back, as the collection reads it from its file. The specifications keep the notes in memory, so a reload doesn't.</summary>
      protected VocabNote TheVocabAsStoredReadBack()
      {
         var serializer = GetService<NoteSerializer>();
         return serializer.DeserializeVocab(serializer.Serialize(_vocab));
      }

      public class after_reloading_the_collection : used_in_a_vocabs_match_condition
      {
         readonly VocabNote _vocabAsStored;

         public after_reloading_the_collection()
         {
            GetService<JPCollection>().ReloadFromBackend();
            _vocabAsStored = TheVocabAsStoredReadBack();
         }

         [XF] public void the_vocab_as_stored_uses_the_shared_condition_by_its_name() =>
            _vocabAsStored.MatchingConfiguration.MatchCondition.Must().Be(ConditionGroup.AllOf<CandidateWordVariant>(_followsAVerb));

         [XF] public void the_shared_conditions_are_read_back_from_their_files_with_their_definitions() =>
            _sharedConditions.All.Select(it => $"{it.FullName} = {it.Definition}").Must().SequenceEqual(["Word.FollowsAVerb = PrecedingToken.IsAVerb", "Token.IsAVerb = AnyOf(IsAGodanVerb, IsAnIchidanVerb)"]);
      }

      public class after_its_file_was_deleted_and_the_collection_reloaded : used_in_a_vocabs_match_condition
      {
         public after_its_file_was_deleted_and_the_collection_reloaded()
         {
            File.Delete(Path.Combine(TheFolderOfTheSharedConditions, "Word.FollowsAVerb.json"));
            GetService<JPCollection>().ReloadFromBackend();
         }

         [XF] public void reading_the_vocab_as_stored_throws_since_it_uses_a_shared_condition_there_is_none_of() =>
            Invoking(TheVocabAsStoredReadBack).Must().Throw<FormatException>();
      }
   }

   public class after_a_file_was_changed_so_that_IsAVerb_uses_itself : When_sharing_a_condition
   {
      public after_a_file_was_changed_so_that_IsAVerb_uses_itself() =>
         File.WriteAllText(Path.Combine(TheFolderOfTheSharedConditions, "Token.IsAVerb.json"), """{"subject": "Token", "name": "IsAVerb", "definition": {"sharedCondition": "IsAVerb"}}""");

      [XF] public void reloading_the_collection_throws() => Invoking(() => GetService<JPCollection>().ReloadFromBackend()).Must().Throw<FormatException>();
   }

   /// <summary>The corpus also has <c>Token.IsAVerbToo</c>, <c>IsAVerb</c>, and then <c>Token.IsAVerb</c>'s file is changed to make it <c>IsAVerbToo</c>.</summary>
   public class after_a_file_was_changed_so_that_IsAVerb_uses_IsAVerbToo_which_uses_it : When_sharing_a_condition
   {
      public after_a_file_was_changed_so_that_IsAVerb_uses_IsAVerbToo_which_uses_it()
      {
         _sharedConditions.Define(TokenConditions.Level, "IsAVerbToo", _isAVerb);
         File.WriteAllText(Path.Combine(TheFolderOfTheSharedConditions, "Token.IsAVerb.json"), """{"subject": "Token", "name": "IsAVerb", "definition": {"sharedCondition": "IsAVerbToo"}}""");
      }

      [XF] public void reloading_the_collection_throws() => Invoking(() => GetService<JPCollection>().ReloadFromBackend()).Must().Throw<FormatException>();
   }

   public class after_a_file_was_changed_so_that_IsAVerb_holds_a_condition_on_a_word : When_sharing_a_condition
   {
      public after_a_file_was_changed_so_that_IsAVerb_holds_a_condition_on_a_word() =>
         File.WriteAllText(Path.Combine(TheFolderOfTheSharedConditions, "Token.IsAVerb.json"), """{"subject": "Token", "name": "IsAVerb", "definition": {"fact": "singleToken"}}""");

      [XF] public void reloading_the_collection_throws() => Invoking(() => GetService<JPCollection>().ReloadFromBackend()).Must().Throw<FormatException>();
   }

   public class after_a_file_was_renamed_so_that_its_name_is_not_its_shared_conditions : When_sharing_a_condition
   {
      public after_a_file_was_renamed_so_that_its_name_is_not_its_shared_conditions() =>
         File.Move(Path.Combine(TheFolderOfTheSharedConditions, "Token.IsAVerb.json"), Path.Combine(TheFolderOfTheSharedConditions, "Token.IsANoun.json"));

      [XF] public void reloading_the_collection_throws() => Invoking(() => GetService<JPCollection>().ReloadFromBackend()).Must().Throw<FormatException>();
   }
}
