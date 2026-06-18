using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Tail;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;

public sealed class VocabMatch : Match
{
   static readonly List<Func<VocabMatchInspector, FailedMatchRequirement?>> VocabStaticDisplayRequirementsList =
   [
      ForbidsCompositionallyTransparentCompound.ApplyTo,
      ForbidsYieldsToValidSurfaceSurface.ApplyTo
   ];

   static readonly List<Func<VocabMatchInspector, FailedMatchRequirement?>> VocabStaticDisplayRequirementsListCombined;

   static readonly List<Func<VocabMatchInspector, FailedMatchRequirement?>> RequirementsList =
   [
      new RequiresOrForbids("irrealis",
                            it => it.RequiresForbids.Irrealis,
                            it => it.PreviousLocationIsIrrealis).ApplyTo,

      new RequiresOrForbids("godan",
                            it => it.RequiresForbids.Godan,
                            it => it.PreviousLocationIsGodan).ApplyTo,

      new RequiresOrForbids("ichidan",
                            it => it.RequiresForbids.Ichidan,
                            it => it.PreviousLocationIsIchidan).ApplyTo,

      // head requirements

      ForbidsPrefixIsIn.ApplyTo,
      RequiresPrefixIsIn.ApplyTo,
      RequiresOrForbidsIsSentenceStart.ApplyTo,
      new RequiresOrForbids("te_form_prefix",
                            it => it.RequiresForbids.TeFormStemHead,
                            it => it.HasTeFormStemHead).ApplyTo,

      RequiresOrForbidsTeFormStemStart.ApplyTo,
      RequiresOrForbidsPastTenseStem.ApplyTo,

      RequiresOrForbidsGodanImperativeHead.ApplyTo,
      RequiresOrForbidsStartsWithGodanPotentialStemOrInflection.ApplyTo,
      RequiresOrForbidsStartsWithGodanImperativeStemOrInflection.ApplyTo,
      RequiresOrForbidsStartsWithIchidanImperativeStemOrInflection.ApplyTo,

      // tail requirements
      RequiresOrForbidsIsSentenceEnd.ApplyTo,
      ForbidsSuffixIsIn.ApplyTo,

      // misc requirements
      ForbidsIsPoisonWord.ApplyTo,
      RequiresOrForbidsMasuStem.ApplyTo,
      RequiresOrForbidsAdverbHead.ApplyTo,
      RequiresOrForbidsDictionaryFormStart.ApplyTo,
      RequiresOrForbidsDictionaryFormHead.ApplyTo,

      RequiresOrForbidsSurface.ApplyTo,
      RequiresOrForbidsIsSingleToken.ApplyTo,
      ForbidsSurfaceIsIn.ApplyTo
   ];

   static readonly List<Func<VocabMatchInspector, FailedMatchRequirement?>> CombinedRequirements;

   static VocabMatch()
   {
      VocabStaticDisplayRequirementsListCombined = new List<Func<VocabMatchInspector, FailedMatchRequirement?>>(VocabStaticDisplayRequirementsList);
      VocabStaticDisplayRequirementsListCombined.AddRange(
         MatchStaticDisplayRequirements.Select<Func<MatchInspector, FailedMatchRequirement?>, Func<VocabMatchInspector, FailedMatchRequirement?>>(req => inspector => req(inspector)));

      CombinedRequirements = new List<Func<VocabMatchInspector, FailedMatchRequirement?>>(
         MatchPrimaryValidityRequirements.Select<Func<MatchInspector, FailedMatchRequirement?>, Func<VocabMatchInspector, FailedMatchRequirement?>>(req => inspector => req(inspector)));
      CombinedRequirements.AddRange(RequirementsList);
   }

   // Access to parent class static fields
   static List<Func<MatchInspector, FailedMatchRequirement?>> MatchStaticDisplayRequirements =>
      typeof(Match).GetField("MatchStaticDisplayRequirements",
                             System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
        .GetValue(null) as List<Func<MatchInspector, FailedMatchRequirement?>> ?? [];

   static List<Func<MatchInspector, FailedMatchRequirement?>> MatchPrimaryValidityRequirements =>
      typeof(Match).GetField("MatchPrimaryValidityRequirements",
                             System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
        .GetValue(null) as List<Func<MatchInspector, FailedMatchRequirement?>> ?? [];

   public VocabMatchInspector VocabInspector { get; }
   public VocabMatchingRulesConfigurationRequiresForbidsFlags RequiresForbids { get; }
   public VocabNoteMatchingRules Rules { get; }
   public VocabNote Vocab { get; }

   bool? _anotherMatchOwnsTheFormCache = null;

   public VocabMatch(CandidateWordVariant wordVariant, VocabNote vocab) : base(wordVariant)
   {
      VocabInspector = new VocabMatchInspector(this);
      RequiresForbids = vocab.MatchingConfiguration.RequiresForbids;
      Rules = vocab.MatchingConfiguration.ConfigurableRules;
      Vocab = vocab;
   }

   protected override List<FailedMatchRequirement> CreateStaticDisplayRequirementFailures()
   {
      return VocabStaticDisplayRequirementsListCombined
            .Select(requirement => requirement(VocabInspector))
            .Where(failure => failure != null)
            .Cast<FailedMatchRequirement>()
            .ToList();
   }

   protected override bool StaticDisplayRequirementsFulfilledInternal()
   {
      return !VocabStaticDisplayRequirementsListCombined
             .Select(requirement => requirement(VocabInspector))
             .Any(failure => failure != null);
   }

   protected override IEnumerable<MatchRequirement?> CreateDynamicDisplayRequirements() => [ForbidsHasDisplayedOverlappingFollowingCompound.ApplyTo(VocabInspector)];

   protected override List<FailedMatchRequirement> CreatePrimaryValidityFailures()
   {
      return CombinedRequirements
            .Select(requirement => requirement(VocabInspector))
            .Where(failure => failure != null)
            .Cast<FailedMatchRequirement>()
            .ToList();
   }

   protected override bool IsPrimarilyValid()
   {
      return CombinedRequirements.Select(requirement => requirement(VocabInspector))
                                 .All(failure => failure == null);
   }

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
      MatchingConfiguration.BoolFlags.QuestionOverridesForm.IsSet()
         ? Vocab.Question.Raw
         : base.ParsedForm;

   public override string ExclusionForm
   {
      get
      {
         var question = Vocab.Question;
         var tokenizedForm = TokenizedForm;
         if(question.Raw == tokenizedForm && question.IsDisambiguated)
         {
            return question.DisambiguationName;
         }

         return question.Raw;
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
      if(MatchingConfiguration.BoolFlags.QuestionOverridesForm.IsSet())
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
