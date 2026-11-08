using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.SysUtils.Collections.Generic;

namespace JAStudio.Core.Note.Collection;

class VocabCache : NoteCache<VocabNote, VocabSnapshot>
{
   readonly Dictionary<string, HashSet<VocabNote>> _byDisambiguationName = new();
   readonly Dictionary<string, HashSet<VocabNote>> _byForm = new();
   readonly Dictionary<string, HashSet<VocabNote>> _byKanjiInMainForm = new();
   readonly Dictionary<string, HashSet<VocabNote>> _byKanjiInAnyForm = new();
   readonly Dictionary<string, HashSet<VocabNote>> _byCompoundPart = new();
   readonly Dictionary<string, HashSet<VocabNote>> _byDerivedFrom = new();
   readonly Dictionary<string, HashSet<VocabNote>> _byReading = new();
   readonly Dictionary<string, HashSet<VocabNote>> _byStem = new();

   public VocabCache(NoteServices noteServices) : base(noteServices) {}

   protected override void ClearDerivedIndexes()
   {
      _byDisambiguationName.Clear();
      _byForm.Clear();
      _byKanjiInMainForm.Clear();
      _byKanjiInAnyForm.Clear();
      _byCompoundPart.Clear();
      _byDerivedFrom.Clear();
      _byReading.Clear();
      _byStem.Clear();
   }


   public List<VocabNote> WithForm(string form) =>
      _monitor.Locked(() => _byForm.TryGetValue(form, out var notes) ? notes.ToList() : []);

   public List<VocabNote> WithDisambiguationName(string form) =>
      _monitor.Locked(() => _byDisambiguationName.TryGetValue(form, out var notes) ? notes.ToList() : []);

   public List<VocabNote> WithCompoundPart(string compoundPart) =>
      _monitor.Locked(() => _byCompoundPart.TryGetValue(compoundPart, out var notes) ? notes.ToList() : []);

   public List<VocabNote> DerivedFrom(string form) =>
      _monitor.Locked(() => _byDerivedFrom.TryGetValue(form, out var notes) ? notes.ToList() : []);

   public List<VocabNote> WithKanjiInMainForm(string kanji) =>
      _monitor.Locked(() => _byKanjiInMainForm.TryGetValue(kanji, out var notes) ? notes.ToList() : []);

   public List<VocabNote> WithKanjiInAnyForm(string kanji) =>
      _monitor.Locked(() => _byKanjiInAnyForm.TryGetValue(kanji, out var notes) ? notes.ToList() : []);

   public List<VocabNote> WithReading(string reading) =>
      _monitor.Locked(() => _byReading.TryGetValue(reading, out var notes) ? notes.ToList() : []);

   public List<VocabNote> WithStem(string stem) =>
      _monitor.Locked(() => _byStem.TryGetValue(stem, out var notes) ? notes.ToList() : []);

   protected override VocabSnapshot CreateSnapshot(VocabNote note) => new(note);

   protected override void InheritorRemoveFromCache(VocabNote note, VocabSnapshot snapshot)
   {
      _byForm.RemoveFromSets(snapshot.Forms, note);
      _byCompoundPart.RemoveFromSets(snapshot.CompoundParts, note);

      _byDerivedFrom.RemoveFromSet(snapshot.DerivedFrom, note);
      _byDisambiguationName.RemoveFromSet(snapshot.DisambiguationName, note);

      _byKanjiInMainForm.RemoveFromSets(snapshot.MainFormKanji, note);
      _byKanjiInAnyForm.RemoveFromSets(snapshot.AllKanji, note);
      _byReading.RemoveFromSets(snapshot.Readings, note);
      _byStem.RemoveFromSets(snapshot.Stems, note);
   }

   protected override void InheritorAddToCache(VocabNote note, VocabSnapshot snapshot)
   {
      _byForm.AddToSets(snapshot.Forms, note);
      _byCompoundPart.AddToSets(snapshot.CompoundParts, note);

      _byDerivedFrom.AddToSet(snapshot.DerivedFrom, note);
      _byDisambiguationName.AddToSet(snapshot.DisambiguationName, note);

      _byKanjiInMainForm.AddToSets(snapshot.MainFormKanji, note);
      _byKanjiInAnyForm.AddToSets(snapshot.AllKanji, note);
      _byReading.AddToSets(snapshot.Readings, note);
      _byStem.AddToSets(snapshot.Stems, note);
   }
}
