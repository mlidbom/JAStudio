using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.Requirements;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.StateTests.Tail;

public sealed class ForbidsHasDisplayedOverlappingFollowingCompound : CustomForbidsNoCache
{
   ForbidsHasDisplayedOverlappingFollowingCompound(VocabMatchInspector inspector) : base(inspector) {}

   public static ForbidsHasDisplayedOverlappingFollowingCompound? ApplyTo(VocabMatchInspector inspector)
   {
      if(inspector.Match.IsHighlighted) return null;

      if(ShouldYield(inspector))
         return new ForbidsHasDisplayedOverlappingFollowingCompound(inspector);

      return null;
   }

   static bool ShouldYield(VocabMatchInspector inspector)
   {
      var yieldLastToken = inspector.RequiresForbids.YieldLastToken;
      if(yieldLastToken.IsRequired) return true;

      if(inspector.BreakdownParsingOptions is not {} options || yieldLastToken.IsForbidden)
         return false;

      var pos = inspector.Match.Vocab.PartsOfSpeech;

      if(options.AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound && pos.IsSuruVerbIncluded() && !pos.IsNiSuruGaSuruKuSuruCompound())
         return true;
      if(options.AutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound && pos.IsPassiveVerbCompound())
         return true;
      if(options.AutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound && pos.IsCausativeVerbCompound())
         return true;

      return false;
   }

   public override string Description => "has_displayed_following_overlapping_compound";

   protected override bool InternalIsInState()
   {
      // todo: this is a problematic reference to display_words. That collection is initialized using this class,
      // so this class will return different results depending on whether it is used after or before display_words is first initialized. Ouch

      var endLocation = Inspector.EndLocation;
      var tailLocation = endLocation;
      // The dictionary verb inflection is a special case that is shown as a separate token after the word in which it is part, so it does not by itself cover/shadow anything
      // thus, we skip such tokens as possible locations for following compounds to overlap, since that words starting with that token are shown after this compound anyway, and are not hidden by this compound.
      // this logic is mirrored in is_shadowed logic
      if(endLocation.Token.IsDictionaryVerbInflection)
      {
         if(endLocation.Previous == null)
         {
            return false;
         }

         tailLocation = endLocation.Previous;
      }

      while(tailLocation != Inspector.Word.StartLocation)
      {
         foreach(var displayWord in tailLocation.DisplayWords)
         {
            if(displayWord.EndLocation.TokenIndex > endLocation.TokenIndex)
            {
               return true;
            }
         }

         tailLocation = tailLocation.Previous!;
      }

      return false;
   }
}
