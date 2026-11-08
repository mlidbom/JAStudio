using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Specifications.Fixtures.BaseData.SampleData;
using Xunit;

namespace JAStudio.Core.Specifications.Fixtures;

/// <summary>
/// Tests ported from test_collection_factory.py
/// </summary>
public class CollectionFactoryTests : SpecificationUsingACollection
{
   [Fact]
   public void KanjiAddedCorrectly()
   {
      var kanjiAll = GetService<KanjiCollection>().All();
      var savedKanji = kanjiAll.Select(kanji =>
                                          new KanjiSpec(
                                             kanji.GetQuestion(),
                                             kanji.GetAnswer(),
                                             KanjiReadingsNotation.Of(kanji.Readings.On),
                                             KanjiReadingsNotation.Of(kanji.Readings.Kun)
                                          )
      ).ToHashSet();

      Assert.Equal(KanjiSpec.TestKanjiList.ToHashSet(), savedKanji);
   }

   [Fact]
   public void VocabAddedCorrectly()
   {
      var expectedVocab = VocabLists.TestSpecialVocab.OrderBy(x => x.Question).ToList();
      var vocabAll = GetService<VocabCollection>().All();
      var savedVocab = vocabAll
                      .Select(vocab => new VocabSpec(vocab.GetQuestion(), vocab.GetAnswer(), vocab.GetReadings()))
                      .OrderBy(x => x.Question)
                      .ToList();

      Assert.Equal(expectedVocab, savedVocab);
   }

   [Fact]
   public void SentencesAddedCorrectly()
   {
      var expectedSentences = SentenceSpec.TestSentenceList.OrderBy(x => x.Question).ToList();
      var sentencesAll = GetService<SentenceCollection>().All()
                                                         .OrderBy(x => x.Question.WithoutInvisibleSpace())
                                                         .ToList();
      var savedSentences = sentencesAll
                          .Select(sentence => new SentenceSpec(
                                     sentence.Question.WithoutInvisibleSpace(),
                                     sentence.GetAnswer()
                                  ))
                          .ToList();

      Assert.Equal(expectedSentences, savedSentences);
   }
}
