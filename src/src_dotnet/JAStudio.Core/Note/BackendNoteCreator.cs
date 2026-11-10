using System;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note;

public interface IBackendNoteCreator
{
   void CreateKanji(KanjiNote note, Action callback);
   void CreateVocab(VocabNote note, Action callback);
   void CreateSentence(SentenceNote note, Action callback);
}
