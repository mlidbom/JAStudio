using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.Collection;

public class SentenceCollection
{
   readonly IBackendNoteCreator _backendNoteCreator;
   internal readonly SentenceCache Cache;
   // ReSharper disable once UnusedMember.Global used from python
   public IExternalNoteUpdateHandler ExternalSyncHandler => Cache;

   public SentenceCollection(IBackendNoteCreator backendNoteCreator, NoteServices noteServices)
   {
      _backendNoteCreator = backendNoteCreator;
      Cache = new SentenceCache(noteServices);
   }

   /// <summary>
   /// The sentences that may contain <paramref name="vocab"/>: those whose question contains one of its forms or their stems<br/>
   /// (<see cref="VocabNote.TextsASentenceItCanMatchInContainsOneOf"/>). A change to the vocab may change how they parse.
   /// </summary>
   public List<SentenceNote> PotentiallyMatchingVocab(VocabNote vocab)
   {
      var searchStrings = vocab.TextsASentenceItCanMatchInContainsOneOf();
      return [.. All().AsParallel().AsOrdered().Where(sentence => ContainsOneOf(sentence.GetQuestion()))];

      bool ContainsOneOf(string question) => searchStrings.Any(question.Contains);
   }

   public List<SentenceNote> SentencesWithSubstring(string substring)
   {
      return Cache.All().Where(s => s.GetQuestion().Contains(substring)).ToList();
   }

   public List<SentenceNote> All() => Cache.All();

   public SentenceNote? WithIdOrNone(NoteId noteId) => Cache.WithIdOrNone(noteId);
   public SentenceNote? WithExternalIdOrNone(long externalNoteId) => Cache.WithExternalIdOrNone(externalNoteId);

   public List<SentenceNote> WithQuestion(string question) => Cache.WithQuestion(question);

   public List<SentenceNote> WithVocab(VocabNote vocabNote)
   {
      var matches = Cache.WithVocab(vocabNote);
      var question = vocabNote.GetQuestion();
      // TODO: isn't this check redundant, won't the match have been removed during indexing?
      return matches.Where(match => !match.Configuration.IncorrectMatches.Words().Contains(question)).ToList();
   }

   public List<SentenceNote> WithVocabOwnedForm(VocabNote vocabNote)
   {
      var question = vocabNote.GetQuestion();
      return vocabNote.Forms.NotOwnedByOtherVocab()
                      .SelectMany(form => Cache.WithVocabForm(form))
                      .Distinct()
                      .Where(match => !match.Configuration.IncorrectMatches.Words().Contains(question))
                      .ToList();
   }

   public List<SentenceNote> WithVocabMarkedIncorrect(VocabNote vocabNote) => Cache.WithVocabMarkedIncorrect(vocabNote.Question.DisambiguationName);

   public List<SentenceNote> WithForm(string form) => Cache.WithVocabForm(form);

   public List<SentenceNote> WithHighlightedVocab(string form) => Cache.WithHighlightedVocab(form);

   /// <summary>Adds <paramref name="note"/> to the corpus, and gives it a note in the open Anki profile to study.</summary>
   public void Add(SentenceNote note)
   {
      _backendNoteCreator.CreateSentence(note, () => Cache.AddToCache(note));
   }

   /// <summary>
   /// Adds <paramref name="note"/> to the corpus only, without a note in Anki: Anki holds the notes being studied, and the corpus holds them all.<br/>
   /// Saving it to the corpus is what <see cref="JPNote.UpdateInCache"/> does for a note in the collection.
   /// </summary>
   public void AddWithoutAnAnkiNote(SentenceNote note)
   {
      Cache.AddToCache(note);
      note.UpdateInCache();
   }
}
