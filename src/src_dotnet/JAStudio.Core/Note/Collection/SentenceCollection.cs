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

   public List<SentenceNote> PotentiallyMatchingVocab(VocabNote vocab)
   {
      List<string> searchStrings;
      if(vocab.MatchingConfiguration.RequiresForbids.Surface.IsRequired)
      {
         searchStrings = vocab.Forms.AllList();
      } else
      {
         searchStrings = vocab.Conjugator.GetStemsForAllForms()
                              .Concat(vocab.Forms.AllList())
                              .ToList();
      }

      var questions = All().Select(it => it.GetQuestion()).ToList();
      var matching = questions
                    .Where(q => searchStrings.Any(s => q.Contains(s)))
                    .Distinct()
                    .ToList();

      return matching
            .SelectMany(q => Cache.WithQuestion(q))
            .Distinct()
            .ToList();
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

   public List<SentenceNote> WithVocabMarkedInvalid(VocabNote vocabNote) => Cache.WithUserMarkedInvalidVocab(vocabNote.Question.DisambiguationName);
   
   public List<SentenceNote> WithForm(string form) => Cache.WithVocabForm(form);

   public List<SentenceNote> WithUserHighlightedVocab(string form) => Cache.WithUserHighlightedVocab(form);

   public void Add(SentenceNote note)
   {
      _backendNoteCreator.CreateSentence(note, () => Cache.AddToCache(note));
   }
}
