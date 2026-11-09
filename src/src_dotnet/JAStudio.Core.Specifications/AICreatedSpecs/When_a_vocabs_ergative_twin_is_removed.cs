using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>The vocab 開く and 開ける, each the other's ergative twin, and then 開く's ergative twin removed.</summary>
public class When_a_vocabs_ergative_twin_is_removed : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabNote _opens;
   readonly VocabNote _toOpen;

   public When_a_vocabs_ergative_twin_is_removed()
   {
      _opens = VocabNote.Create(NoteServices, "開く", "to open", "あく");
      _toOpen = VocabNote.Create(NoteServices, "開ける", "to open something", "あける");
      _opens.RelatedNotes.ErgativeTwin.Set("開ける");
      _opens.RelatedNotes.ErgativeTwin.Remove();
   }

   [XF] public void the_vocab_has_no_ergative_twin() => _opens.RelatedNotes.ErgativeTwin.Get().Must().Be("");
   [XF] public void its_twin_no_longer_has_the_vocab_as_its_ergative_twin() => _toOpen.RelatedNotes.ErgativeTwin.Get().Must().Be("");
}
