using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Specifications.Fixtures;
using Xunit;

namespace JAStudio.Core.Specifications.LanguageServices.TextAnalysis;

/// <summary>
/// Tests ported from test_text_analysis_with_select_data.py
/// </summary>
public class TextAnalysisWithSelectDataTests : SpecificationUsingACollection
{
   public TextAnalysisWithSelectDataTests() : base(DataNeeded.Vocabulary) {}

   [Theory]
   [InlineData("走る", "走る", "う")]
   [InlineData("走って", "走る", "て")]
   [InlineData("これをください。", "これ", "を", "ください", "くださる", "い")]
   [InlineData("ハート形", "ハート形", "ハート", "形")]
   [InlineData("私が行きましょう。", "私", "が", "行く", "行き", "ましょう", "ます", "ましょ", "う")]
   [InlineData("１人でいる時間がこれほどまでに長く感じるとは", "１人で", "１人", "１", "人", "でいる", "で", "いる", "時間", "が", "これほど", "これ", "ほど", "までに", "まで", "に", "長い", "感じる", "とは", "と", "は", "る")]
   [InlineData("どうやってここを知った。", "どうやって", "どう", "やる", "て", "ここ", "を", "知る", "た")]
   [InlineData("彼の日本語のレベルは私と同じ位だ。", "彼の", "彼", "の", "日本語", "の", "レベル", "は", "私", "と", "同じ位", "同じ", "位", "だ")]
   [InlineData("それなのに 周りは化け物が出ることで有名だと聞き", "それなのに", "周り", "は", "化け物", "が", "出る", "こと", "で", "有名", "だ", "と", "聞く", "聞き", "る")]
   [InlineData("清めの一波", "清める", "清め", "の")]
   [InlineData("さっさと傷を清めてこい", "さっさと", "傷", "を", "清める", "てこ", "て", "こい", "くる", "い")]
   [InlineData("すげえ", "すげえ", "すげ")]
   [InlineData("「コーヒーはいかがですか？」「いえ、結構です。お構いなく。」", "コーヒー", "は", "いかが", "ですか", "です", "か", "いえ", "結構", "です", "お構いなく")]
   [InlineData("解放する", "解放する", "解放", "する", "る")]
   [InlineData("落書きしたろ", "落書き", "する", "た")]
   [InlineData("なのかな", "か", "かな", "な", "なの", "の", "のか")]
   [InlineData("前だったのか", "前", "だった", "だ", "たの", "た", "のか", "の", "か")]
   [InlineData("未練たらしい", "未練たらしい", "未練", "たらしい")]
   [InlineData("作るに決まってるだろ", "作る", "う", "に決まってる", "に決まる", "に", "決まる", "てる", "だ", "だろ")]
   [InlineData("良いものを食べる", "良い", "もの", "を", "食べる", "る")]
   [InlineData("のに", "のに")]
   [InlineData("もう逃がしません", "もう", "逃がす", "ません", "ます", "ん")]
   [InlineData("死んどる", "死ぬ", "んどる")]
   [InlineData("そうよ　あんたの言うとおりよ！", "そう", "よ", "あんた", "の", "言うとおり", "言う", "う", "とおり", "よ")]
   public void IdentifyWords(string sentence, params string[] expectedOutput)
   {
      var sentenceNote = CreateTestSentence(sentence, "");
      var words = sentenceNote.GetParsingResult().ParsedWords
                              .Select(w => w.ParsedForm)
                              .ToHashSet()
                              .OrderBy(w => w)
                              .ToList();
      var expected = expectedOutput.ToHashSet().OrderBy(w => w).ToList();
      Assert.Equal(expected, words);
   }

   [Theory]
   [InlineData("言わず", "言う", "ず")]
   [InlineData("声出したら駄目だからね", "声", "出す", "たら", "駄目", "だから", "だ", "から", "ね")]
   [InlineData("無理して思い出す", "無理", "して", "する", "て", "思い出す", "思い出す", "う")]
   [InlineData("私が頼んだの", "私", "が", "頼む", "んだ", "の")]
   public void ExcludedSurfaces(string sentence, params string[] expectedOutput)
   {
      var sentenceNote = CreateTestSentence(sentence, "");
      var words = sentenceNote.GetParsingResult().ParsedWords
                              .Select(w => w.ParsedForm)
                              .ToList();
      Assert.Equal(expectedOutput.ToList(), words);
   }

   [Theory]
   [InlineData("うわ こわっ", "うわ", "こい", "わっ")]
   public void StrictlySuffix(string sentence, params string[] expectedOutput)
   {
      var sentenceNote = CreateTestSentence(sentence, "");
      var words = sentenceNote.GetParsingResult().ParsedWords
                              .Select(w => w.ParsedForm)
                              .ToList();
      Assert.Equal(expectedOutput.ToList(), words);
   }

   [Theory]
   [InlineData("うるせえ", "うるせえ", "う", "せえ", "せる")]
   [InlineData("お金貸せって", "お金", "貸す", "え", "って")]
   public void RequiresAStem(string sentence, params string[] expectedOutput)
   {
      var sentenceNote = CreateTestSentence(sentence, "");
      var rootWords = sentenceNote.GetParsingResult().ParsedWords
                                  .Select(w => w.ParsedForm)
                                  .ToList();
      Assert.Equal(expectedOutput.ToList(), rootWords);
   }

   [Fact]
   public void IgnoresNoiseCharacters()
   {
      var sentence = ". , : ; / | 。 、ー ? !";
      var expected = new HashSet<string> { "ー" };

      var sentenceNote = CreateTestSentence(sentence, "");
      var words = sentenceNote.GetParsingResult().ParsedWords
                              .Select(w => w.ParsedForm)
                              .ToHashSet();
      Assert.Equal(expected, words);
   }

   [Fact]
   public void ThatVocabIsNotIndexedEvenIfFormIsHighlightedIfInvalidAndThereIsAnotherValidVocabWithTheForm()
   {
      var sentence = CreateTestSentence("勝つんだ", "");
      sentence.Configuration.AddHighlightedWord("んだ");
      sentence.UpdateParsedWords(force: true);
      var parsingResult = sentence.GetParsingResult();
      var words = parsingResult.ParsedWords
                               .Select(w => w.ParsedForm)
                               .Distinct()
                               .ToList();
      Assert.Equal(["勝つ", "う", "んだ", "ん", "だ"], words);
   }

   // test_no_memory_leak_weak_references_are_disposed - Skipped
   // This test is Python-specific (tests WeakRef disposal). C# uses normal references with GC.
}
