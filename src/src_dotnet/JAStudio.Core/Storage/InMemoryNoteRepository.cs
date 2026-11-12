using System.Collections.Concurrent;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Storage;

/// <summary>The notes the specifications save, held in memory instead of the corpus's files. Notes are saved from many threads at once, as the reparse does.</summary>
class InMemoryNoteRepository : INoteRepository
{
   readonly ConcurrentDictionary<NoteId, KanjiNote> _kanji = new();
   readonly ConcurrentDictionary<NoteId, VocabNote> _vocab = new();
   readonly ConcurrentDictionary<NoteId, SentenceNote> _sentences = new();

   public void Save(KanjiNote note) => _kanji[note.GetId()] = note;
   public void Save(VocabNote note) => _vocab[note.GetId()] = note;
   public void Save(SentenceNote note) => _sentences[note.GetId()] = note;

   public void Delete(NoteId noteId)
   {
      switch(noteId)
      {
         case VocabId: _vocab.TryRemove(noteId, out _); break;
         case KanjiId: _kanji.TryRemove(noteId, out _); break;
         case SentenceId: _sentences.TryRemove(noteId, out _); break;
      }
   }

   public AllNotesData LoadAll() =>
      new([.. _kanji.Values], [.. _vocab.Values], [.. _sentences.Values]);
}
