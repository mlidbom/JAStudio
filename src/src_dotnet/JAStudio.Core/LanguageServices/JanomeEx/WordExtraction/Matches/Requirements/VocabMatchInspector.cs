using JAStudio.Core.Note.Vocabulary.Matching;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.Requirements;

/// <summary>
/// Base class providing access to VocabMatch context and helper properties.
/// This class holds a reference to a VocabMatch and provides convenient
/// properties for inspecting the match's word, variant, location, and surrounding context.
/// </summary>
public class VocabMatchInspector : MatchInspector
{
   public VocabMatchInspector(VocabMatch match) : base(match) {}

   public new VocabMatch Match => (VocabMatch)base.Match;

   public VocabMatchingRequireForbidFlags RequiresForbids => Match.RequiresForbids;

   public bool PreviousLocationIsIrrealis =>
      PreviousLocation is { Token.IsIrrealis: true };

   public bool PreviousLocationIsGodan =>
      PreviousLocation is { Token.IsGodanVerb: true };

   public bool PreviousLocationIsIchidan =>
      PreviousLocation is { Token.IsIchidanVerb: true };

   public bool BaseEqualsSurface => Word.SurfaceForm == Word.BaseForm;
}
