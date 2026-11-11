using System.Linq;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;

/// <summary>
/// Base class providing access to Match context and helper properties.
/// This class holds a reference to a Match and provides convenient
/// properties for inspecting the match's word, variant, location, and surrounding context.
/// </summary>
public class MatchInspector
{
   internal MatchInspector(Match match) => Match = match;

   public Match Match { get; }

   public CandidateWordVariant Variant => Match.Variant;
   public CandidateWord Word => Variant.Word;
   /// <summary>The options of the sentence breakdown the analysis is made for; null when it's made as a stored parsing result is.</summary>
   public BreakdownParsingOptions? BreakdownParsingOptions => Word.Analysis.BreakdownParsingOptions;
   public CandidateWordVariant SurfaceVariant => Word.SurfaceVariant;
   public bool IsBase => !Variant.IsSurface;
   public TextAnalysisLocation StartLocation => Word.StartLocation;
   public bool StartLocationIsDictionaryVerbInflection => StartLocation.Token.IsDictionaryVerbInflection;
   public TextAnalysisLocation EndLocation => Word.EndLocation;
   public SentenceConfiguration Configuration => Variant.Configuration;
   public TextAnalysisLocation? PreviousLocation => StartLocation.Previous;
   public string Prefix => PreviousLocation?.Token.Surface ?? "";
   public bool HasGodanImperativePart => StartLocation.Token.IsGodanImperativeInflection || StartLocation.Token.IsGodanImperativeStem;
   public bool HasGodanPotentialStart => StartLocation.Token.IsGodanPotentialInflection || StartLocation.Token.IsGodanPotentialStem;

   public bool IsVerbDictionaryFormCompound =>
      Word.LocationCount == 2 &&
      StartLocation.Token.IsDictionaryVerbFormStem &&
      EndLocation.Token.IsDictionaryVerbInflection;

   public bool IsIchidanCoveringGodanPotential =>
      Word.LocationCount == 2 &&
      HasGodanPotentialStart &&
      HasGodanPotentialEnding;

   public bool HasGodanPotentialEnding => Word.EndLocation.Token.IsGodanPotentialInflection;

   public bool CompoundLocationsAllHaveValidNonCompoundMatches =>
      Word.Locations.All(location => location.NonCompoundCandidate.HasValidWords());

   public bool IsEndOfStatement => EndLocation.Token.IsEndOfStatement;

   public bool IsCompoundEndingOnDictionaryFormWhereSurfaceDiffersFromBase =>
      Word.IsCompound &&
      IsBase &&
      EndLocation.Token.IsDictionaryVerbInflection &&
      EndLocation.Token.Surface != EndLocation.Token.BaseForm;
}
