using System.Collections.Generic;
using JAStudio.Core.Note.Vocabulary;
using V = JAStudio.Core.Note.Tags.Vocab;
using VM = JAStudio.Core.Note.Tags.Vocab.Matching;

namespace JAStudio.Core.Specifications.Fixtures.BaseData.SampleData;

static class VocabLists
{
   public static readonly List<VocabSpec> TestSpecialVocab =
   [
      // <non-standard-token-splitting-to-enable-more-pedagogical-breakdowns-for-conjugations>
      new("う", "dictionary form verb inflection", forms: ["る"], tags: [VM.Requires.DictionaryFormStart]),
      // godan potential
      new("える",
          "to-be-able-to",
          ["える"],
          forms: ["える", "ける", "せる", "てる", "ねる", "へる", "める", "れる", "げる", "ぜる", "でる", "べる", "ぺる"],
          tags: [VM.IsInflectingWord, VM.Requires.GodanPotential, V.QuestionOverridesForm]),
      new("えない", "unable-able-to", ["えない"], compounds: ["える", "ない"], tags: [VM.IsInflectingWord]),
      // /godan potential
      new("え", "_!/do! (godan imperative)", ["え"], forms: ["え", "け", "せ", "ね", "へ", "め", "れ", "げ", "ぜ", "で", "べ", "ぺ"], tags: [VM.IsInflectingWord, VM.Requires.GodanImperative, V.QuestionOverridesForm]),

      new("ろ", "_!/do! (ichidan imperative)", ["ろ"], tags: [VM.IsInflectingWord, VM.Requires.IchidanImperative]),
      new("よ", "_!/do! (ichidan imperative)", ["よ"], tags: [VM.IsInflectingWord, VM.Requires.IchidanImperative]),
      new("い", "_!/do! (godan-special imperative)", ["い"], tags: [VM.IsInflectingWord, VM.Requires.GodanImperative]),

      // needs exclusion
      new("う", "volational inflection", tags: [VM.Forbids.DictionaryFormStart]),
      new("うん", tags: [VM.Forbids.DictionaryFormStart]),
      new("よ", "emphasis", ["よ"], tags: [VM.Forbids.IchidanImperative]),
      new("せよ", tags: [VM.Forbids.GodanImperative]),
      new("させる", "get-_/is-_", ["させる"], forms: ["せる"], tags: [VM.IsInflectingWord, VM.Forbids.GodanPotential]),
      new("頑張れ", "do-your-best!", tags: [VM.Forbids.GodanPotential]),
      new("あれても", forms: ["れても"], compounds: ["あれる", "ても"], tags: [VM.YieldLastTokenToOverlappingCompound, VM.Requires.Irrealis, VM.Requires.Godan, VM.Forbids.GodanPotential, V.QuestionOverridesForm]),
      new("くえ", "longtooth-grouper", tags: [VM.Forbids.GodanImperative]),
      // /needs exclusion

      // related
      new("なさる", tags: [VM.IsInflectingWord]),
      new("なさい", tags: [VM.IsInflectingWord]),
      // /related

      // compounds
      new("落ち着ける", compounds: ["落ち着く", "える"]),
      // /compounds

      // untangle offending actual dictionary entry
      new("楽しめる", compounds: ["楽しむ", "える"], tags: [VM.IsPoisonWord]), // janome detects it as an ichidan so this sets it straight without showing the word, since it's a poison word...
      // /untangle offending actual dictionary entry

      // </non-standard-token-splitting-to-enable-more-pedagogical-breakdowns-for-conjugations>

      // infinite recursion with recursive shadowed and yielding implementetion for sentence: 服を引き出しの中に入れてください
      new("の中に", compounds: ["の中", "に"], tags: [VM.YieldLastTokenToOverlappingCompound]),
      new("の中", compounds: ["の", "中"]),
      new("中に", compounds: ["中", "に"]),
      new("に入る", compounds: ["に", "入る"]),
      new("入れる"),
      // infinite recursion with recursive shadowed and yielding implementetion

      // <te-stem-required>
      new("て", "{continuing-action}", ["て"], tags: [VM.IsInflectingWord, VM.Requires.TeFormStemStart]),
      new("てる", tags: [VM.IsInflectingWord, VM.Requires.TeFormStemStart]),
      new("とる:progressive", tags: [VM.IsInflectingWord, VM.Requires.TeFormStemStart]),
      new("てん", tags: [VM.IsInflectingWord, VM.Requires.TeFormStemStart]),
      new("とん", tags: [VM.IsInflectingWord, VM.Requires.TeFormStemStart]),
      new("ている", "is-_-ing", readings: ["ている"], tags: [VM.IsInflectingWord, VM.Requires.TeFormStemStart]),
      new("てた", "{was}-{_-ing|_ed}", ["てた"], tags: [VM.IsInflectingWord, VM.Requires.TeFormStemStart]),
      new("てたら", "{was}-{_-ing|_ed}", ["てたら"], tags: [VM.IsInflectingWord, VM.Requires.TeFormStemStart]),

      new("んで", "and/て", forms: ["で"], prefixIn: ["ん"], tags: [VM.Requires.TeFormStemStart, V.QuestionOverridesForm]),
      new("んどる", forms: ["どる"], prefixIn: ["ん"], tags: [V.QuestionOverridesForm, VM.Requires.TeFormStemStart]),
      new("んでる", forms: ["でる"], prefixIn: ["ん"], tags: [V.QuestionOverridesForm, VM.Requires.TeFormStemStart]),
      // </te-stem-required>
      // <te-stem-forbidden>
      new("で", tags: [VM.Forbids.TeFormStemStart]),
      new("でいる", tags: [VM.Forbids.TeFormStemStart]),
      new("んで", "thing-is", tags: [VM.Forbids.TeFormStemStart]),
      new("とんだ", tags: [VM.Forbids.TeFormStemStart]),
      // </te-stem-forbidden>
      new("１人で", compounds: ["で", "１人"], tags: [VM.YieldLastTokenToOverlappingCompound]),
      new("ないで", compounds: ["ない", "で"], tags: [VM.YieldLastTokenToOverlappingCompound]),

      // <past-tense-required>
      new("た", "{past-tense} | (please)do", ["た"], surfaceNot: ["たら"], tags: [VM.IsInflectingWord, VM.Requires.PastTenseStem]),
      new("んだ:past", "did/was", forms: ["だ"], tags: [VM.Requires.PastTenseStem, VM.IsInflectingWord, V.QuestionOverridesForm]),
      // <past-tense-required>
      // <past-tense-forbidden>
      new("んだ:のだ", "thing-is", tags: [VM.Requires.Surface, VM.YieldLastTokenToOverlappingCompound, VM.Forbids.PastTenseStem]),
      new("たって", tags: [VM.Forbids.PastTenseStem]),
      new("だ", surfaceNot: ["なら", "な"], tags: [VM.IsInflectingWord, VM.Forbids.PastTenseStem]),
      // </past-tense-forbidden>

      new("たら", "conj{if/when} prt{as-for | why-not..  | I-said!/I-tell-you!}", ["たら"], tags: [VM.IsInflectingWord]),
      new("ちゃう", "to do: accidentally/unfortunately | completely", ["ちゃう"], tags: [VM.IsInflectingWord]),
      new("ても良い", "{concession/compromise} | {permission}", ["てもいい"], tags: [VM.IsInflectingWord]),
      new("すぎる", "too-much", ["すぎる"], tags: [VM.IsInflectingWord]),
      new("いらっしゃいます", "to: come/be/do", ["いらっしゃいます"]),
      new("を頼む", "I-entrust-to-you", ["を頼む"], tags: [VM.Requires.Surface]),
      new("作れる", "to-be-able: to-make", ["つくれる"], compounds: ["作る", "える"]),
      new("たい", "want to", ["たい"], tags: [VM.IsInflectingWord]),
      new("解放する", "to{} release", ["かいほうする"]),

      // require a stems
      new("あれる", "get-_/is-_", ["あれる"], forms: ["れる"], tags: [VM.Requires.Irrealis, VM.Requires.Godan, V.QuestionOverridesForm, VM.IsInflectingWord]),
      new("あせる", "get-_/is-_", ["あせる"], forms: ["せる"], tags: [VM.Requires.Irrealis, VM.Requires.Godan, V.QuestionOverridesForm, VM.IsInflectingWord]),

      new("する", "to: do", yieldToSurface: ["しろ"]),
      new("しろ", "do!", ["しろ"]),
      new("らっしゃる", yieldToSurface: ["らっしゃい"]),
      new("らっしゃい"),

      new("ぬ", "not", ["ぬ"], surfaceNot: ["ず"]),

      new("だの", "and-the-like", ["だの"], prefixNot: ["ん"]),

      new("こ", "familiarizing-suffix", ["こ"], forms: ["っこ"], tags: [VM.Forbids.SentenceStart]),

      new("ない", "not", forms: ["ない"], tags: [VM.IsInflectingWord]),
      new("無い", "not", forms: ["ない"], tags: [VM.IsInflectingWord]),
      new("うまい", yieldToSurface: ["うまく"]),
      new("うまく"),
      new("笑える", tos: [POS.IchidanVerb]),

      new("にする", "to: turn-into"),
      new("のか", tags: [VM.Requires.SentenceEnd]),
      new("ないと", tags: [VM.YieldLastTokenToOverlappingCompound]),
      new("して", tags: [VM.YieldLastTokenToOverlappingCompound]),
      new("ても"),
      new("と思う"),
      new("たの", tags: [VM.IsPoisonWord]),

      new("たかな", tags: [VM.IsPoisonWord]),
      new("たか", tags: [VM.IsPoisonWord]),
      new("なんて", tags: [VM.YieldLastTokenToOverlappingCompound]),
      new("何て", tags: [VM.YieldLastTokenToOverlappingCompound]),
      new("というか", forms: ["[と言うか]", "っていうか", "ていうか", "て言うか"]),
      new("ていうか", forms: ["と言うか", "というか", "っていうか", "[て言うか]"]),
      new("鰻", forms: ["[うな]"], prefixNot: ["ろ", "よ"]),
      new("書き"),
      new("風の強い", tags: [VM.Requires.Surface]),
      new("たね", tags: [VM.Requires.SingleToken]),
      new("たらしい", tags: [VM.Requires.SingleToken]),
      new("に決まる", forms: ["に決る", "に決まる", "に極る"]),
      new("に決まってる", forms: ["に決っている", "に決まっている", "に極っている", "に決ってる", "に決まってる", "に極ってる"]),
      new("された", surfaceNot: ["されたら"]),

      new("んです", tags: [VM.Requires.Surface, VM.YieldLastTokenToOverlappingCompound]),

      new("たん", forms: ["たの"], tags: [VM.Requires.SingleToken]),
      new("たの", forms: ["たん"], tags: [VM.YieldLastTokenToOverlappingCompound]),
      new("たんだ", tags: [VM.YieldLastTokenToOverlappingCompound]),
      new("んだろう", tags: [VM.IsPoisonWord]),
      new("しちゃう"),
      new("ものを", tags: [VM.Requires.SentenceEnd]),
      new("いいものを", forms: ["よいものを", "良いものを", "かったものを"], tags: [VM.Requires.SentenceEnd]),
      new("に行く", compounds: ["に", "行く"], tags: [VM.YieldLastTokenToOverlappingCompound]),
      new("行った", compounds: ["行く", "た"], tags: [VM.YieldLastTokenToOverlappingCompound]),

      new("うと", compounds: ["う", "と"], tags: [VM.YieldLastTokenToOverlappingCompound, VM.Forbids.DictionaryFormStart]),
      new("と思って", compounds: ["と思う", "て"], tags: [VM.YieldLastTokenToOverlappingCompound]),
      new("ませ", forms: ["まし"], suffixNot: ["ん"]),
      new("ところが", tags: [VM.Requires.SentenceStart]),

      new("成る", "to: become | result-in | turn-into", ["なる"], prefixNot: ["く"]),
      new("なる", "to: become | result-in | turn-into", ["なる"], prefixNot: ["く"]),
      new("くなる", "to: become", ["くなる"], forms: ["なる"], prefixIn: ["く"], tags: [V.QuestionOverridesForm]),
      new("言える", "to-be: able-to-say", compounds: ["言う", "える"]),
      new("出会える", "to: be-{able/fortunate-enough}-to-{meet/come-across}"),
      new("ていける", "can-go-on"),

      new("ても知らない", forms: ["ても知らん"], compounds: ["ても", "知る", "ん"], tags: [VM.Requires.TeFormStemStart]),

      new("とおり"),
      new("られる", tags: [VM.IsInflectingWord]),
      new("返せる", tos: [POS.IchidanVerb]), // exposes a bug in godan imperative detection
      new("外出中", tags: [V.IsCompositionallyTransparentCompound]),
      new("買い替える", tags: [V.IsCompositionallyTransparentCompound]),

      new("きれない", forms: ["[切れない]"], tags: [VM.Requires.MasuStem]),

      new("考えすぎ", tags: [VM.Forbids.AdverbHead]),
      new("考えすぎる", tags: [VM.Forbids.AdverbHead]),

      new("な", forms: ["な"]),
      new("な:dict", tags: [VM.Requires.DictionaryFormHead]),
      new("な:masu", tags: [VM.Requires.MasuStem]),
      new("な:s.end", tags: [VM.Requires.SentenceEnd, VM.Forbids.SentenceStart]),
      new("な:s.start", tags: [VM.Requires.SentenceStart]),
      new("すんな", forms: ["[すな]"], tags: [VM.Forbids.DictionaryFormStart]),

      new("がある", tags: [VM.YieldLastTokenToOverlappingCompound]),
      new("うの", forms: ["うの", "くの", "ぐの", "すの", "つの", "ぬの", "ぶの", "むの", "るの"], tags: [VM.Requires.DictionaryFormStart, V.QuestionOverridesForm]),

      new("寝れる", tags: [VM.IsPoisonWord]),
      new("れる:ichidan", tags: [VM.Requires.Ichidan, VM.Requires.Irrealis]),
      new("れない", tags: [VM.Requires.Ichidan, VM.Requires.Irrealis]),
      new("っ放し", forms: ["っはなし", "っぱなし"], tags: [VM.IsInflectingWord]),

      new("なぜかというと"),
      new("おうと", forms: ["うと"]),

      new("に会う", forms: ["にあう"]),

      new("よい", yieldToSurface: ["よく"]),
      new("よく", suffixNot: ["て"]),

      new("ないし", tags: [VM.Forbids.Irrealis, VM.Forbids.SentenceEnd, VM.Forbids.AdverbHead]),

      new("いただける:able-to", tags: [VM.Requires.TeFormStemHead]),
      new("いただける:acceptable", tags: [VM.Forbids.TeFormStemHead]),
      new("てない", tags: [VM.Requires.TeFormStemStart]),
      new("てしまいます", tags: [VM.Requires.TeFormStemStart]),

      new("う以上", tags: [VM.Requires.DictionaryFormStart]),
   ];
}
