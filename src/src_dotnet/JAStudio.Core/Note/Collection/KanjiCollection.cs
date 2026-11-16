using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.Collection;

public class KanjiCollection
{
   internal readonly KanjiCache Cache;

   public KanjiCollection(NoteServices noteServices) => Cache = new KanjiCache(noteServices);

   public List<KanjiNote> All() => Cache.All();

   public KanjiNote? WithIdOrNone(NoteId noteId) => Cache.WithIdOrNone(noteId);
   public KanjiNote? WithTheStudyNoteOrNone(long studyNoteId) => Cache.WithTheStudyNoteOrNone(studyNoteId);

   public List<KanjiNote> WithAnyKanjiIn(List<string> kanjiList)
   {
      return kanjiList.SelectMany(k => Cache.WithQuestion(k)).ToList();
   }

   public KanjiNote? WithKanji(string kanji)
   {
      var results = Cache.WithQuestion(kanji);
      return results.Count == 1 ? results[0] : null;
   }

   public List<KanjiNote> WithRadical(string radical) => Cache.WithRadical(radical);

   /// <summary>Renames the word-reference <paramref name="oldName"/> to <paramref name="newName"/> in every kanji's primary vocab, after the vocab that had the one name got the other.</summary>
   internal void RenameThePrimaryVocab(WordReference oldName, WordReference newName)
   {
      foreach(var kanji in All())
         kanji.Edit(it => it.PrimaryVocab.Rename(oldName, newName));
   }

   public HashSet<KanjiNote> WithReading(string reading)
   {
      var hiraganaReading = KanaUtils.AnythingToHiragana(reading);
      return Cache.WithReading(hiraganaReading);
   }

   /// <inheritdoc cref="SentenceCollection.Add"/>
   public void Add(KanjiNote note)
   {
      Cache.AddToCache(note);
      note.UpdateInCache();
   }
}
