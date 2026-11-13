using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Storage;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A note JAStudio creates is JAStudio's own: it's added to the collection and saved to the corpus, and gets no study-note in Anki, whether JAStudio<br/>
/// runs inside Anki or not (Magnus, 2026-11-13). Anki holds only the notes being studied.
/// </summary>
public class When_a_note_is_created : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly JPNote _sentence;

   public When_a_note_is_created() => _sentence = CreateSentence("家に帰る");

   [XF] public void it_gets_no_study_note_in_Anki() => _sentence.HasAnAnkiNote().Must().BeFalse();
   [XF] public void it_is_in_the_collection() => GetService<JPCollection>().NoteFromNoteId(_sentence.GetId()).Must().Be(_sentence);
   [XF] public void it_is_saved_to_the_corpus() => GetService<INoteRepository>().LoadAll().Sentences.Must().Satisfy(it => it.Exists(saved => saved.GetId() == _sentence.GetId()));
}
