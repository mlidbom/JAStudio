using System.Collections.Generic;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.Collection;

class VocabCache : NoteCache<VocabNote, VocabSnapshot>
{
   readonly NoteIndex<WordReference, VocabNote, VocabSnapshot> _byReferenceNamingIt;
   readonly NoteIndex<string, VocabNote, VocabSnapshot> _byForm;
   readonly NoteIndex<string, VocabNote, VocabSnapshot> _byFormPrefix;
   readonly NoteIndex<string, VocabNote, VocabSnapshot> _byKanjiInMainForm;
   readonly NoteIndex<string, VocabNote, VocabSnapshot> _byKanjiInAnyForm;
   readonly NoteIndex<WordReference, VocabNote, VocabSnapshot> _byCompoundPart;
   readonly NoteIndex<WordReference, VocabNote, VocabSnapshot> _byDerivedFrom;
   readonly NoteIndex<string, VocabNote, VocabSnapshot> _byReading;
   readonly NoteIndex<string, VocabNote, VocabSnapshot> _byStem;

   public VocabCache(NoteServices noteServices) : base(noteServices)
   {
      _byForm = NewIndex(snapshot => snapshot.Forms);
      _byFormPrefix = NewIndex(snapshot => snapshot.FormPrefixes);
      _byCompoundPart = NewIndex(snapshot => snapshot.CompoundParts);
      _byDerivedFrom = NewIndex(snapshot => snapshot.DerivedFrom);
      _byReferenceNamingIt = NewIndex(snapshot => snapshot.ReferenceNamingIt);
      _byKanjiInMainForm = NewIndex(snapshot => snapshot.MainFormKanji);
      _byKanjiInAnyForm = NewIndex(snapshot => snapshot.AllKanji);
      _byReading = NewIndex(snapshot => snapshot.Readings);
      _byStem = NewIndex(snapshot => snapshot.Stems);
   }

   public List<VocabNote> WithForm(string form) => _byForm.NotesWith(form);

   /// <summary>Whether any vocab has a form that starts with <paramref name="text"/>, or is it.</summary>
   public bool HasAFormStartingWith(string text) => _byFormPrefix.HasNotesWith(text);

   /// <summary>The vocab whose question <paramref name="reference"/> names exactly (<see cref="VocabNoteQuestion.Reference"/>).</summary>
   public List<VocabNote> NamedExactlyBy(WordReference reference) => _byReferenceNamingIt.NotesWith(reference);

   /// <summary>The vocab with exactly <paramref name="compoundPart"/> among their compound parts.</summary>
   public List<VocabNote> WithCompoundPart(WordReference compoundPart) => _byCompoundPart.NotesWith(compoundPart);

   /// <summary>The vocab whose derived-from word is exactly <paramref name="word"/>.</summary>
   public List<VocabNote> DerivedFrom(WordReference word) => _byDerivedFrom.NotesWith(word);

   public List<VocabNote> WithKanjiInMainForm(string kanji) => _byKanjiInMainForm.NotesWith(kanji);

   public List<VocabNote> WithKanjiInAnyForm(string kanji) => _byKanjiInAnyForm.NotesWith(kanji);

   public List<VocabNote> WithReading(string reading) => _byReading.NotesWith(reading);

   public List<VocabNote> WithStem(string stem) => _byStem.NotesWith(stem);

   protected override VocabSnapshot CreateSnapshot(VocabNote note) => new(note);
}
