using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests.Head;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests.Tail;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches;

public sealed class VocabMatch : Match
{
   static readonly List<Func<VocabMatchInspector, FailedMatchRequirement?>> VocabStaticDisplayRequirementsList =
   [
      ForbidsCompositionallyTransparentCompound.ApplyTo,
      ForbidsYieldsToValidSurfaceSurface.ApplyTo
   ];

   static readonly List<Func<VocabMatchInspector, FailedMatchRequirement?>> VocabStaticDisplayRequirementsListCombined =
      [.. VocabStaticDisplayRequirementsList, .. MatchStaticDisplayRequirements];

   /// <summary>
   /// The vocab's requirements that its <see cref="MatchCondition"/> doesn't replace: the poison word flag, and dictionary form (start), which is<br/>
   /// forbidden by default for every vocab (open question 1 in dev_docs/matching-configuration.md).
   /// </summary>
   static readonly List<Func<VocabMatchInspector, FailedMatchRequirement?>> RequirementsBesidesTheMatchCondition =
   [
      ForbidsIsPoisonWord.ApplyTo,
      RequiresOrForbidsDictionaryFormStart.ApplyTo
   ];

   static readonly List<Func<VocabMatchInspector, FailedMatchRequirement?>> CombinedRequirements =
      [.. MatchPrimaryValidityRequirements, .. RequirementsBesidesTheMatchCondition];

   public VocabMatchInspector VocabInspector { get; }
   public VocabMatchingRequireForbidFlags RequiresForbids { get; }
   public VocabNote Vocab { get; }

   bool? _anotherMatchOwnsTheFormCache = null;

   public VocabMatch(CandidateWordVariant wordVariant, VocabNote vocab) : base(wordVariant)
   {
      VocabInspector = new VocabMatchInspector(this);
      RequiresForbids = vocab.MatchingConfiguration.RequiresForbids;
      Vocab = vocab;
   }

   protected override List<FailedMatchRequirement> CreateStaticDisplayRequirementFailures() =>
      [.. VocabStaticDisplayRequirementsListCombined.Select(requirement => requirement(VocabInspector)).OfType<FailedMatchRequirement>()];

   protected override bool StaticDisplayRequirementsFulfilledInternal() => NoneFail(VocabStaticDisplayRequirementsListCombined, VocabInspector);

   protected override IEnumerable<MatchRequirement?> CreateDynamicDisplayRequirements() => [ForbidsHasDisplayedOverlappingFollowingCompound.ApplyTo(VocabInspector)];

   protected override List<FailedMatchRequirement> CreatePrimaryValidityFailures() =>
   [
      .. CombinedRequirements.Select(requirement => requirement(VocabInspector)).OfType<FailedMatchRequirement>(),
      .. MatchCondition.ConditionsNotMetBy(Variant).Select(FailedMatchRequirement.NotMet)
   ];

   protected override bool IsPrimarilyValid() => NoneFail(CombinedRequirements, VocabInspector) && MatchCondition.IsMetBy(Variant);

   /// <summary>The vocab's match-condition (<see cref="VocabNoteMatchingConfiguration.MatchCondition"/>): a word matches the vocab only when it meets it.</summary>
   public AllOfGroup<CandidateWordVariant> MatchCondition => Vocab.MatchingConfiguration.MatchCondition;

   protected override List<FailedMatchRequirement> CreateInterdependentValidityFailures()
   {
      var failure = ForbidsAnotherMatchIsHigherPriority.ApplyTo(VocabInspector);
      return failure != null ? [failure] : [];
   }

   protected override bool IsInterdependentlyValid() => ForbidsAnotherMatchIsHigherPriority.ApplyTo(VocabInspector) == null;

   public VocabNoteMatchingConfiguration MatchingConfiguration => Vocab.MatchingConfiguration;
   public override string MatchForm => Vocab.GetQuestion();
   public override string Answer => Vocab.GetAnswer();
   public override List<string> Readings => Vocab.GetReadings();

   public override string ParsedForm =>
      MatchingConfiguration.Flags.QuestionOverridesForm.IsSet()
         ? Vocab.Question.Text
         : base.ParsedForm;

   public override string ExclusionForm
   {
      get
      {
         var question = Vocab.Question;
         var tokenizedForm = TokenizedForm;
         if(question.Text == tokenizedForm && question.IsDisambiguated)
         {
            return question.DisambiguationName;
         }

         return question.Text;
      }
   }

   public bool AnotherMatchIsHigherPriority
   {
      get
      {
         if(_anotherMatchOwnsTheFormCache == null)
         {
            if(Variant.VocabMatches.Any(otherMatch =>
                                           otherMatch != this &&
                                           otherMatch.IsPrimarilyValidProperty &&
                                           otherMatch.IsHigherPriorityForMatch(this)))
            {
               _anotherMatchOwnsTheFormCache = true;
            } else
            {
               _anotherMatchOwnsTheFormCache = false;
            }
         }

         return _anotherMatchOwnsTheFormCache.Value;
      }
   }

   bool IsHigherPriorityForMatch(VocabMatch other)
   {
      var ownsForm = Vocab.Forms.IsOwnedForm(TokenizedForm);
      var otherOwnsForm = other.Vocab.Forms.IsOwnedForm(TokenizedForm);
      if(ownsForm && !otherOwnsForm)
      {
         return true;
      }

      if(!ownsForm && otherOwnsForm)
      {
         return false;
      }

      if(Vocab.MatchingConfiguration.CustomRequirementsWeight > other.MatchingConfiguration.CustomRequirementsWeight)
      {
         return true;
      }

      return false;
   }

   protected override int StartIndexInternal()
   {
      if(MatchingConfiguration.Flags.QuestionOverridesForm.IsSet())
      {
         var lengthDiff = Vocab.GetQuestion().Length - TokenizedForm.Length;
         if(lengthDiff != 0 && TokenizedForm != "" && Vocab.GetQuestion().Contains(TokenizedForm))
         {
            return base.StartIndexInternal() - lengthDiff;
         }
      }

      return base.StartIndexInternal();
   }

   public override string ToString() =>
      $"{Vocab.GetQuestion()}, {Vocab.GetAnswer()[..Math.Min(10, Vocab.GetAnswer().Length)]}: " +
      $"{MatchForm[..Math.Min(10, MatchForm.Length)]}: " +
      $"failure_reasons: {string.Join(" ", FailureReasons)} " +
      $"## hiding_reasons: {string.Join(" ", HidingReasons)}";
}
