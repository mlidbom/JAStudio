using System.Collections.Generic;
using System.Text;

namespace JAStudio.Core.LanguageServices;

public static class HepburnKanaToRomaji
{
   // Digraphs (yōon) must be checked before single kana
   static readonly Dictionary<string, string> DigraphMap = new()
   {
      // Hiragana yōon
      ["きゃ"] = "kya", ["きゅ"] = "kyu", ["きょ"] = "kyo",
      ["しゃ"] = "sha", ["しゅ"] = "shu", ["しょ"] = "sho",
      ["ちゃ"] = "cha", ["ちゅ"] = "chu", ["ちょ"] = "cho",
      ["にゃ"] = "nya", ["にゅ"] = "nyu", ["にょ"] = "nyo",
      ["ひゃ"] = "hya", ["ひゅ"] = "hyu", ["ひょ"] = "hyo",
      ["みゃ"] = "mya", ["みゅ"] = "myu", ["みょ"] = "myo",
      ["りゃ"] = "rya", ["りゅ"] = "ryu", ["りょ"] = "ryo",
      ["ぎゃ"] = "gya", ["ぎゅ"] = "gyu", ["ぎょ"] = "gyo",
      ["じゃ"] = "ja",  ["じゅ"] = "ju",  ["じょ"] = "jo",
      ["びゃ"] = "bya", ["びゅ"] = "byu", ["びょ"] = "byo",
      ["ぴゃ"] = "pya", ["ぴゅ"] = "pyu", ["ぴょ"] = "pyo",
      ["ぢゃ"] = "ja",  ["ぢゅ"] = "ju",  ["ぢょ"] = "jo",

      // Katakana yōon
      ["キャ"] = "kya", ["キュ"] = "kyu", ["キョ"] = "kyo",
      ["シャ"] = "sha", ["シュ"] = "shu", ["ショ"] = "sho",
      ["チャ"] = "cha", ["チュ"] = "chu", ["チョ"] = "cho",
      ["ニャ"] = "nya", ["ニュ"] = "nyu", ["ニョ"] = "nyo",
      ["ヒャ"] = "hya", ["ヒュ"] = "hyu", ["ヒョ"] = "hyo",
      ["ミャ"] = "mya", ["ミュ"] = "myu", ["ミョ"] = "myo",
      ["リャ"] = "rya", ["リュ"] = "ryu", ["リョ"] = "ryo",
      ["ギャ"] = "gya", ["ギュ"] = "gyu", ["ギョ"] = "gyo",
      ["ジャ"] = "ja",  ["ジュ"] = "ju",  ["ジョ"] = "jo",
      ["ビャ"] = "bya", ["ビュ"] = "byu", ["ビョ"] = "byo",
      ["ピャ"] = "pya", ["ピュ"] = "pyu", ["ピョ"] = "pyo",
      ["ヂャ"] = "ja",  ["ヂュ"] = "ju",  ["ヂョ"] = "jo",

      // Extended katakana digraphs for loanwords
      ["ティ"] = "ti",  ["ディ"] = "di",
      ["ファ"] = "fa",  ["フィ"] = "fi",  ["フェ"] = "fe", ["フォ"] = "fo",
      ["ウィ"] = "wi",  ["ウェ"] = "we",  ["ウォ"] = "wo",
      ["ヴァ"] = "va",  ["ヴィ"] = "vi",  ["ヴェ"] = "ve", ["ヴォ"] = "vo",
      ["ツァ"] = "tsa", ["ツィ"] = "tsi", ["ツェ"] = "tse", ["ツォ"] = "tso",
      ["トゥ"] = "tu",  ["ドゥ"] = "du",
      ["シェ"] = "she", ["ジェ"] = "je",  ["チェ"] = "che",
   };

   static readonly Dictionary<char, string> SingleKanaMap = new()
   {
      // Hiragana
      ['あ'] = "a",  ['い'] = "i",  ['う'] = "u",  ['え'] = "e",  ['お'] = "o",
      ['か'] = "ka", ['き'] = "ki", ['く'] = "ku", ['け'] = "ke", ['こ'] = "ko",
      ['さ'] = "sa", ['し'] = "shi", ['す'] = "su", ['せ'] = "se", ['そ'] = "so",
      ['た'] = "ta", ['ち'] = "chi", ['つ'] = "tsu", ['て'] = "te", ['と'] = "to",
      ['な'] = "na", ['に'] = "ni", ['ぬ'] = "nu", ['ね'] = "ne", ['の'] = "no",
      ['は'] = "ha", ['ひ'] = "hi", ['ふ'] = "fu", ['へ'] = "he", ['ほ'] = "ho",
      ['ま'] = "ma", ['み'] = "mi", ['む'] = "mu", ['め'] = "me", ['も'] = "mo",
      ['や'] = "ya",               ['ゆ'] = "yu",                ['よ'] = "yo",
      ['ら'] = "ra", ['り'] = "ri", ['る'] = "ru", ['れ'] = "re", ['ろ'] = "ro",
      ['わ'] = "wa", ['ゐ'] = "wi",               ['ゑ'] = "we", ['を'] = "wo",
      ['ん'] = "n",
      // Dakuten
      ['が'] = "ga", ['ぎ'] = "gi", ['ぐ'] = "gu", ['げ'] = "ge", ['ご'] = "go",
      ['ざ'] = "za", ['じ'] = "ji", ['ず'] = "zu", ['ぜ'] = "ze", ['ぞ'] = "zo",
      ['だ'] = "da", ['ぢ'] = "ji", ['づ'] = "zu", ['で'] = "de", ['ど'] = "do",
      ['ば'] = "ba", ['び'] = "bi", ['ぶ'] = "bu", ['べ'] = "be", ['ぼ'] = "bo",
      // Handakuten
      ['ぱ'] = "pa", ['ぴ'] = "pi", ['ぷ'] = "pu", ['ぺ'] = "pe", ['ぽ'] = "po",
      // Small kana (used in loanwords, casual speech)
      ['ぁ'] = "a",  ['ぃ'] = "i",  ['ぅ'] = "u",  ['ぇ'] = "e",  ['ぉ'] = "o",
      ['ゃ'] = "ya",               ['ゅ'] = "yu",                ['ょ'] = "yo",
      // Vu
      ['ゔ'] = "vu",

      // Katakana
      ['ア'] = "a",  ['イ'] = "i",  ['ウ'] = "u",  ['エ'] = "e",  ['オ'] = "o",
      ['カ'] = "ka", ['キ'] = "ki", ['ク'] = "ku", ['ケ'] = "ke", ['コ'] = "ko",
      ['サ'] = "sa", ['シ'] = "shi", ['ス'] = "su", ['セ'] = "se", ['ソ'] = "so",
      ['タ'] = "ta", ['チ'] = "chi", ['ツ'] = "tsu", ['テ'] = "te", ['ト'] = "to",
      ['ナ'] = "na", ['ニ'] = "ni", ['ヌ'] = "nu", ['ネ'] = "ne", ['ノ'] = "no",
      ['ハ'] = "ha", ['ヒ'] = "hi", ['フ'] = "fu", ['ヘ'] = "he", ['ホ'] = "ho",
      ['マ'] = "ma", ['ミ'] = "mi", ['ム'] = "mu", ['メ'] = "me", ['モ'] = "mo",
      ['ヤ'] = "ya",               ['ユ'] = "yu",                ['ヨ'] = "yo",
      ['ラ'] = "ra", ['リ'] = "ri", ['ル'] = "ru", ['レ'] = "re", ['ロ'] = "ro",
      ['ワ'] = "wa", ['ヰ'] = "wi",               ['ヱ'] = "we", ['ヲ'] = "wo",
      ['ン'] = "n",
      // Dakuten
      ['ガ'] = "ga", ['ギ'] = "gi", ['グ'] = "gu", ['ゲ'] = "ge", ['ゴ'] = "go",
      ['ザ'] = "za", ['ジ'] = "ji", ['ズ'] = "zu", ['ゼ'] = "ze", ['ゾ'] = "zo",
      ['ダ'] = "da", ['ヂ'] = "ji", ['ヅ'] = "zu", ['デ'] = "de", ['ド'] = "do",
      ['バ'] = "ba", ['ビ'] = "bi", ['ブ'] = "bu", ['ベ'] = "be", ['ボ'] = "bo",
      // Handakuten
      ['パ'] = "pa", ['ピ'] = "pi", ['プ'] = "pu", ['ペ'] = "pe", ['ポ'] = "po",
      // Small kana (used in loanwords)
      ['ァ'] = "a",  ['ィ'] = "i",  ['ゥ'] = "u",  ['ェ'] = "e",  ['ォ'] = "o",
      ['ャ'] = "ya",               ['ュ'] = "yu",                ['ョ'] = "yo",
      // Vu
      ['ヴ'] = "vu",
   };

   static readonly HashSet<char> Vowels = ['a', 'i', 'u', 'e', 'o'];

   public static string Convert(string text)
   {
      if(string.IsNullOrEmpty(text))
         return string.Empty;

      var result = new StringBuilder(text.Length * 2);
      var i = 0;

      while(i < text.Length)
      {
         var current = text[i];

         // Sokuon (っ/ッ): double the next consonant
         if(current is 'っ' or 'ッ')
         {
            var nextRomaji = PeekNextRomaji(text, i + 1);
            if(nextRomaji != null && nextRomaji.Length > 0 && !Vowels.Contains(nextRomaji[0]))
            {
               result.Append(nextRomaji[0]);
            }
            // If next is a vowel or nothing follows, just skip the sokuon
            i++;
            continue;
         }

         // Chōonpu (ー): repeat the previous vowel
         if(current == 'ー')
         {
            if(result.Length > 0)
            {
               var lastChar = result[^1];
               if(Vowels.Contains(lastChar))
                  result.Append(lastChar);
            }
            i++;
            continue;
         }

         // Try digraph (two-character match)
         if(i + 1 < text.Length)
         {
            var pair = text.Substring(i, 2);
            if(DigraphMap.TryGetValue(pair, out var digraphRomaji))
            {
               result.Append(digraphRomaji);
               i += 2;
               continue;
            }
         }

         // Try single kana
         if(SingleKanaMap.TryGetValue(current, out var singleRomaji))
         {
            result.Append(singleRomaji);
            i++;
            continue;
         }

         // Not kana — pass through unchanged (spaces, kanji, punctuation, 〜, etc.)
         result.Append(current);
         i++;
      }

      return result.ToString();
   }

   static string? PeekNextRomaji(string text, int index)
   {
      if(index >= text.Length)
         return null;

      // Try digraph at the next position
      if(index + 1 < text.Length)
      {
         var pair = text.Substring(index, 2);
         if(DigraphMap.TryGetValue(pair, out var digraphRomaji))
            return digraphRomaji;
      }

      // Try single kana
      return SingleKanaMap.TryGetValue(text[index], out var singleRomaji) ? singleRomaji : null;
   }
}
