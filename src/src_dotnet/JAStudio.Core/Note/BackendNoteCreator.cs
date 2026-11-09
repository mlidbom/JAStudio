using System;
using System.Threading;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note;

public interface IBackendNoteCreator
{
   void CreateKanji(KanjiNote note, Action callback);
   void CreateVocab(VocabNote note, Action callback);
   void CreateSentence(SentenceNote note, Action callback);
}

/// <summary>
/// Gives each note a pretend Anki note in specifications, where there's no Anki: registers a new Anki note id for the note, as creating its<br/>
/// real Anki note does, so that what Anki reports about the Anki note, such as its deletion or its cards' studying status, reaches the note.<br/>
/// Then, as in Anki, the note is saved where the corpus keeps notes.
/// </summary>
class TestingBackendNoteCreator : IBackendNoteCreator
{
   long _lastAnkiNoteId;

   public void CreateKanji(KanjiNote note, Action callback) => CreateThePretendAnkiNoteOf(note, callback);
   public void CreateVocab(VocabNote note, Action callback) => CreateThePretendAnkiNoteOf(note, callback);
   public void CreateSentence(SentenceNote note, Action callback) => CreateThePretendAnkiNoteOf(note, callback);

   void CreateThePretendAnkiNoteOf(JPNote note, Action callback)
   {
      note.Services.ExternalNoteIdMap.Register(Interlocked.Increment(ref _lastAnkiNoteId), note.GetId());
      callback();
      note.UpdateInCache();
   }
}
