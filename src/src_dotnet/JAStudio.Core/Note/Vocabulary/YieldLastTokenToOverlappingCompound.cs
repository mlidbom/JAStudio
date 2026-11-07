using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Note.Vocabulary;

public class YieldLastTokenToOverlappingCompound : RequireForbidFlagField
{
   readonly VocabNotePartsOfSpeech _pos;

   public YieldLastTokenToOverlappingCompound(VocabNote vocab)
      : base(vocab,
             0,
             0,
             Tags.Vocab.Matching.YieldLastTokenToOverlappingCompound,
             Tags.Vocab.Matching.Forbids.AutoYielding) =>
      _pos = vocab.PartsOfSpeech;

   bool DecideIfRequired() =>
      base.IsRequired
   || (!IsForbidden && _pos.IsCompleteNaAdjective());

   public override bool IsRequired => DecideIfRequired();

   public override string ToString()
   {
      var parts = new System.Collections.Generic.List<string>();
      if(IsRequired) parts.Add("required");
      if(IsForbidden) parts.Add("forbidden");
      return string.Join(" ", parts);
   }
}
