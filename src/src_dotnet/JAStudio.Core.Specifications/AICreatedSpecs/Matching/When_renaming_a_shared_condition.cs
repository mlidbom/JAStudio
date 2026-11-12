using System;
using System.IO;
using System.Linq;
using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using static Compze.Must.MustActions;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// The shared-conditions of <see cref="When_sharing_a_condition"/>, <c>Token.IsAVerb</c> and <c>Word.FollowsAVerb</c>, <c>PrecedingToken.IsAVerb</c>, and the<br/>
/// vocab ます, whose match-condition is <c>AllOf(FollowsAVerb, PrecedingToken.IsAVerb)</c>.
/// </summary>
public class When_renaming_a_shared_condition : When_sharing_a_condition
{
   readonly VocabNote _ます;

   public When_renaming_a_shared_condition()
   {
      _ます = CreateVocab("ます", "polite ending", "ます");
      _ます.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(_followsAVerb, PrecedingToken.Meets(_isAVerb)));
   }

   void RenameIsAVerb(string newName) => _sharedConditions.Rename(_isAVerb, newName);

   [XF] public void renaming_it_its_own_name_throws_since_a_shared_condition_on_a_token_has_it() => Invoking(() => RenameIsAVerb("IsAVerb")).Must().Throw<ArgumentAssertionFailedException>();

   [XF] public void renaming_it_a_name_not_in_code_throws() => Invoking(() => RenameIsAVerb("is a verb")).Must().Throw<ArgumentAssertionFailedException>();

   public class after_renaming_IsAVerb_IsAVerbOfEitherKind : When_renaming_a_shared_condition
   {
      public after_renaming_IsAVerb_IsAVerbOfEitherKind() => RenameIsAVerb("IsAVerbOfEitherKind");

      [XF] public void it_has_the_new_name() => _isAVerb.FullName.Must().Be("Token.IsAVerbOfEitherKind");

      [XF] public void its_file_is_renamed_with_it() =>
         new DirectoryInfo(TheFolderOfTheSharedConditions).GetFiles().Select(it => it.Name).Order(StringComparer.Ordinal)
                                                          .Must().SequenceEqual(["Token.IsAVerbOfEitherKind.json", "Word.FollowsAVerb.json"]);

      [XF] public void the_definition_using_it_uses_it_by_the_new_name() => _followsAVerb.Definition.ToString().Must().Be("PrecedingToken.IsAVerbOfEitherKind");

      [XF] public void the_vocabs_match_condition_uses_it_by_the_new_name() =>
         _ます.MatchingConfiguration.MatchCondition.ToString().Must().Be("AllOf(FollowsAVerb, PrecedingToken.IsAVerbOfEitherKind)");

      public class and_reloading_the_collection : after_renaming_IsAVerb_IsAVerbOfEitherKind
      {
         public and_reloading_the_collection() => GetService<JPCollection>().ReloadFromBackend();

         [XF] public void the_shared_conditions_are_read_back_from_their_files_by_their_names() =>
            _sharedConditions.All.Select(it => $"{it.FullName} = {it.Definition}")
                             .Must().SequenceEqual(["Word.FollowsAVerb = PrecedingToken.IsAVerbOfEitherKind", "Token.IsAVerbOfEitherKind = AnyOf(IsAGodanVerb, IsAnIchidanVerb)"]);
      }
   }
}
