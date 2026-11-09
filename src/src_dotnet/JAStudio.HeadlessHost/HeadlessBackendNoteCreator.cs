using System;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.HeadlessHost;

/// <summary>Refuses to create the Anki note of a new note: the headless JAStudio has no Anki to create it in.</summary>
class HeadlessBackendNoteCreator : IBackendNoteCreator
{
   public void CreateKanji(KanjiNote note, Action callback) => throw NoAnki();
   public void CreateVocab(VocabNote note, Action callback) => throw NoAnki();
   public void CreateSentence(SentenceNote note, Action callback) => throw NoAnki();

   static NotSupportedException NoAnki() => new("The headless JAStudio has no Anki to create a new note's Anki note in.");
}
