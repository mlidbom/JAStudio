using System.Collections.Generic;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.SysUtils.Collections.Generic;

namespace JAStudio.Core.Note.Collection;

class VocabCache : NoteCache<VocabNote, VocabSnapshot>
{
   readonly Dictionary<WordReference, HashSet<VocabNote>> _byReferenceNamingIt = new();
   readonly Dictionary<string, HashSet<VocabNote>> _byForm = new();
   readonly Dictionary<string, HashSet<VocabNote>> _byKanjiInMainForm = new();
   readonly Dictionary<string, HashSet<VocabNote>> _byKanjiInAnyForm = new();
   readonly Dictionary<WordReference, HashSet<VocabNote>> _byCompoundPart = new();
   readonly Dictionary<WordReference, HashSet<VocabNote>> _byDerivedFrom = new();
   readonly Dictionary<string, HashSet<VocabNote>> _byReading = new();
   readonly Dictionary<string, HashSet<VocabNote>> _byStem = new();

   public VocabCache(NoteServices noteServices) : base(noteServices) {}

   protected override void ClearDerivedIndexes()
   {
      _byReferenceNamingIt.Clear();
      _byForm.Clear();
      _byKanjiInMainForm.Clear();
      _byKanjiInAnyForm.Clear();
      _byCompoundPart.Clear();
      _byDerivedFrom.Clear();
      _byReading.Clear();
      _byStem.Clear();
   }

   public List<VocabNote> WithForm(string form) => LookUp(_byForm, form);

   /// <summary>The vocab whose question <paramref name="reference"/> names exactly (<see cref="VocabNoteQuestion.Reference"/>).</summary>
   public List<VocabNote> NamedExactlyBy(WordReference reference) => LookUp(_byReferenceNamingIt, reference);

   /// <summary>The vocab with exactly <paramref name="compoundPart"/> among their compound parts.</summary>
   public List<VocabNote> WithCompoundPart(WordReference compoundPart) => LookUp(_byCompoundPart, compoundPart);

   /// <summary>The vocab whose derived-from word is exactly <paramref name="word"/>.</summary>
   public List<VocabNote> DerivedFrom(WordReference word) => LookUp(_byDerivedFrom, word);

   public List<VocabNote> WithKanjiInMainForm(string kanji) => LookUp(_byKanjiInMainForm, kanji);

   public List<VocabNote> WithKanjiInAnyForm(string kanji) => LookUp(_byKanjiInAnyForm, kanji);

   public List<VocabNote> WithReading(string reading) => LookUp(_byReading, reading);

   public List<VocabNote> WithStem(string stem) => LookUp(_byStem, stem);

   protected override VocabSnapshot CreateSnapshot(VocabNote note) => new(note);

   protected override void InheritorRemoveFromCache(VocabNote note, VocabSnapshot snapshot)
   {
      _byForm.RemoveFromSets(snapshot.Forms, note);
      _byCompoundPart.RemoveFromSets(snapshot.CompoundParts, note);

      _byDerivedFrom.RemoveFromSets(snapshot.DerivedFrom, note);
      _byReferenceNamingIt.RemoveFromSets(snapshot.ReferenceNamingIt, note);

      _byKanjiInMainForm.RemoveFromSets(snapshot.MainFormKanji, note);
      _byKanjiInAnyForm.RemoveFromSets(snapshot.AllKanji, note);
      _byReading.RemoveFromSets(snapshot.Readings, note);
      _byStem.RemoveFromSets(snapshot.Stems, note);
   }

   protected override void InheritorAddToCache(VocabNote note, VocabSnapshot snapshot)
   {
      _byForm.AddToSets(snapshot.Forms, note);
      _byCompoundPart.AddToSets(snapshot.CompoundParts, note);

      _byDerivedFrom.AddToSets(snapshot.DerivedFrom, note);
      _byReferenceNamingIt.AddToSets(snapshot.ReferenceNamingIt, note);

      _byKanjiInMainForm.AddToSets(snapshot.MainFormKanji, note);
      _byKanjiInAnyForm.AddToSets(snapshot.AllKanji, note);
      _byReading.AddToSets(snapshot.Readings, note);
      _byStem.AddToSets(snapshot.Stems, note);
   }
}
