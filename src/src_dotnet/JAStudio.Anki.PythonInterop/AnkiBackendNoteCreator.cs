using System;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.PythonInterop.Utilities;

namespace JAStudio.Anki.PythonInterop;

public class AnkiBackendNoteCreator : IBackendNoteCreator
{
   readonly PythonObjectWrapper _noteCreator;

   public AnkiBackendNoteCreator()
   {
      using(PythonEnvironment.LockGil())
      {
         _noteCreator = PythonEnvironment.Import("jastudio.note.anki_backend_note_creator")
            .Use(module => new PythonObjectWrapper(module.AnkiBackendNoteCreator()));
      }
   }

   public void CreateKanji(KanjiNote note, Action callback) => _noteCreator.Use(it => it.create_kanji(note, callback));

   public void CreateVocab(VocabNote note, Action callback) => _noteCreator.Use(it => it.create_vocab(note, callback));

   public void CreateSentence(SentenceNote note, Action callback) => _noteCreator.Use(it => it.create_sentence(note, callback));
}
