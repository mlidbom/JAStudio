using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Reservoir.Analysis;
using JAStudio.Core.Reservoir.Measures;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>
/// A work's sentences measured for how they parse (<see cref="HowAWorkParses"/>): four sentences, one displaying a missing word, one displaying<br/>
/// only a vocab, one with a missing word it doesn't display, and one displaying a word only the dictionary has.
/// </summary>
public class When_measuring_how_a_works_sentences_parse : IAIGeneratedSpec
{
   readonly HowAWorkParses _work = new();
   long _lastId;

   public When_measuring_how_a_works_sentences_parse()
   {
      _work.Add(SentenceWith(new ReservoirWord(0, "知つてる力", ReservoirWordKind.Missing, ReservoirWord.NoVocabNumber, IsDisplayed: true, IsValid: true)));
      _work.Add(SentenceWith(new ReservoirWord(0, "行く", ReservoirWordKind.Vocab, 1, IsDisplayed: true, IsValid: true)));
      _work.Add(SentenceWith(new ReservoirWord(0, "ぶっ", ReservoirWordKind.Missing, ReservoirWord.NoVocabNumber, IsDisplayed: false, IsValid: true)));
      _work.Add(SentenceWith(new ReservoirWord(0, "所以", ReservoirWordKind.Dictionary, ReservoirWord.NoVocabNumber, IsDisplayed: true, IsValid: true)));
   }

   [XF] public void every_sentence_is_counted() => _work.Sentences.Must().Be(4L);

   [XF] public void only_a_sentence_displaying_a_missing_word_counts_as_one() => _work.SentencesWithAMissingWord.Count.Must().Be(1);

   [XF] public void the_share_is_of_all_the_works_sentences() => _work.ShareWithAMissingWord.Must().Be(0.25);

   AnalyzedReservoirSentence SentenceWith(ReservoirWord word) =>
      new(++_lastId, "a sentence", SourceId: 1, Occurrences: 1, HasAudio: false, new ReservoirSentenceAnalysis([word], []));
}
