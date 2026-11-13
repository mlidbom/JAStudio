using System;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.StandaloneHost;

/// <summary>
/// Creates no Anki notes: the standalone JAStudio has no Anki. Creating a note creates its Anki note, so creating notes is offered only while there's<br/>
/// an Anki (<see cref="AnkiAvailability"/>).
/// </summary>
sealed class NoAnkiNoteCreator : IBackendNoteCreator
{
   public void CreateKanji(KanjiNote note, Action callback) => throw NotOffered();
   public void CreateVocab(VocabNote note, Action callback) => throw NotOffered();
   public void CreateSentence(SentenceNote note, Action callback) => throw NotOffered();

   static InvalidOperationException NotOffered() => new("The standalone JAStudio has no Anki to create a new note's Anki note in, so creating notes isn't offered.");
}
