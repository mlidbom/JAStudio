using System.Collections.Generic;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.Collection;

class SentenceCache : NoteCache<SentenceNote, SentenceSnapshot>
{
   readonly NoteIndex<string, SentenceNote, SentenceSnapshot> _byVocabForm;
   readonly NoteIndex<string, SentenceNote, SentenceSnapshot> _byHighlightedVocab;
   readonly NoteIndex<string, SentenceNote, SentenceSnapshot> _byVocabMarkedIncorrect;
   readonly NoteIndex<NoteId, SentenceNote, SentenceSnapshot> _byVocabId;

   public SentenceCache(NoteServices noteServices) : base(noteServices)
   {
      _byVocabForm = NewIndex(snapshot => snapshot.Words);
      _byHighlightedVocab = NewIndex(snapshot => snapshot.HighlightedVocab);
      _byVocabMarkedIncorrect = NewIndex(snapshot => snapshot.MarkedIncorrectVocab);
      _byVocabId = NewIndex(snapshot => snapshot.DetectedVocab);
   }

   protected override SentenceSnapshot CreateSnapshot(SentenceNote note) => new(note);

   public List<SentenceNote> WithVocab(VocabNote vocab) => _byVocabId.NotesWith(vocab.GetId());

   public List<SentenceNote> WithVocabForm(string form) => _byVocabForm.NotesWith(form);

   public List<SentenceNote> WithHighlightedVocab(string form) => _byHighlightedVocab.NotesWith(form);

   public List<SentenceNote> WithVocabMarkedIncorrect(string form) => _byVocabMarkedIncorrect.NotesWith(form);
}
