using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Reservoir.Measures;

/// <summary>
/// What the corpus's sentences exercise, counted as the measures over the reservoir count it: for each vocab, how many sentences match it, how<br/>
/// many of its matches each failure reason rejects, and how many sentences display each word no vocab has. The sentences are analyzed anew,<br/>
/// each with its configuration, as its stored parsing result is made, to count the rejections, which a stored parsing result doesn't keep.
/// </summary>
public sealed class WhatTheCorpusHas
{
   readonly Dictionary<string, int> _sentencesByVocabNoteId = [];
   readonly Dictionary<(string VocabNoteId, string Reason), int> _rejectionsByVocabAndReason = [];
   readonly Dictionary<string, int> _sentencesDisplayingUnmatchedWord = [];

   WhatTheCorpusHas(JPCollection collection)
   {
      var analyses = collection.Sentences.All().AsParallel().Select(it => it.CreateAnalysis()).ToList();
      foreach(var analysis in analyses)
      {
         foreach(var noteId in analysis.ValidMatches.OfType<VocabMatch>().Select(it => it.Vocab.GetId().ToString()).Distinct())
            _sentencesByVocabNoteId[noteId] = _sentencesByVocabNoteId.GetValueOrDefault(noteId) + 1;

         var rejections = analysis.Locations.SelectMany(it => it.CandidateWords).SelectMany(it => it.Variants).SelectMany(it => it.Matches).OfType<VocabMatch>().Where(it => !it.IsValid);
         foreach(var rejection in rejections.SelectMany(match => match.FailureReasons.Select(reason => (match.Vocab.GetId().ToString(), reason))))
            _rejectionsByVocabAndReason[rejection] = _rejectionsByVocabAndReason.GetValueOrDefault(rejection) + 1;

         foreach(var form in analysis.DisplayMatches.Where(it => it is not VocabMatch).Select(it => it.ParsedForm).Distinct())
            _sentencesDisplayingUnmatchedWord[form] = _sentencesDisplayingUnmatchedWord.GetValueOrDefault(form) + 1;
      }

      SentenceCount = analyses.Count;
   }

   public static WhatTheCorpusHas In(JPCollection collection) => new(collection);

   public int SentenceCount { get; }

   /// <summary>How many of the corpus's sentences have a valid match of the vocab with the note id <paramref name="vocabNoteId"/>.</summary>
   public int SentencesMatching(string vocabNoteId) => _sentencesByVocabNoteId.GetValueOrDefault(vocabNoteId);

   /// <summary>How many of the vocab's matches in the corpus's sentences the failure reason <paramref name="reason"/> rejects.</summary>
   public int Rejections(string vocabNoteId, string reason) => _rejectionsByVocabAndReason.GetValueOrDefault((vocabNoteId, reason));

   /// <summary>How many of the corpus's sentences display <paramref name="form"/> as a word no vocab has: a dictionary entry, or nothing.</summary>
   public int SentencesDisplayingUnmatched(string form) => _sentencesDisplayingUnmatchedWord.GetValueOrDefault(form);
}
