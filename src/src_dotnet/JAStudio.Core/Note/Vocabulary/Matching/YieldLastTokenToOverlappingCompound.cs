using System.Collections.Generic;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>
/// The require/forbid rule that a match to the vocab yields its last token to an overlapping compound that follows it.<br/>
/// A complete な-adjective requires it unless it's forbidden.
/// </summary>
public class YieldLastTokenToOverlappingCompound : RequireForbidFlagField
{
   readonly VocabNotePartsOfSpeech _partsOfSpeech;

   internal YieldLastTokenToOverlappingCompound(VocabNotePartsOfSpeech partsOfSpeech, IReadOnlyDictionary<string, RequiredOrForbiddenDto> storedRules, NoteGuard guard)
      : base("yieldLastTokenToOverlappingCompound", 0, 0, storedRules, guard) =>
      _partsOfSpeech = partsOfSpeech;

   /// <summary>Required as configured, or, unless it's forbidden, because the vocab is a complete な-adjective.</summary>
   public override bool IsRequired => base.IsRequired || (!IsForbidden && _partsOfSpeech.IsCompleteNaAdjective());

   public override string ToString()
   {
      var parts = new List<string>();
      if(IsRequired) parts.Add("required");
      if(IsForbidden) parts.Add("forbidden");
      return string.Join(" ", parts);
   }
}
