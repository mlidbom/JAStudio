using System.Collections.Generic;
using JAStudio.Core.Note.Vocabulary;
using static JAStudio.Core.Specifications.Fixtures.BaseData.SampleData.VocabMatchingSetting;

namespace JAStudio.Core.Specifications.Fixtures.BaseData.SampleData;

static class VocabLists
{
   public static readonly List<VocabSpec> TestSpecialVocab =
   [
      // <non-standard-token-splitting-to-enable-more-pedagogical-breakdowns-for-conjugations>
      new("う", "dictionary form verb inflection", matching: [Requires.DictionaryFormStart]),
      // godan potential
      new("える",
          "to-be-able-to",
          ["える"],
          forms: ["える", "ける", "せる", "てる", "ねる", "へる", "める", "れる", "げる", "ぜる", "でる", "べる", "ぺる"],
          matching: [IsInflectingWord, Requires.GodanPotential, QuestionOverridesForm]),
      new("えない", "unable-able-to", ["えない"], compounds: ["える", "ない"], matching: [IsInflectingWord]),
      // /godan potential
      new("え", "_!/do! (godan imperative)", ["え"], forms: ["え", "け", "せ", "ね", "へ", "め", "れ", "げ", "ぜ", "で", "べ", "ぺ"], matching: [IsInflectingWord, Requires.GodanImperative, QuestionOverridesForm]),

      new("ろ", "_!/do! (ichidan imperative)", ["ろ"], matching: [IsInflectingWord, Requires.IchidanImperative]),
      new("よ", "_!/do! (ichidan imperative)", ["よ"], matching: [IsInflectingWord, Requires.IchidanImperative]),
      new("い", "_!/do! (godan-special imperative)", ["い"], matching: [IsInflectingWord, Requires.GodanImperative]),

      // needs exclusion
      new("う", "volational inflection", matching: [Forbids.DictionaryFormStart]),
      new("うん", matching: [Forbids.DictionaryFormStart]),
      new("よ", "emphasis", ["よ"], matching: [Forbids.IchidanImperative]),
      new("せよ", matching: [Forbids.GodanImperative]),
      new("させる", "get-_/is-_", ["させる"], forms: ["せる"], matching: [IsInflectingWord, Forbids.GodanPotential]),
      new("頑張れ", "do-your-best!", matching: [Forbids.GodanPotential]),
      new("あれても", forms: ["れても"], compounds: ["あれる", "ても"], matching: [YieldLastTokenToOverlappingCompound, Requires.Irrealis, Requires.Godan, Forbids.GodanPotential, QuestionOverridesForm]),
      new("くえ", "longtooth-grouper", matching: [Forbids.GodanImperative]),
      // /needs exclusion

      // related
      new("なさる", matching: [IsInflectingWord]),
      new("なさい", matching: [IsInflectingWord]),
      // /related

      // compounds
      new("落ち着ける", compounds: ["落ち着く", "える"]),
      // /compounds

      // untangle offending actual dictionary entry
      new("楽しめる", compounds: ["楽しむ", "える"], matching: [IsPoisonWord]), // janome detects it as an ichidan so this sets it straight without showing the word, since it's a poison word...
      // /untangle offending actual dictionary entry

      // </non-standard-token-splitting-to-enable-more-pedagogical-breakdowns-for-conjugations>

      // infinite recursion with recursive shadowed and yielding implementetion for sentence: 服を引き出しの中に入れてください
      new("の中に", compounds: ["の中", "に"], matching: [YieldLastTokenToOverlappingCompound]),
      new("の中", compounds: ["の", "中"]),
      new("中に", compounds: ["中", "に"]),
      new("に入る", compounds: ["に", "入る"]),
      new("入れる"),
      // infinite recursion with recursive shadowed and yielding implementetion

      // <te-stem-required>
      new("て", "{continuing-action}", ["て"], matching: [IsInflectingWord, Requires.TeFormStemStart]),
      new("てる", matching: [IsInflectingWord, Requires.TeFormStemStart]),
      new("とる:progressive", matching: [IsInflectingWord, Requires.TeFormStemStart]),
      new("てん", matching: [IsInflectingWord, Requires.TeFormStemStart]),
      new("とん", matching: [IsInflectingWord, Requires.TeFormStemStart]),
      new("ている", "is-_-ing", readings: ["ている"], matching: [IsInflectingWord, Requires.TeFormStemStart]),
      new("てた", "{was}-{_-ing|_ed}", ["てた"], matching: [IsInflectingWord, Requires.TeFormStemStart]),
      new("てたら", "{was}-{_-ing|_ed}", ["てたら"], matching: [IsInflectingWord, Requires.TeFormStemStart]),

      new("んで", "and/て", forms: ["で"], prefixIn: ["ん"], matching: [Requires.TeFormStemStart, QuestionOverridesForm]),
      new("んどる", forms: ["どる"], prefixIn: ["ん"], matching: [QuestionOverridesForm, Requires.TeFormStemStart]),
      new("んでる", forms: ["でる"], prefixIn: ["ん"], matching: [QuestionOverridesForm, Requires.TeFormStemStart]),
      // </te-stem-required>
      // <te-stem-forbidden>
      new("で", matching: [Forbids.TeFormStemStart]),
      new("でいる", matching: [Forbids.TeFormStemStart]),
      new("んで", "thing-is", matching: [Forbids.TeFormStemStart]),
      new("とんだ", matching: [Forbids.TeFormStemStart]),
      // </te-stem-forbidden>
      new("１人で", compounds: ["で", "１人"], matching: [YieldLastTokenToOverlappingCompound]),
      new("ないで", compounds: ["ない", "で"], matching: [YieldLastTokenToOverlappingCompound]),

      // <past-tense-required>
      new("た", "{past-tense} | (please)do", ["た"], surfaceNot: ["たら"], matching: [IsInflectingWord, Requires.PastTenseStem]),
      new("んだ:past", "did/was", forms: ["だ"], matching: [Requires.PastTenseStem, IsInflectingWord, QuestionOverridesForm]),
      // <past-tense-required>
      // <past-tense-forbidden>
      new("んだ:のだ", "thing-is", matching: [Requires.Surface, YieldLastTokenToOverlappingCompound, Forbids.PastTenseStem]),
      new("たって", matching: [Forbids.PastTenseStem]),
      new("だ", surfaceNot: ["なら", "な"], matching: [IsInflectingWord, Forbids.PastTenseStem]),
      // </past-tense-forbidden>

      new("たら", "conj{if/when} prt{as-for | why-not..  | I-said!/I-tell-you!}", ["たら"], matching: [IsInflectingWord]),
      new("ちゃう", "to do: accidentally/unfortunately | completely", ["ちゃう"], matching: [IsInflectingWord]),
      new("ても良い", "{concession/compromise} | {permission}", ["てもいい"], matching: [IsInflectingWord]),
      new("すぎる", "too-much", ["すぎる"], matching: [IsInflectingWord]),
      new("いらっしゃいます", "to: come/be/do", ["いらっしゃいます"]),
      new("を頼む", "I-entrust-to-you", ["を頼む"], matching: [Requires.Surface]),
      new("作れる", "to-be-able: to-make", ["つくれる"], compounds: ["作る", "える"]),
      new("たい", "want to", ["たい"], matching: [IsInflectingWord]),
      new("解放する", "to{} release", ["かいほうする"]),

      // require a stems
      new("あれる", "get-_/is-_", ["あれる"], forms: ["れる"], matching: [Requires.Irrealis, Requires.Godan, QuestionOverridesForm, IsInflectingWord]),
      new("あせる", "get-_/is-_", ["あせる"], forms: ["せる"], matching: [Requires.Irrealis, Requires.Godan, QuestionOverridesForm, IsInflectingWord]),

      new("する", "to: do", yieldToSurface: ["しろ"]),
      new("しろ", "do!", ["しろ"]),
      new("らっしゃる", yieldToSurface: ["らっしゃい"]),
      new("らっしゃい"),

      new("ぬ", "not", ["ぬ"], surfaceNot: ["ず"]),

      new("だの", "and-the-like", ["だの"], prefixNot: ["ん"]),

      new("こ", "familiarizing-suffix", ["こ"], forms: ["っこ"], matching: [Forbids.SentenceStart]),

      new("ない", "not", forms: ["ない"], matching: [IsInflectingWord]),
      new("無い", "not", forms: ["ない"], matching: [IsInflectingWord]),
      new("うまい", yieldToSurface: ["うまく"]),
      new("うまく"),
      new("笑える", tos: [POS.IchidanVerb]),

      new("にする", "to: turn-into"),
      new("のか", matching: [Requires.SentenceEnd]),
      new("ないと", matching: [YieldLastTokenToOverlappingCompound]),
      new("して", matching: [YieldLastTokenToOverlappingCompound]),
      new("ても"),
      new("と思う"),
      new("たの", matching: [IsPoisonWord]),

      new("たかな", matching: [IsPoisonWord]),
      new("たか", matching: [IsPoisonWord]),
      new("なんて", matching: [YieldLastTokenToOverlappingCompound]),
      new("何て", matching: [YieldLastTokenToOverlappingCompound]),
      new("というか", forms: ["っていうか", "ていうか", "て言うか"], ownedForms: ["と言うか"]),
      new("ていうか", forms: ["と言うか", "というか", "っていうか"], ownedForms: ["て言うか"]),
      new("鰻", ownedForms: ["うな"], prefixNot: ["ろ", "よ"]),
      new("書き"),
      new("風の強い", matching: [Requires.Surface]),
      new("たね", matching: [Requires.SingleToken]),
      new("たらしい", matching: [Requires.SingleToken]),
      new("だって", matching: [Requires.SingleToken, YieldLastTokenToOverlappingCompound]),
      new("に決まる", forms: ["に決る", "に決まる", "に極る"]),
      new("に決まってる", forms: ["に決っている", "に決まっている", "に極っている", "に決ってる", "に決まってる", "に極ってる"]),
      new("された", surfaceNot: ["されたら"]),

      new("んです", matching: [Requires.Surface, YieldLastTokenToOverlappingCompound]),

      new("たん", forms: ["たの"], matching: [Requires.SingleToken]),
      new("たの", forms: ["たん"], matching: [YieldLastTokenToOverlappingCompound]),
      new("たんだ", matching: [YieldLastTokenToOverlappingCompound]),
      new("んだろう", matching: [IsPoisonWord]),
      new("しちゃう"),
      new("ものを", matching: [Requires.SentenceEnd]),
      new("いいものを", forms: ["よいものを", "良いものを", "かったものを"], matching: [Requires.SentenceEnd]),
      new("に行く", compounds: ["に", "行く"], matching: [YieldLastTokenToOverlappingCompound]),
      new("行った", compounds: ["行く", "た"], matching: [YieldLastTokenToOverlappingCompound]),

      new("うと", compounds: ["う", "と"], matching: [YieldLastTokenToOverlappingCompound, Forbids.DictionaryFormStart]),
      new("と思って", compounds: ["と思う", "て"], matching: [YieldLastTokenToOverlappingCompound]),
      new("ませ", forms: ["まし"], suffixNot: ["ん"]),
      new("ところが", matching: [Requires.SentenceStart]),

      new("成る", "to: become | result-in | turn-into", ["なる"], prefixNot: ["く"]),
      new("なる", "to: become | result-in | turn-into", ["なる"], prefixNot: ["く"]),
      new("くなる", "to: become", ["くなる"], forms: ["なる"], prefixIn: ["く"], matching: [QuestionOverridesForm]),
      new("言える", "to-be: able-to-say", compounds: ["言う", "える"]),
      new("出会える", "to: be-{able/fortunate-enough}-to-{meet/come-across}"),
      new("ていける", "can-go-on"),

      new("ても知らない", forms: ["ても知らん"], compounds: ["ても", "知る", "ん"], matching: [Requires.TeFormStemStart]),

      new("とおり"),
      new("られる", matching: [IsInflectingWord]),
      new("返せる", tos: [POS.IchidanVerb]), // exposes a bug in godan imperative detection
      new("外出中", matching: [IsCompositionallyTransparentCompound]),
      new("買い替える", matching: [IsCompositionallyTransparentCompound]),

      new("きれない", ownedForms: ["切れない"], matching: [Requires.MasuStem]),

      new("考えすぎ", matching: [Forbids.AdverbHead]),
      new("考えすぎる", matching: [Forbids.AdverbHead]),

      new("な", forms: ["な"]),
      new("な:dict", matching: [Requires.DictionaryFormHead]),
      new("な:masu", matching: [Requires.MasuStem]),
      new("な:s.end", matching: [Requires.SentenceEnd, Forbids.SentenceStart]),
      new("な:s.start", matching: [Requires.SentenceStart]),
      new("すんな", ownedForms: ["すな"], matching: [Forbids.DictionaryFormStart]),

      new("がある", matching: [YieldLastTokenToOverlappingCompound]),
      new("うの", matching: [Requires.DictionaryFormStart, QuestionOverridesForm]),

      new("寝れる", matching: [IsPoisonWord]),
      new("れる:ichidan", matching: [Requires.Ichidan, Requires.Irrealis]),
      new("れない", matching: [Requires.Ichidan, Requires.Irrealis]),
      new("っ放し", forms: ["っはなし", "っぱなし"], matching: [IsInflectingWord]),

      new("なぜかというと"),
      new("おうと", forms: ["うと"]),

      new("に会う", forms: ["にあう"]),

      new("よい", yieldToSurface: ["よく"]),
      new("よく", suffixNot: ["て"]),

      new("ないし", matching: [Forbids.Irrealis, Forbids.SentenceEnd, Forbids.AdverbHead]),

      new("いただける:able-to", matching: [Requires.TeFormStemHead]),
      new("いただける:acceptable", matching: [Forbids.TeFormStemHead]),
      new("てない", matching: [Requires.TeFormStemStart]),
      new("てしまいます", matching: [Requires.TeFormStemStart]),

      new("う以上", matching: [Requires.DictionaryFormStart])
   ];
}
