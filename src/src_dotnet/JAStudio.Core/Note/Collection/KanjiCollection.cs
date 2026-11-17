using System.Collections.Generic;
using System.Linq;
using Compze.DependencyInjection.Abstractions;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.Collection;

public class KanjiCollection
{
   readonly StoredNotes<KanjiNote> _notes;
   readonly NoteUnitsOfWork _unitsOfWork;
   readonly NoteIndex<KanjiNote, KanjiIndexes.Question> _byQuestion;
   readonly NoteIndex<KanjiNote, KanjiIndexes.Radical> _byRadical;
   readonly NoteIndex<KanjiNote, KanjiIndexes.Reading> _byReading;

   internal KanjiCollection(NoteServices noteServices, IRootResolver resolver, NoteUnitsOfWork unitsOfWork)
   {
      _notes = new StoredNotes<KanjiNote>(noteServices, resolver);
      _unitsOfWork = unitsOfWork;
      _byQuestion = _notes.IndexBy<KanjiIndexes.Question>();
      _byRadical = _notes.IndexBy<KanjiIndexes.Radical>();
      _byReading = _notes.IndexBy<KanjiIndexes.Reading>();
   }

   public List<KanjiNote> All() => _notes.All();

   public KanjiNote? WithIdOrNone(NoteId noteId) => _notes.WithIdOrNone(noteId);
   public KanjiNote? WithTheStudyNoteOrNone(long studyNoteId) => _notes.WithTheStudyNoteOrNone(studyNoteId);

   public List<KanjiNote> WithAnyKanjiIn(List<string> kanjiList)
   {
      return kanjiList.SelectMany(kanji => _byQuestion.NotesWith(new(kanji))).ToList();
   }

   public KanjiNote? WithKanji(string kanji)
   {
      var results = _byQuestion.NotesWith(new(kanji));
      return results.Count == 1 ? results[0] : null;
   }

   public List<KanjiNote> WithRadical(string radical) => _byRadical.NotesWith(new(radical));

   /// <summary>Renames the word-reference <paramref name="oldName"/> to <paramref name="newName"/> in every kanji's primary vocab, after the vocab that had the one name got the other.</summary>
   /// <remarks>Gets for update only the kanji that have <paramref name="oldName"/> as a primary vocab, rather than copying and locking every kanji.</remarks>
   internal void RenameThePrimaryVocab(WordReference oldName, WordReference newName)
   {
      foreach(var kanji in All().Where(it => it.PrimaryVocab.Contains(oldName)))
         kanji.Edit(it => it.PrimaryVocab.Rename(oldName, newName));
   }

   public HashSet<KanjiNote> WithReading(string reading)
   {
      var hiraganaReading = KanaUtils.AnythingToHiragana(reading);
      return _byReading.NotesWith(new(hiraganaReading)).ToHashSet();
   }

   /// <inheritdoc cref="SentenceCollection.Add"/>
   public void Add(KanjiNote note)
   {
      _unitsOfWork.Run(it => it.Add(note));
   }
}
