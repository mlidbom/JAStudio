using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

public sealed class CandidateWord
{
   public List<TextAnalysisLocation> Locations { get; }
   public string SurfaceForm { get; }
   public string BaseForm { get; }
   public CandidateWordVariant SurfaceVariant { get; }
   public CandidateWordVariant? BaseVariant { get; }

   public List<CandidateWordVariant> IndexingVariants { get; private set; }
   public List<CandidateWordVariant> ValidVariants { get; private set; }
   public List<CandidateWordVariant> DisplayVariants { get; private set; }

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
      DisplayVariants = [];
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

      var oldDisplayWordVariants = DisplayVariants;
      DisplayVariants = [];

      if(SurfaceVariant.DisplayMatches.Any())
      {
         DisplayVariants.Add(SurfaceVariant);
      } else if(BaseVariant != null && BaseVariant.DisplayMatches.Any())
      {
         DisplayVariants.Add(BaseVariant);
      }

      bool DisplayWordsWereChanged()
      {
         if(oldDisplayWordVariants.Count != DisplayVariants.Count)
            return true;

         return oldDisplayWordVariants
               .Where((t, index) => t != DisplayVariants[index])
               .Any();
      }

      return DisplayWordsWereChanged();
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
   public bool StartsWithNonWordCharacter => StartsWithNonWordToken || (SurfaceForm != "" && AnalysisConstants.NoiseCharacters.Contains(SurfaceForm[0].ToString()));

   public bool IsShadowed => ShadowedBy != "";

   public string ShadowedBy
   {
      get
      {
         var startLocation = StartLocation;
         if(startLocation.IsShadowedBy.Any())
         {
            var shadowedBy = startLocation.IsShadowedBy[0].DisplayVariants[0];
            // special exception to allow dictionary form endings to be displayed in spite of starting at the same token as the end of the compound including the dictionary ending. This logic is mirrored in ForbidsHasDisplayedOverlappingFollowingCompound
            if(!(startLocation.Token.IsDictionaryVerbInflection && shadowedBy.Word.EndLocation.TokenIndex == startLocation.TokenIndex))
            {
               return shadowedBy.Form;
            }
         }

         if(startLocation.DisplayVariants.Any() &&
            startLocation.DisplayVariants[0].Word.LocationCount > LocationCount)
         {
            return startLocation.DisplayVariants[0].Form;
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
