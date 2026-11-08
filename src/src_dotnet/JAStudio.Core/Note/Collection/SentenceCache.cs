using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.SysUtils.Collections.Generic;

namespace JAStudio.Core.Note.Collection;

class SentenceCache : NoteCache<SentenceNote, SentenceSnapshot>
{
   readonly Dictionary<string, HashSet<SentenceNote>> _byVocabForm = new();
   readonly Dictionary<string, HashSet<SentenceNote>> _byHighlightedVocab = new();
   readonly Dictionary<string, HashSet<SentenceNote>> _byUserMarkedInvalidVocab = new();
   readonly Dictionary<NoteId, HashSet<SentenceNote>> _byVocabId = new();

   public SentenceCache(NoteServices noteServices) : base(noteServices) {}

   protected override void ClearDerivedIndexes()
   {
      _byVocabForm.Clear();
      _byHighlightedVocab.Clear();
      _byUserMarkedInvalidVocab.Clear();
      _byVocabId.Clear();
   }


   protected override SentenceSnapshot CreateSnapshot(SentenceNote note) => new(note);

   public List<SentenceNote> WithVocab(VocabNote vocab) =>
      _monitor.Locked(() => _byVocabId.TryGetValue(vocab.GetId(), out var notes) ? notes.ToList() : []);

   public List<SentenceNote> WithVocabForm(string form) =>
      _monitor.Locked(() => _byVocabForm.TryGetValue(form, out var notes) ? notes.ToList() : []);

   public List<SentenceNote> WithHighlightedVocab(string form) =>
      _monitor.Locked(() => _byHighlightedVocab.TryGetValue(form, out var notes) ? notes.ToList() : []);

   public List<SentenceNote> WithUserMarkedInvalidVocab(string form) =>
      _monitor.Locked(() => _byUserMarkedInvalidVocab.TryGetValue(form, out var notes) ? notes.ToList() : []);

   protected override void InheritorRemoveFromCache(SentenceNote note, SentenceSnapshot snapshot)
   {
      _byVocabForm.RemoveFromSets(snapshot.Words, note);
      _byHighlightedVocab.RemoveFromSets(snapshot.HighlightedVocab, note);
      _byUserMarkedInvalidVocab.RemoveFromSets(snapshot.MarkedIncorrectVocab, note);
      _byVocabId.RemoveFromSets(snapshot.DetectedVocab, note);
   }

   protected override void InheritorAddToCache(SentenceNote note, SentenceSnapshot snapshot)
   {
      _byVocabForm.AddToSets(snapshot.Words, note);
      _byHighlightedVocab.AddToSets(snapshot.HighlightedVocab, note);
      _byUserMarkedInvalidVocab.AddToSets(snapshot.MarkedIncorrectVocab, note);
      _byVocabId.AddToSets(snapshot.DetectedVocab, note);
   }
}
