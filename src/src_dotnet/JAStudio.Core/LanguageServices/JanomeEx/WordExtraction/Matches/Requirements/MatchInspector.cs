using System.Linq;
using JAStudio.Core.Configuration;
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
   public Settings Settings => Word.Analysis.Services.Settings;
   public CandidateWordVariant SurfaceVariant => Word.SurfaceVariant;
   public bool IsBase => !Variant.IsSurface;
   public TextAnalysisLocation StartLocation => Word.StartLocation;
   public bool StartLocationIsDictionaryVerbInflection => StartLocation.Token.IsDictionaryVerbInflection;
   public TextAnalysisLocation EndLocation => Word.EndLocation;
   public SentenceConfiguration Configuration => Variant.Configuration;
   public TextAnalysisLocation? PreviousLocation => StartLocation.Previous;
   public bool HasTeFormStem => StartLocation.Token.HasTeFormStem;
   public bool HasTeFormStemHead => PreviousLocation != null && PreviousLocation.Token.HasTeFormStem;
   public string Prefix => PreviousLocation?.Token.Surface ?? "";
   public TextAnalysisLocation? NextLocation => Word.EndLocation.Next;
   public string Suffix => NextLocation?.Token.Surface ?? "";
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

   public bool HasMasuStem =>
      StartLocation.Previous != null &&
      StartLocation.Previous.Token.IsMasuStem;

   public bool HasAdverbHead =>
      StartLocation.Previous != null &&
      StartLocation.Previous.Token.IsAdverb;

   public bool IsEndOfStatement => EndLocation.Token.IsEndOfStatement;

   public bool IsCompoundEndingOnDictionaryFormWhereSurfaceDiffersFromBase =>
      Word.IsCompound &&
      IsBase &&
      EndLocation.Token.IsDictionaryVerbInflection &&
      EndLocation.Token.Surface != EndLocation.Token.BaseForm;
}
