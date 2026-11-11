using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Reservoir.Analysis;
using JAStudio.Core.Reservoir.Analysis.Numbering;
using JAStudio.Core.Reservoir.Measures;

namespace JAStudio.Core.Reservoir.Picking;

/// <summary>
/// The features of reservoir sentences: the things a sentence can exercise, which the corpus wants a few sentences of each. A feature is a<br/>
/// vocab's match, a failure reason rejecting a vocab's match, or a word no vocab has that the sentences of several works show, where a name<br/>
/// stays in one. Each has an id, given when it's first found, a description, and how many more sentences of it the corpus still lacks: those<br/>
/// it wants, less those it has, and one less each time a sentence giving it is picked.
/// </summary>
sealed class SentenceFeatures
{
   const int SentencesWantedForAVocab = 3;
   const int SentencesWantedForARejection = 2;
   const int SentencesWantedForAWord = 1;
   const int WorksShowingADictionaryWordForItToCount = 3;
   const int WorksShowingAMissingWordForItToCount = 5;

   readonly ReservoirMeasures _measures;
   readonly WhatTheCorpusHas _corpus;
   readonly Dictionary<string, int> _idsByKey = [];
   readonly List<string> _descriptions = [];
   readonly List<int> _sentencesStillLacking = [];

   public SentenceFeatures(ReservoirMeasures measures, WhatTheCorpusHas corpus)
   {
      _measures = measures;
      _corpus = corpus;
   }

   /// <summary>The ids of the features that <paramref name="analysis"/>'s sentence exercises, those found for the first time given one now.</summary>
   public IEnumerable<int> Of(ReservoirSentenceAnalysis analysis)
   {
      foreach(var vocabNumber in analysis.VocabNumbersOfItsValidMatches())
         yield return VocabFeature(vocabNumber);

      foreach(var word in analysis.Words.Where(IsADisplayedWordNoVocabHasShownInEnoughWorksToCount))
         yield return WordNoVocabHasFeature(word);

      foreach(var rejection in analysis.VocabRejections())
         yield return RejectionFeature(rejection);
   }

   /// <summary>Whether the corpus still lacks sentences of <paramref name="feature"/>.</summary>
   public bool IsStillLacking(int feature) => _sentencesStillLacking[feature] > 0;

   /// <summary>Counts a picked sentence as giving <paramref name="feature"/>: the corpus lacks one sentence fewer of it.</summary>
   public void GiveOnce(int feature) => _sentencesStillLacking[feature]--;

   /// <summary>What <paramref name="feature"/> is, as a line of text, such as <c>the vocab 仰る</c>.</summary>
   public string DescriptionOf(int feature) => _descriptions[feature];

   int VocabFeature(int vocabNumber) =>
      FeatureWithKey($"v{vocabNumber}",
                     () => SentencesWantedForAVocab - _corpus.SentencesMatching(VocabNumbered(vocabNumber).NoteId),
                     () => $"the vocab {VocabNumbered(vocabNumber).Question}");

   int WordNoVocabHasFeature(ReservoirWord word) =>
      FeatureWithKey($"w{word.Kind}{word.Form}",
                     () => SentencesWantedForAWord - _corpus.SentencesDisplayingUnmatched(word.Form),
                     () => $"the {(word.Kind == ReservoirWordKind.Dictionary ? "dictionary-only" : "missing")} word {word.Form}");

   int RejectionFeature(VocabRejectedForAReason rejection) =>
      FeatureWithKey($"r{rejection.VocabNumber}:{rejection.ReasonNumber}",
                     () => SentencesWantedForARejection - _corpus.Rejections(VocabNumbered(rejection.VocabNumber).NoteId, ReasonNumbered(rejection.ReasonNumber)),
                     () => $"{VocabNumbered(rejection.VocabNumber).Question} rejected: {ReasonNumbered(rejection.ReasonNumber)}");

   /// <summary>Whether <paramref name="word"/> is a displayed word no vocab has, and the sentences of enough works show it for it to be no name.</summary>
   bool IsADisplayedWordNoVocabHasShownInEnoughWorksToCount(ReservoirWord word) =>
      word is { IsDisplayed: true, Kind: not ReservoirWordKind.Vocab } && _measures.SentencesByUnmatchedWord[(word.Form, word.Kind)].Works >= WorksShowingAWordForItToCount(word.Kind);

   /// <summary>
   /// The id of the feature with the key <paramref name="key"/>. A feature found for the first time is given one, with how many sentences of it<br/>
   /// the corpus lacks (<paramref name="sentencesLacking"/>, none when it has enough) and its <paramref name="description"/>.
   /// </summary>
   int FeatureWithKey(string key, Func<int> sentencesLacking, Func<string> description)
   {
      if(_idsByKey.TryGetValue(key, out var id)) return id;

      id = _idsByKey[key] = _sentencesStillLacking.Count;
      _sentencesStillLacking.Add(Math.Max(0, sentencesLacking()));
      _descriptions.Add(description());
      return id;
   }

   NumberedVocab VocabNumbered(int vocabNumber) => _measures.Numbers.VocabNumbered(vocabNumber);

   string ReasonNumbered(int reasonNumber) => _measures.Numbers.ReasonNumbered(reasonNumber);

   static int WorksShowingAWordForItToCount(ReservoirWordKind kind) =>
      kind == ReservoirWordKind.Dictionary ? WorksShowingADictionaryWordForItToCount : WorksShowingAMissingWordForItToCount;
}
