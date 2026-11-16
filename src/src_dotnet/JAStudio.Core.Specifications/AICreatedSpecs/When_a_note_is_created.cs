using System.Linq;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Storage;
using JAStudio.Core.Storage.Dto;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A note JAStudio creates is JAStudio's own: it's added to the collection and saved to the corpus, and gets no study-note in Anki, whether JAStudio<br/>
/// runs inside Anki or not (Magnus, 2026-11-13). Anki holds only the notes being studied.
/// </summary>
public class When_a_note_is_created : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly ITaggregateReference<JPNote> _sentenceReference;
   JPNote _sentence => _sentenceReference.Current;

   public When_a_note_is_created() => _sentenceReference = CreateSentence("家に帰る").Reference();

   [XF] public void it_gets_no_study_note_in_Anki() => _sentence.HasAStudyNote().Must().BeFalse();
   [XF] public void it_is_in_the_collection() => GetService<JPCollection>().NoteFromNoteId(_sentence.GetId()).Must().Be(_sentence);
   [XF] public void it_is_saved_to_the_corpus() => GetService<NotesStoredInMemory>().Snapshots<SentenceDto>().Select(it => it.Id).Must().Contain(_sentence.GetId().Value);
}
