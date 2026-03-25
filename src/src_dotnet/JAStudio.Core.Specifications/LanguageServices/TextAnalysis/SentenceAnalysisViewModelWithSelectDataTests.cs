using JAStudio.Core.Configuration;
using JAStudio.Core.Specifications.Fixtures;
using Xunit;

namespace JAStudio.Core.Specifications.LanguageServices.TextAnalysis;

/// <summary>
/// Tests ported from test_sentence_analysis_viewmodel_with_select_data.py
/// </summary>
public class SentenceAnalysisViewModelWithSelectDataTests : SentenceAnalysisViewModelCommon
{
   public SentenceAnalysisViewModelWithSelectDataTests() : base(DataNeeded.Vocabulary) {}

   [Theory]
   [InlineData("座っているのはいただけない", "座る", "ている", "の", "は", "いただける:acceptable", "ない")]
   [InlineData("お金を貸していただけないでしょうか", "お金", "を", "貸す", "て", "いただける:able-to", "ない", "でしょうか")]
   [InlineData("お腹空かしてない", "お腹", "空かす", "てない")]
   [InlineData("さっき殴ったから拗ねてんのか", "さっき", "殴る", "た", "から", "拗ねる", "てん", "のか")]
   [InlineData("言っとる", "言う", "とる:progressive")]
   [InlineData("何言っとんだ", "何", "言う", "とん", "んだ:past")]
   [InlineData("長居してしまいまして", "長居", "する", "てしまいます", "て")]
   [InlineData("天気がよくて", "天気", "が", "よい", "て")]
   [InlineData("馴染めないでいる", "馴染む", "える", "ない", "でいる")]
   public void RequireForbidTePrefixOrStem(string sentence, params string[] expectedOutput)
   {
      AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
   }

   [Theory]
   [InlineData("寝れない", "寝る", "れない")]
   [InlineData("食べれる", "食べる", "れる:ichidan", "る:う")]
   public void RequiresEStem(string sentence, params string[] expectedOutput)
   {
      AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
   }

   [Theory]
   [InlineData("おっせぇ<wbr>な　あいつら", "おる", "せ", "ぇ:[MISSING]:emergency", "な:s.end", "あいつら")]
   [InlineData("何て言うか<wbr>さ", "何", "て言うか:ていうか", "さ")]
   [InlineData("だったら普通に金<wbr>貸せって言えよ", "だったら", "普通に", "金", "貸す", "え", "って言う", "え", "よ")]
   public void WbrWordSeparation(string sentence, params string[] expectedOutput)
   {
      AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
   }

   [Theory]
   [InlineData("食べるな", "食べる", "る:う", "な:dict")]
   [InlineData("食べな", "食べる", "な:masu")]
   [InlineData("そうだな", "そうだ", "な:s.end")]
   [InlineData("頭突き以外でな", "頭突き", "以外", "で", "な:s.end")]
   [InlineData("胸あるよ", "胸", "ある", "う", "よ")]
   [InlineData("和むし", "和む", "う", "し")]
   [InlineData("デカいな", "デカい", "な:masu")]
   public void RequireForbidDictionaryFormPrefixAndStem(string sentence, params string[] expectedOutput)
   {
      AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
   }

   [Theory]
   [InlineData("なる", "なる", "う")]
   [InlineData("する", "する", "る:う")]
   [InlineData("くる", "くる", "う")]
   [InlineData("食べる", "食べる", "る:う")]
   [InlineData("はしゃいでる", "はしゃぐ", "でる")]
   [InlineData("音がするの", "音がする", "うの")]
   [InlineData("大声出すな", "大声出す", "う", "な:dict")]
   [InlineData("があるの", "がある", "うの")]
   [InlineData("にある", "に", "ある", "う")]
   [InlineData("なぜかというと", "なぜかというと")]
   [InlineData("止めるかな", "止める", "る:う", "かな")]
   public void DictionaryFormSplitting(string sentence, params string[] expectedOutput)
   {
      AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
   }

   [Theory]
   [InlineData("考えすぎ", "考えすぎ")]
   [InlineData("難しく考えすぎ", "難しい", "考える", "すぎ")]
   public void ForbidAdverbStem(string sentence, params string[] expectedOutput)
   {
      AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
   }

   [Theory]
   [InlineData("平日に切れないよう", "平日", "に", "切れる", "ない", "よう")]
   [InlineData("償いきれない", "償い", "きれない")]
   public void RequiresMasuStem(string sentence, params string[] expectedOutput)
   {
      AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
   }

   [Theory]
   [InlineData("教えにしたがって", "教え", "に", "したがって")]
   public void HideAllCompounds(string sentence, params string[] expectedOutput)
   {
      try
      {
         GetService<JapaneseConfig>().HideAllCompounds.Value = true;
         AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
      }
      finally
      {
         GetService<JapaneseConfig>().HideAllCompounds.Value = false;
      }
   }

   [Theory]
   [InlineData("外出中", "外出", "中")]
   [InlineData("買い替えています", "買い替える", "ている", "ます")]
   public void HideTransparentCompounds(string sentence, params string[] expectedOutput)
   {
      AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
   }

   [Theory]
   [InlineData("お金貸せって", "お金", "貸す", "え", "って")]
   [InlineData("お前に会えて", "お前", "に会う", "える", "て")]
   [InlineData("逆に大丈夫に思えてくる", "逆に", "大丈夫", "に", "思える", "て", "くる", "る:う")]
   [InlineData("黙れ", "黙る", "え")]
   [InlineData("楽しめてる", "楽しむ", "える", "てる")]
   [InlineData("会えたりしない", "会う", "える", "たり", "する", "ない")]
   [InlineData("くれよ", "くれる", "え", "よ")]
   [InlineData("放せよ　俺は…", "放す", "え", "よ", "俺", "は")]
   [InlineData("出ていけ", "出ていく", "え")]
   [InlineData("進めない", "進める", "ない")]
   [InlineData("さっさと傷を清めてこい", "さっさと", "傷", "を", "清める", "てこ", "い")]
   [InlineData("清めの一波", "清め", "の", "一波:[MISSING]:emergency")]
   [InlineData("ここは清められ", "ここ", "は", "清める", "られる")]
   [InlineData("その物陰に隠れろ", "その", "物陰", "に", "隠れる", "ろ")]
   [InlineData("聞けよ", "聞え", "よ")]
   [InlineData("返せったら", "返す", "え", "ったら")]
   [InlineData("返せ俺の", "返す", "え", "俺", "の")]
   [InlineData("返せ盗人", "返す", "え", "盗人")]
   [InlineData("カバンに入れっぱなし", "カバン", "に", "入れる", "っぱなし:っ放し")]
   [InlineData("綺麗 母様に見せよう", "綺麗", "母様", "に", "見せる", "う")]
   public void GodanPotentialAndImperative(string sentence, params string[] expectedOutput)
   {
      AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
   }

   [Theory]
   [InlineData("食べろ", "食べる", "ろ")]
   [InlineData("食べよ", "食べる", "よ")]
   [InlineData("見つけよ", "見つける", "よ")]
   [InlineData("捕まえよ", "捕まえる", "よ")]
   [InlineData("離れよ", "離れる", "よ")]
   [InlineData("落ち着け！", "落ち着く", "え")]
   [InlineData("食べよう", "食べる", "う")]
   public void IchidanImperative(string sentence, params string[] expectedOutput)
   {
      AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
   }

   [Theory]
   [InlineData("書きなさい", "書く", "なさい")]
   public void IImperativeForms(string sentence, params string[] expectedOutput)
   {
      AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
   }

   [Theory(Skip = "TODO: Broken by port to MeCab")]
   [InlineData("幼すぎて よく覚えていないけど", "幼い", "すぎる", "て", "よく", "覚える", "ている", "ない", "けど")]
   public void BrokenByPortToMecab(string sentence, params string[] expectedOutput)
   {
      AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
   }

   [Theory]
   [InlineData("厳密に言えば　俺一人が友達だけど", "厳密に言えば", "俺", "一人", "が", "友達", "だけど")]
   [InlineData("厳密に言えば　俺一人が友達だけどだけど", "厳密に言えば", "俺", "一人", "が", "友達", "だけど", "だけど")]
   [InlineData("ばら撒かれるなんて死んでもいやだ", "ばら撒く", "あれる", "る:う", "なんて", "死んでも", "いや", "だ")]
   [InlineData("お前も色々考えてるんだなぁ", "お前", "も", "色々", "考える", "てる", "んだ:のだ", "なぁ")]
   [InlineData("教科書落ちちゃうから", "教科書", "落ちる", "ちゃう", "う", "から")]
   [InlineData("待ってました", "待つ", "て", "ます", "た")]
   [InlineData("落ちてないかな", "落ちる", "てない", "かな")]
   [InlineData("分かってたら", "分かる", "てたら")]
   [InlineData("思い出せそうな気がする", "思い出す", "える", "そうだ", "気がする", "る:う")]
   [InlineData("代筆を頼みたいんだが", "代筆", "を", "頼む", "たい", "ん", "だが")]
   [InlineData("飛ばされる", "飛ばす", "あれる", "る:う")]
   [InlineData("破られたか", "破る", "あれる", "た", "か")]
   [InlineData("大家族だもの", "大家族", "だもの")]
   [InlineData("奪うんだもの", "奪う", "う", "ん", "だもの")]
   [InlineData("やり過ぎた", "やり過ぎる", "た")]
   [InlineData("ない", "ない")]
   [InlineData("俺に謝られても", "俺", "に", "謝る", "あれても")]
   [InlineData("いいのかよ", "いい", "のか", "よ")]
   [InlineData("立ってるのかと思った", "立つ", "てる", "のか", "と思う", "た")]
   [InlineData("ないと思う", "ない", "と思う", "う")]
   [InlineData("しても", "する", "ても")]
   [InlineData("見えなくなったって そんな", "見える", "なくなる", "たって:emergency", "そんな")]
   [InlineData("焼けたかな", "焼ける", "た", "かな")]
   [InlineData("また来ような", "また", "来る", "う", "な:s.end")]
   [InlineData("何なんだろうな", "何だ", "ん", "だろう", "な:s.end")]
   [InlineData("存在したね", "存在", "する", "た", "ね")]
   [InlineData("作るに決まってるだろ", "作る", "う", "に決まってる", "だろ")]
   [InlineData("知らないんでしょう", "知らない", "ん", "でしょう")]
   [InlineData("横取りされたらたまらん", "横取り", "される", "たら", "たまらん")]
   [InlineData("ガチだったんでしょ", "ガチ", "だった", "ん", "でしょ")]
   [InlineData("どうしちゃったんだろうな", "どう", "しちゃう", "たん:たの", "だろう", "な:s.end")]
   [InlineData("良いものを食べる", "良い", "もの", "を", "食べる", "る:う")]
   [InlineData("いいものを食べる", "いい", "もの", "を", "食べる", "る:う")]
   [InlineData("うまく笑えずに", "うまく", "笑える", "ずに")]
   [InlineData("慣れているんでね", "慣れる", "ている", "んで", "ね")]
   [InlineData("私が頼んだの", "私", "が", "頼む", "んだ:past", "の")]
   [InlineData("月光が差し込んでるんだ", "月光", "が", "差し込む", "んでる", "んだ:のだ")]
   [InlineData("悪かったって", "悪い", "た", "って")]
   [InlineData("落としたって何を", "落とす", "た", "って", "何を")]
   [InlineData("行ったって話", "行く", "たって:emergency", "話")]
   [InlineData("会いに行ったんだ", "会う", "に行く", "たんだ")]
   [InlineData("聞こうと思ってた", "聞く", "う", "と思う", "てた")]
   [InlineData("沈んで", "沈む", "んで")]
   [InlineData("死んどる", "死ぬ", "んどる")]
   [InlineData("ちょっと強引なところがあるから", "ちょっと", "強引", "な", "ところ", "がある", "う", "から")]
   [InlineData("また寒くなるな", "また", "寒い", "くなる", "う", "な:dict")]
   [InlineData("空を飛べる機械", "空を飛ぶ", "える", "る:う", "機械")]
   [InlineData("出会える", "出会える", "る:う")]
   [InlineData("頑張れた", "頑張る", "える", "た")]
   [InlineData("頑張れ", "頑張れ")]
   [InlineData("私たちなら嘘をつかずに付き合っていけるかもしれないね", "私たち", "なら", "嘘をつく", "ずに", "付き合う", "ていける", "る:う", "かもしれない", "ね")]
   [InlineData("どやされても知らんぞ", "どやす", "あれる", "ても知らん:ても知らない", "ぞ")]
   [InlineData("服を引き出しの中に入れてください", "服", "を", "引き出し", "の中", "に入る", "える", "て", "ください")]
   [InlineData("他人を気遣い", "他人", "を", "気遣う")]
   [InlineData("まだ割れんのか", "まだ", "割れる", "のか")]
   [InlineData("思えないしな", "思える", "ない", "しな")]
   public void MiscStuff(string sentence, params string[] expectedOutput)
   {
      AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
   }

   [Theory]
   [InlineData("しろ", "しろ")]
   [InlineData("後で下に下りてらっしゃいね", "後で", "下に", "下りる", "て", "らっしゃい", "ね")]
   public void YieldToSurface(string sentence, params string[] expectedOutput)
   {
      AssertDisplayWordsEqualAndThatAnalysisInternalStateIsValid(sentence, expectedOutput);
   }

   [Theory]
   [InlineData("厳密に言えば　俺一人が友達だけど", new[] { "厳密に言えば", "言え", "だけど" }, new[] { "厳密", "に", "言う", "ば", "俺", "一人", "が", "友達", "だ", "けど" })]
   [InlineData("厳密に言えばだけど俺一人が友達だけど", new[] { "だけど:6" }, new[] { "厳密に言えば", "だけど:emergency", "俺", "一人", "が", "友達", "だけど" })]
   [InlineData("私は毎日ジョギングをすることを習慣にしています。", new[] { "にして:17", "にし:17", "して:18", "し:18", "してい:18", "い:12", "にする" }, new[] { "私", "は", "毎日", "ジョギング", "を", "する", "る:う", "こと", "を", "習慣", "に", "する", "ている", "ます" })]
   [InlineData("私は毎日ジョギングをすることを習慣にしています。", new[] { "にして:17", "にし:17", "して:18", "し:18", "してい:18" }, new[] { "私", "は", "毎日", "ジョギング", "を", "する", "る:う", "こと", "を", "習慣", "にする", "ている", "ます" })]
   [InlineData("頑張れたというか", new[] { "頑張る", "頑張れ" }, new[] { "頑張:[MISSING]:emergency", "える", "た", "というか" })]
   [InlineData("いらっしゃいません", new[] { "いらっしゃいませ" }, new[] { "いらっしゃいます", "ん" })]
   [InlineData("風の強さに驚きました", new[] { "風の強い" }, new[] { "風", "の", "強さ", "に", "驚き", "ます", "た" })]
   public void IncorrectExclusions(string sentence, string[] incorrectStrings, string[] expectedOutput)
   {
      AssertDisplayWordsEqualWithIncorrectExclusions(sentence, incorrectStrings, expectedOutput);
   }

   [Theory]
   [InlineData("厳密に言えば　俺一人が友達だけど", new[] { "厳密に言えば", "言え", "だけど" }, new[] { "厳密", "に", "言う", "ば", "俺", "一人", "が", "友達", "だ", "けど" })]
   [InlineData("厳密に言えばだけど俺一人が友達だけど", new[] { "だけど:6" }, new[] { "厳密に言えば", "俺", "一人", "が", "友達", "だけど" })]
   [InlineData("私は毎日ジョギングをすることを習慣にしています。", new[] { "にして:17", "にし:17", "して:18", "し:18", "してい:18", "い:12", "にする" }, new[] { "私", "は", "毎日", "ジョギング", "を", "する", "る:う", "こと", "を", "習慣", "に", "する", "ている", "ます" })]
   [InlineData("私は毎日ジョギングをすることを習慣にしています。", new[] { "にして:17", "にし:17", "して:18", "し:18", "してい:18" }, new[] { "私", "は", "毎日", "ジョギング", "を", "する", "る:う", "こと", "を", "習慣", "にする", "ている", "ます" })]
   [InlineData("頑張れたというか", new[] { "頑張る", "頑張れ" }, new[] { "える", "た", "というか" })]
   [InlineData("いらっしゃいません", new[] { "いらっしゃいませ" }, new[] { "いらっしゃいます", "ん" })]
   [InlineData("風の強さに驚きました", new[] { "風の強い" }, new[] { "風", "の", "強さ", "に", "驚き", "ます", "た" })]
   public void HiddenExclusions(string sentence, string[] hiddenStrings, string[] expectedOutput)
   {
      AssertDisplayWordsEqualWithHiddenExclusions(sentence, hiddenStrings, expectedOutput);
   }

   [Theory]
   [InlineData("風の強さに驚きました", "風の強い", "風", "の", "強さ", "強", "強い", "さ", "に", "驚き", "驚く", "まし:ませ", "まし", "ます", "た")]
   public void AllWordsEqual(string sentence, params string[] expectedOutput)
   {
      AssertAllWordsEqual(sentence, expectedOutput);
   }
}
