using JAStudio.Core.Reservoir.Analysis;

namespace JAStudio.Core.Reservoir.Measures;

/// <summary>
/// How the sentences the reservoir keeps from one work parse: how many there are, and how many display a missing word, one neither a vocab<br/>
/// nor the dictionary has.
/// </summary>
/// <remarks>
/// A work whose sentences often display one is likely garbled: subtitles read off the screen by OCR, as some decks' are, with 力 for か; or<br/>
/// text that isn't Japanese at all. Its "missing words" are then errors, and its sentences no examples worth taking in.
/// </remarks>
public sealed class HowAWorkParses
{
   /// <summary>How many of the work's sentences were measured: those the reservoir keeps from it, with an analysis.</summary>
   public long Sentences { get; private set; }

   /// <summary>The work's sentences that display a missing word.</summary>
   public CountWithExamples SentencesWithAMissingWord { get; } = new();

   /// <summary>The share of the work's sentences that display a missing word, from 0 to 1.</summary>
   public double ShareWithAMissingWord => Sentences == 0 ? 0 : SentencesWithAMissingWord.Count / (double)Sentences;

   /// <summary>Counts <paramref name="sentence"/>, one of the work's.</summary>
   public void Add(AnalyzedReservoirSentence sentence)
   {
      Sentences++;
      if(sentence.Analysis.DisplaysAMissingWord()) SentencesWithAMissingWord.Add(sentence);
   }
}
