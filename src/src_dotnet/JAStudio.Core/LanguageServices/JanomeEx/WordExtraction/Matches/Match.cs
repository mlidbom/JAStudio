using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;

public abstract class Match
{
   /// <summary>The requirements every match must meet to be valid, whether to a vocab or to a dictionary entry.</summary>
   protected static readonly List<Func<MatchInspector, FailedMatchRequirement?>> MatchPrimaryValidityRequirements =
   [
      ForbidsIsConfiguredIncorrect.ApplyTo,
      ForbidsDictionaryInflectionSurfaceWithBase.ApplyTo,
      ForbidsDictionaryVerbFormStemAsCompoundEnd.ApplyTo,
      ForbidsIsGodanPotentialInflectionWithBase.ApplyTo,
      ForbidsIsGodanImperativeInflectionWithBase.ApplyTo,
      ForbidsSurfaceIfBaseIsValidAndContextIndicatesAVerb.ApplyTo,
      new Forbids("compound_ending_on_dictionary_form_where_surface_differs_from_base",
                  it => it.IsCompoundEndingOnDictionaryFormWhereSurfaceDiffersFromBase).ApplyTo
   ];

   /// <summary>The requirements every valid match must meet to be displayed that don't depend on which other matches are displayed.</summary>
   protected static readonly List<Func<MatchInspector, FailedMatchRequirement?>> MatchStaticDisplayRequirements =
   [
      ForbidsIsConfiguredHidden.ApplyTo,
      ForbidsConfiguredToHideAllCompounds.ApplyTo
   ];

   public MatchInspector Inspector { get; }
   public CandidateWordVariant Variant { get; }

   bool? _isPrimarilyValidInternalCache;
   bool? _isValidInternalCache;
   bool? _isValidCache;
   int? _startIndexCache;
   bool? _staticDisplayRequirementsFulfilledCache;
   List<MatchRequirement>? _displayRequirementsCache;

   protected Match(CandidateWordVariant wordVariant)
   {
      Variant = wordVariant;
      Inspector = new MatchInspector(this);
   }

   protected virtual List<MatchRequirement> DynamicDisplayRequirements
   {
      get
      {
         if(_displayRequirementsCache == null)
         {
            var requirements = new List<MatchRequirement?>
                               {
                                  new ForbidsIsShadowed(Inspector)
                               };
            requirements.AddRange(CreateDynamicDisplayRequirements());
            _displayRequirementsCache = requirements.Where(r => r != null).Cast<MatchRequirement>().ToList();
         }

         return _displayRequirementsCache;
      }
   }

   protected virtual List<FailedMatchRequirement> CreatePrimaryValidityFailures()
   {
      return MatchPrimaryValidityRequirements
            .Select(requirement => requirement(Inspector))
            .Where(failure => failure != null)
            .Cast<FailedMatchRequirement>()
            .ToList();
   }

   protected virtual bool IsPrimarilyValid() => NoneFail(MatchPrimaryValidityRequirements, Inspector);

   /// <summary>
   /// Whether none of <paramref name="requirements"/> fails for the match <paramref name="inspector"/> inspects. Allocates nothing: parsing checks<br/>
   /// requirements millions of times a pass, and a LINQ chain allocated a delegate and an iterator for each check (2026-11-12).
   /// </summary>
   protected static bool NoneFail<TInspector>(List<Func<TInspector, FailedMatchRequirement?>> requirements, TInspector inspector)
   {
      foreach(var requirement in requirements)
      {
         if(requirement(inspector) != null) return false;
      }

      return true;
   }

   protected virtual bool IsInterdependentlyValid() => true;

   protected virtual List<FailedMatchRequirement> CreateInterdependentValidityFailures() => [];

   protected virtual List<FailedMatchRequirement> CreateStaticDisplayRequirementFailures()
   {
      return MatchStaticDisplayRequirements
            .Select(requirement => requirement(Inspector))
            .Where(failure => failure != null)
            .Cast<FailedMatchRequirement>()
            .ToList();
   }

   public bool StaticDisplayRequirementsFulfilled
   {
      get
      {
         if(_staticDisplayRequirementsFulfilledCache == null)
         {
            _staticDisplayRequirementsFulfilledCache = StaticDisplayRequirementsFulfilledInternal();
         }

         return _staticDisplayRequirementsFulfilledCache.Value;
      }
   }

   protected virtual bool StaticDisplayRequirementsFulfilledInternal() => NoneFail(MatchStaticDisplayRequirements, Inspector);

   protected virtual IEnumerable<MatchRequirement?> CreateDynamicDisplayRequirements() => [];

   public abstract string Answer { get; }
   public abstract List<string> Readings { get; }
   public virtual string TokenizedForm => Variant.Form;
   public virtual string MatchForm => TokenizedForm;
   public virtual string ParsedForm => TokenizedForm;
   public virtual string ExclusionForm => Variant.Form;

   public CandidateWord Word => Variant.Word;

   public virtual bool IsValid
   {
      get
      {
         if(_isValidCache == null)
         {
            _isValidCache = IsValidInternal();
         }

         return _isValidCache.Value;
      }
   }

   bool IsValidInternal() => IsValidInternalProperty || IsHighlighted;

   public bool IsPrimarilyValidProperty
   {
      get
      {
         if(_isPrimarilyValidInternalCache == null)
         {
            _isPrimarilyValidInternalCache = IsPrimarilyValid();
         }

         return _isPrimarilyValidInternalCache.Value;
      }
   }

   bool IsValidInternalProperty
   {
      get
      {
         if(_isValidInternalCache == null)
         {
            _isValidInternalCache = IsPrimarilyValidProperty && IsInterdependentlyValid();
         }

         return _isValidInternalCache.Value;
      }
   }

   public bool IsHighlighted => Variant.Configuration.Highlights(this);
   public bool IsDisplayed => IsValidForDisplay || IsEmergencyDisplayed;

   public int StartIndex
   {
      get
      {
         if(_startIndexCache == null)
         {
            _startIndexCache = StartIndexInternal();
         }

         return _startIndexCache.Value;
      }
   }

   protected virtual int StartIndexInternal() => Variant.StartIndex;

   public bool IsValidForDisplay =>
      IsValid &&
      StaticDisplayRequirementsFulfilled &&
      DynamicDisplayRequirements.All(requirement => requirement.IsFulfilled);

   public bool IsEmergencyDisplayed =>
      !IsValid &&
      Variant.IsSurface &&
      SurfaceIsSeeminglyValidSingleToken &&
      !BaseIsValidWord &&
      !IsShadowed &&
      !HasValidForDisplaySibling;

   bool HasValidForDisplaySibling =>
      Variant.Matches.Any(otherMatch => otherMatch != this && otherMatch.IsValidForDisplay);

   bool BaseIsValidWord => Word.BaseVariant != null && Word.BaseVariant.HasValidMatch;
   bool SurfaceIsSeeminglyValidSingleToken => Word.HasSeeminglyValidSingleToken;
   public bool IsShadowed => Word.IsShadowed;

   public virtual List<string> FailureReasons =>
      !IsValid
         ? CreatePrimaryValidityFailures().Select(r => r.FailureReason).Concat(
            CreateInterdependentValidityFailures().Select(r => r.FailureReason)).ToList()
         : [];

   public List<string> HidingReasons =>
      DynamicDisplayRequirements
        .Where(requirement => !requirement.IsFulfilled)
        .Select(requirement => requirement.FailureReason)
        .Concat(CreateStaticDisplayRequirementFailures().Select(r => r.FailureReason))
        .ToList();

   public WordExclusion ToExclusion() => WordExclusion.AtIndex(ExclusionForm, StartIndex);

   public override string ToString() =>
      $"{ParsedForm}, {MatchForm[..Math.Min(10, MatchForm.Length)]}: " +
      $"failure_reasons: {string.Join(" ", FailureReasons)} " +
      $"## hiding_reasons: {string.Join(" ", HidingReasons)}";
}
