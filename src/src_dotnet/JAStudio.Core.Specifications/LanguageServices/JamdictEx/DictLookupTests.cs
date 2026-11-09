using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Vocabulary;
using Xunit;

namespace JAStudio.Core.Specifications.LanguageServices.JamdictEx;

/// <summary>
/// Tests ported from test_dict_lookup.py
/// </summary>
public class DictLookupTests : SpecificationStartingWithAnEmptyCollection
{
   [Theory]
   [InlineData("為る", new[] { "する" })]
   [InlineData("為る", new[] { "なる" })]
   public void Uk(string word, string[] readings)
   {
      var dictEntry = GetDictEntry(word, readings);
      Assert.True(dictEntry.IsUk());
      Assert.Equal(1, dictEntry.FoundWordsCount());
   }

   [Theory]
   [InlineData("毎月", new[] { "まいつき", "まいげつ" })]
   public void MultiReadings(string word, string[] readings)
   {
      var dictEntry = GetDictEntry(word, readings);
      Assert.Equal(1, dictEntry.FoundWordsCount());
   }

   [Theory]
   [InlineData("元", new[] { "もと" })]
   [InlineData("角", new[] { "かく", "かど" })]
   [InlineData("これ", new[] { "これ" })]
   [InlineData("正す", new[] { "ただす" })]
   [InlineData("て", new[] { "て" })]
   public void MultiMatches(string word, string[] readings)
   {
      var dictEntry = GetDictEntry(word, readings);
      Assert.True(dictEntry.FoundWordsCount() > 1);
   }

   [Theory]
   [InlineData("に", new[] { "に" })]
   [InlineData("しか", new[] { "しか" })]
   [InlineData("ローマ字", new[] { "ろーまじ" })]
   [InlineData("狩り", new[] { "かり" })]
   [InlineData("おけばよかった", new[] { "おけばよかった" })]
   public void Missing(string word, string[] readings)
   {
      var dictEntry = GetDictEntry(word, readings);
      Assert.Equal(1, dictEntry.FoundWordsCount());
   }

   [Theory]
   [InlineData("しない")]
   public void ShouldBeMissing(string word)
   {
      var result = GetService<DictLookup>().LookupWord(word);
      Assert.Empty(result.Entries);
   }

   [Theory]
   [InlineData("田代島", new[] { "たしろじま" })]
   public void Names(string word, string[] readings)
   {
      var dictEntry = GetDictEntry(word, readings);
      Assert.Equal(1, dictEntry.FoundWordsCount());
   }

   [Theory]
   [InlineData("怪我", new[] { "けが" }, new[] { "怪我" })] // Desu's embedded JMDict doesn't mark 怪我 as usually-kana-alone
   [InlineData("部屋", new[] { "へや" }, new[] { "部屋" })]
   public void ValidForms(string word, string[] readings, string[] expectedForms)
   {
      var dictEntry = GetDictEntry(word, readings);
      Assert.Equal(1, dictEntry.FoundWordsCount());

      var expectedSet = new HashSet<string>(expectedForms);
      Assert.Equal(expectedSet, dictEntry.ValidForms());
   }

   [Theory]
   [InlineData("写る", new string[] {}, "to-be: photographed/projected")]
   [InlineData("張り切る", new[] { "はりきる" }, "to{?}{be-in-high-spirits/be-full-of-vigor-(vigour)/be-enthusiastic/be-eager/stretch-to-breaking-point}")]
   [InlineData("早まる", new[] { "はやまる" }, "to: be-{brought-forward-(e.g.-by-three-hours)/moved-up/advanced} | be-{hasty/rash} | {quicken/speed-up/gather-speed}")]
   [InlineData("部屋", new[] { "へや" }, "room/chamber | apartment/flat/pad | stable")]
   [InlineData("拭く", new[] { "ふく" }, "to{} wipe/dry")]
   [InlineData("歩く", new[] { "あるく" }, "to: walk")]
   public void GenerateAnswer(string word, string[] readings, string expectedAnswer)
   {
      var lookupResult = GetDictEntry(word, readings);
      var generatedAnswer = lookupResult.FormatAnswer();
      Assert.Equal(expectedAnswer, generatedAnswer);
   }

   [Theory]
   [InlineData("怪我", new[] { "けが" }, new[] { "noun", "suru verb" })]
   [InlineData("部屋", new[] { "へや" }, new[] { "noun" })]
   [InlineData("確実", new[] { "かくじつ" }, new[] { "na-adjective", "noun" })]
   [InlineData("式", new[] { "しき" }, new[] { "suffix", "noun" })]
   [InlineData("吸う", new[] { "すう" }, new[] { "godan verb", "transitive" })]
   [InlineData("走る", new[] { "はしる" }, new[] { "godan verb", "intransitive" })]
   [InlineData("帰る", new[] { "かえる" }, new[] { "godan verb", "intransitive" })]
   [InlineData("使う", new[] { "つかう" }, new[] { "godan verb", "transitive" })]
   [InlineData("書く", new[] { "かく" }, new[] { "godan verb", "transitive" })]
   [InlineData("立つ", new[] { "たつ" }, new[] { "godan verb", "intransitive" })]
   [InlineData("死ぬ", new[] { "しぬ" }, new[] { "nu verb", "godan verb", "intransitive" })]
   [InlineData("飛ぶ", new[] { "とぶ" }, new[] { "godan verb", "intransitive" })]
   [InlineData("読む", new[] { "よむ" }, new[] { "godan verb", "transitive" })]
   [InlineData("小さな", new[] { "ちいさな" }, new[] { "pre-noun-adjectival" })]
   public void Pos(string word, string[] readings, string[] expectedPos)
   {
      var dictEntry = GetDictEntry(word, readings);
      Assert.Equal(1, dictEntry.FoundWordsCount());

      var expectedPosSet = new HashSet<string>(expectedPos);
      Assert.Equal(expectedPosSet, dictEntry.Entries[0].PartsOfSpeech().Select(it => it.Name).ToHashSet());
      Assert.Equal(expectedPosSet, dictEntry.PartsOfSpeech().Select(it => it.Name).ToHashSet());
   }

   DictLookupResult GetDictEntry(string word, string[] readings)
   {
      var vocab = GetService<VocabNoteFactory>().Create(word, "", [..readings]);
      return GetService<DictLookup>().LookupVocabWordOrName(vocab);
   }
}
