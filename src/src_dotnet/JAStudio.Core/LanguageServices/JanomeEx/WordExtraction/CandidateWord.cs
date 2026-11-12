using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

public sealed class CandidateWord
{
   public List<TextAnalysisLocation> Locations { get; }
   public string SurfaceForm { get; }
   public string BaseForm { get; }
   public CandidateWordVariant SurfaceVariant { get; }
   public CandidateWordVariant? BaseVariant { get; }

   /// <summary>Both of the word's variants: its <see cref="SurfaceVariant"/>, and its <see cref="BaseVariant"/> when its base form differs from its surface.</summary>
   public IEnumerable<CandidateWordVariant> Variants => BaseVariant == null ? [SurfaceVariant] : [SurfaceVariant, BaseVariant];

   public List<CandidateWordVariant> IndexingVariants { get; private set; }
   public List<CandidateWordVariant> ValidVariants { get; private set; }

   /// <summary>
   /// The variant the word is displayed as, if any: its <see cref="SurfaceVariant"/> when that has matches to display, or else its <see cref="BaseVariant"/><br/>
   /// when that has. Each display pass decides it anew (<see cref="RunDisplayAnalysisPassTrueIfThereWereChanges"/>).
   /// </summary>
   public CandidateWordVariant? DisplayVariant { get; private set; }

   /// <summary>Whether the word is displayed: whether it has a <see cref="DisplayVariant"/>.</summary>
   public bool IsDisplayed => DisplayVariant != null;

   public CandidateWord(List<TextAnalysisLocation> locations)
   {
      Locations = locations;

      SurfaceForm = string.Join("", locations.Select(l => l.Token.Surface));
      BaseForm = string.Join("", locations.Take(locations.Count - 1).Select(l => l.Token.Surface)) +
                 locations[^1].Token.BaseForm;

      SurfaceVariant = new CandidateWordVariant(this, SurfaceForm);
      BaseVariant = BaseForm != SurfaceForm ? new CandidateWordVariant(this, BaseForm) : null;

      IndexingVariants = [];
      ValidVariants = [];
   }

   public bool HasBaseVariantWithValidMatch => BaseVariant != null && BaseVariant.HasValidMatch;

   public bool ShouldIncludeSurfaceInAllWords =>
      SurfaceVariant.HasValidMatch ||
      (!HasBaseVariantWithValidMatch && HasSeeminglyValidSingleToken);

   public bool ShouldIndexBase => BaseVariant != null && (BaseVariant.HasValidMatch || BaseVariant.IsKnownWord);
   public bool ShouldIndexSurface => SurfaceVariant.IsKnownWord || ShouldIncludeSurfaceInAllWords;

   public void RunValidityAnalysis()
   {
      if(BaseVariant != null) BaseVariant.RunValidityAnalysis();
      SurfaceVariant.RunValidityAnalysis();

      ValidVariants = [];
      if(HasBaseVariantWithValidMatch)
      {
         ValidVariants.Add(BaseVariant!);
      }

      if(SurfaceVariant.HasValidMatch)
      {
         ValidVariants.Add(SurfaceVariant);
      }

      if(ShouldIndexSurface)
      {
         IndexingVariants.Add(SurfaceVariant);
      }

      if(ShouldIndexBase)
      {
         IndexingVariants.Add(BaseVariant!);
      }
   }

   public bool RunDisplayAnalysisPassTrueIfThereWereChanges()
   {
      if(LocationCount > 1 && !ValidVariants.Any()) return false;

      if(BaseVariant != null) BaseVariant.RunVisibilityAnalysis();
      SurfaceVariant.RunVisibilityAnalysis();

      var displayVariantBefore = DisplayVariant;
      DisplayVariant = TheVariantToDisplay();
      return DisplayVariant != displayVariantBefore;

      CandidateWordVariant? TheVariantToDisplay() =>
         SurfaceVariant.DisplayMatches.Any() ? SurfaceVariant
         : BaseVariant != null && BaseVariant.DisplayMatches.Any() ? BaseVariant
         : null;
   }

   public bool HasValidWords() => ValidVariants.Count > 0;

   public bool HasSeeminglyValidSingleToken => !IsCompound && !StartsWithNonWordCharacter;
   public TextAnalysis Analysis => Locations[0].Analysis;
   public bool IsCompound => LocationCount > 1;
   public TextAnalysisLocation StartLocation => Locations[0];
   public TextAnalysisLocation EndLocation => Locations[^1];
   public int LocationCount => Locations.Count;
   public bool StartsWithNonWordToken => StartLocation.Token.IsNonWordCharacter;
   public bool IsWord => SurfaceVariant.IsKnownWord || (BaseVariant != null && BaseVariant.IsKnownWord);
   public bool IsInflectableWord => EndLocation.Token.IsInflectableWord;
   public bool NextTokenIsInflectingWord => EndLocation.IsNextLocationInflectingWord();
   public bool IsInflectedWord => IsInflectableWord && NextTokenIsInflectingWord;
   public bool StartsWithNonWordCharacter => StartsWithNonWordToken || (SurfaceForm != "" && AnalysisConstants.NoiseCharacters.Contains(SurfaceForm[0]));

   /// <summary>
   /// The sentence's text before this word, from the sentence's start, as its tokens have it: without a <c>&lt;wbr&gt;</c> word-break hint, and with<br/>
   /// the characters that confuse the tokenizer, such as <c>!</c>, as full-width spaces (<see cref="JNTokenizer"/>).
   /// </summary>
   public string PrecedingText => string.Concat(Analysis.Locations.Take(StartLocation.TokenIndex).Select(it => it.Token.Surface));

   /// <summary>The sentence's text after this word, to the sentence's end, as its tokens have it, as <see cref="PrecedingText"/> is.</summary>
   public string FollowingText => string.Concat(Analysis.Locations.Skip(EndLocation.TokenIndex + 1).Select(it => it.Token.Surface));

   public bool IsShadowed => ShadowedBy != "";

   public string ShadowedBy
   {
      get
      {
         var startLocation = StartLocation;
         if(startLocation.IsShadowedBy.Any())
         {
            var shadowedBy = startLocation.IsShadowedBy[0].DisplayVariant ?? throw new InvalidOperationException("A location shadows others only while it displays a variant.");
            // special exception to allow dictionary form endings to be displayed in spite of starting at the same token as the end of the compound including the dictionary ending. This logic is mirrored in ForbidsHasDisplayedOverlappingFollowingCompound
            if(!(startLocation.Token.IsDictionaryVerbInflection && shadowedBy.Word.EndLocation.TokenIndex == startLocation.TokenIndex))
            {
               return shadowedBy.Form;
            }
         }

         if(startLocation.DisplayVariant is {} displayedAtTheStart && displayedAtTheStart.Word.LocationCount > LocationCount)
         {
            return displayedAtTheStart.Form;
         }

         return "";
      }
   }

   public override string ToString() =>
      $"surface: {SurfaceVariant}, base:{BaseVariant}, " +
      $"hdc:{HasValidWords()}, " +
      $"iw:{IsWord} " +
      $"icc:{IsCompound})";
}
