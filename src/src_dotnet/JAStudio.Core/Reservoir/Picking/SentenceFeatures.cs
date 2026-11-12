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
/// it wants (<see cref="SentencesWanted"/>), less those it has, and one less each time a sentence giving it is picked.
/// </summary>
sealed class SentenceFeatures
{
   const int WorksShowingADictionaryWordForItToCount = 3;
   const int WorksShowingAMissingWordForItToCount = 5;

   readonly ReservoirMeasures _measures;
   readonly WhatTheCorpusHas _corpus;
   readonly SentencesWanted _wanted;
   readonly Dictionary<string, int> _idsByKey = [];
   readonly List<string> _descriptions = [];
   readonly List<int> _sentencesStillLacking = [];
   readonly List<int> _vocabFeatures = [];

   public SentenceFeatures(ReservoirMeasures measures, WhatTheCorpusHas corpus, SentencesWanted wanted)
   {
      _measures = measures;
      _corpus = corpus;
      _wanted = wanted;
   }

   /// <summary>How many of the vocab the reservoir's sentences match still lack sentences: those the reservoir has too few sentences of to give them all.</summary>
   public int VocabStillLackingSentences => _vocabFeatures.Count(IsStillLacking);

   /// <summary>The ids of the features that <paramref name="analysis"/>'s sentence exercises, those found for the first time given one now.</summary>
   public IEnumerable<int> Of(ReservoirSentenceAnalysis analysis)
   {
      foreach(var vocabNumber in analysis.VocabNumbersOfItsValidMatches())
         yield return VocabFeature(vocabNumber);

      foreach(var word in analysis.DisplayedWordsNoVocabHas().Where(IsShownInEnoughWorksToCount))
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

   int VocabFeature(int vocabNumber)
   {
      var featuresBefore = _sentencesStillLacking.Count;
      var feature = FeatureWithKey($"v{vocabNumber}",
                                   () => _wanted.ForAVocab - _corpus.SentencesMatching(VocabNumbered(vocabNumber).NoteId),
                                   () => $"the vocab {VocabNumbered(vocabNumber).Question}");
      if(_sentencesStillLacking.Count > featuresBefore) _vocabFeatures.Add(feature);
      return feature;
   }

   int WordNoVocabHasFeature((string Form, ReservoirWordKind Kind) word) =>
      FeatureWithKey($"w{word.Kind}{word.Form}",
                     () => _wanted.ForAWordNoVocabHas - _corpus.SentencesDisplayingUnmatched(word.Form),
                     () => $"the {(word.Kind == ReservoirWordKind.Dictionary ? "dictionary-only" : "missing")} word {word.Form}");

   int RejectionFeature(VocabRejectedForAReason rejection) =>
      FeatureWithKey($"r{rejection.VocabNumber}:{rejection.ReasonNumber}",
                     () => _wanted.ForARejection - _corpus.Rejections(VocabNumbered(rejection.VocabNumber).NoteId, ReasonNumbered(rejection.ReasonNumber)),
                     () => $"{VocabNumbered(rejection.VocabNumber).Question} rejected: {ReasonNumbered(rejection.ReasonNumber)}");

   /// <summary>Whether the sentences of enough works show <paramref name="word"/>, a displayed word no vocab has, for it to be no name.</summary>
   bool IsShownInEnoughWorksToCount((string Form, ReservoirWordKind Kind) word) => _measures.SentencesByUnmatchedWord[word].Works >= WorksShowingAWordForItToCount(word.Kind);

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
