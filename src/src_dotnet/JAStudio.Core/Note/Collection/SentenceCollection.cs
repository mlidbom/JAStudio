using System.Collections.Generic;
using System.Linq;
using Compze.DependencyInjection.Abstractions;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.Collection;

public class SentenceCollection
{
   readonly StoredNotes<SentenceNote> _notes;
   readonly NoteUnitsOfWork _unitsOfWork;
   readonly NoteIndex<SentenceNote, SentenceIndexes.Question> _byQuestion;
   readonly NoteIndex<SentenceNote, SentenceIndexes.Word> _byWord;
   readonly NoteIndex<SentenceNote, SentenceIndexes.MatchedVocab> _byMatchedVocab;
   readonly NoteIndex<SentenceNote, SentenceIndexes.HighlightedWord> _byHighlightedWord;
   readonly NoteIndex<SentenceNote, SentenceIndexes.WordMarkedAsMatchedIncorrectly> _byWordMarkedAsMatchedIncorrectly;

   internal SentenceCollection(NoteServices noteServices, IRootResolver resolver, NoteUnitsOfWork unitsOfWork)
   {
      _notes = new StoredNotes<SentenceNote>(noteServices, resolver);
      _unitsOfWork = unitsOfWork;
      _byQuestion = _notes.IndexBy<SentenceIndexes.Question>();
      _byWord = _notes.IndexBy<SentenceIndexes.Word>();
      _byMatchedVocab = _notes.IndexBy<SentenceIndexes.MatchedVocab>();
      _byHighlightedWord = _notes.IndexBy<SentenceIndexes.HighlightedWord>();
      _byWordMarkedAsMatchedIncorrectly = _notes.IndexBy<SentenceIndexes.WordMarkedAsMatchedIncorrectly>();
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
      return All().Where(s => s.GetQuestion().Contains(substring)).ToList();
   }

   public List<SentenceNote> All() => _notes.All();

   /// <summary>The sentences that are parsing cases (<see cref="SentenceNote.ParsingCase"/>), in no particular order.</summary>
   public List<SentenceNote> ThatAreParsingCases() => [.. All().Where(it => it.ParsingCase != null)];

   /// <summary>
   /// Each sentence that is a parsing case, parsed now and compared with what its case expects, in no particular order. They're parsed in<br/>
   /// parallel: each is parsed under every combination of the breakdown's options, which takes a few seconds for some hundred cases one by one.
   /// </summary>
   public List<ComparedParsingCase> ParsingCasesCompared() => [.. ThatAreParsingCases().AsParallel().Select(ComparedParsingCase.Of)];

   public SentenceNote? WithIdOrNone(NoteId noteId) => _notes.WithIdOrNone(noteId);
   public SentenceNote? WithTheStudyNoteOrNone(long studyNoteId) => _notes.WithTheStudyNoteOrNone(studyNoteId);

   public List<SentenceNote> WithQuestion(string question) => _byQuestion.NotesWith(new(question));

   public List<SentenceNote> WithVocab(VocabNote vocabNote)
   {
      var matches = _byMatchedVocab.NotesWith(new(vocabNote.GetId()));
      var question = vocabNote.GetQuestion();
      // TODO: isn't this check redundant, won't the match have been removed during indexing?
      return matches.Where(match => !match.Configuration.IncorrectMatches.Words().Contains(question)).ToList();
   }

   public List<SentenceNote> WithVocabOwnedForm(VocabNote vocabNote)
   {
      var question = vocabNote.GetQuestion();
      return vocabNote.Forms.NotOwnedByOtherVocab()
                      .SelectMany(form => _byWord.NotesWith(new(form)))
                      .Distinct()
                      .Where(match => !match.Configuration.IncorrectMatches.Words().Contains(question))
                      .ToList();
   }

   public List<SentenceNote> WithVocabMarkedIncorrect(VocabNote vocabNote) => _byWordMarkedAsMatchedIncorrectly.NotesWith(new(vocabNote.Question.DisambiguationName));

   public List<SentenceNote> WithForm(string form) => _byWord.NotesWith(new(form));

   public List<SentenceNote> WithHighlightedVocab(string form) => _byHighlightedWord.NotesWith(new(form));

   /// <summary>
   /// Adds <paramref name="note"/>, a note just created, to the collection and saves it to the corpus, in a unit of work. It gets no study-note in Anki: the corpus holds every note, and Anki only the notes being studied, whether JAStudio<br/>
   /// runs inside Anki or not (Magnus, 2026-11-13).
   /// </summary>
   public void Add(SentenceNote note)
   {
      _unitsOfWork.Run(it => it.Add(note));
   }
}
