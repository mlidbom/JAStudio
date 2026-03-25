using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JAStudio.Core.LanguageServices;

public static class HepburnRomajiToKana
{
   // Sorted by length descending so longer matches are tried first (e.g. "sha" before "sh")
   static readonly List<(string Romaji, string Hiragana, string Katakana)> Mappings = BuildMappings();

   static readonly int MaxRomajiLength = Mappings.Max(m => m.Romaji.Length);

   public static string ToHiragana(string romaji)
   {
      if(string.IsNullOrEmpty(romaji))
         return string.Empty;

      return ConvertRomaji(romaji, hiragana: true);
   }

   public static string ToKatakana(string romaji)
   {
      if(string.IsNullOrEmpty(romaji))
         return string.Empty;

      return ConvertRomaji(romaji, hiragana: false);
   }

   static string ConvertRomaji(string input, bool hiragana)
   {
      var result = new StringBuilder(input.Length);
      var lower = input.ToLowerInvariant();
      var i = 0;

      while(i < lower.Length)
      {
         // Handle double consonant (sokuon): "tt" → っt, "kk" → っk, etc.
         // But NOT "nn" — that's handled as n + n-row kana
         if(i + 1 < lower.Length
            && lower[i] == lower[i + 1]
            && lower[i] != 'n'
            && !IsVowel(lower[i])
            && char.IsAsciiLetter(lower[i]))
         {
            result.Append(hiragana ? 'っ' : 'ッ');
            i++;
            continue;
         }

         // Special handling for 'n': it's ん only when NOT followed by a vowel or 'y'
         // (because na/ni/nu/ne/no/nya/nyu/nyo are their own kana)
         if(lower[i] == 'n')
         {
            var nextChar = i + 1 < lower.Length ? lower[i + 1] : '\0';
            var isNKana = nextChar == '\0'
                          || (!IsVowel(nextChar) && nextChar != 'y' && nextChar != 'n');

            if(isNKana)
            {
               result.Append(hiragana ? 'ん' : 'ン');
               i++;
               continue;
            }

            // "nn" → ん (explicit double-n)
            if(nextChar == 'n')
            {
               // Check if the second 'n' starts a new n-row kana
               var charAfterNn = i + 2 < lower.Length ? lower[i + 2] : '\0';
               if(IsVowel(charAfterNn) || charAfterNn == 'y')
               {
                  // nn + vowel: first n = ん, second n starts n-row kana
                  result.Append(hiragana ? 'ん' : 'ン');
                  i++;
                  continue;
               }

               // nn at end or before consonant: ん
               result.Append(hiragana ? 'ん' : 'ン');
               i += 2;
               continue;
            }

            // n + vowel or n + y: fall through to normal matching (na, ni, nya, etc.)
         }

         // Try longest match first
         var matched = false;
         var maxLen = System.Math.Min(MaxRomajiLength, lower.Length - i);
         for(var len = maxLen; len >= 1; len--)
         {
            var substring = lower.Substring(i, len);
            var mapping = FindMapping(substring);
            if(mapping != null)
            {
               result.Append(hiragana ? mapping.Value.Hiragana : mapping.Value.Katakana);
               i += len;
               matched = true;
               break;
            }
         }

         if(!matched)
         {
            result.Append(input[i]);
            i++;
         }
      }

      return result.ToString();
   }

   static bool IsVowel(char c) => c is 'a' or 'i' or 'u' or 'e' or 'o';

   static (string Romaji, string Hiragana, string Katakana)? FindMapping(string romaji)
   {
      foreach(var mapping in Mappings)
      {
         if(mapping.Romaji == romaji)
            return mapping;
      }
      return null;
   }

   static List<(string Romaji, string Hiragana, string Katakana)> BuildMappings()
   {
      var mappings = new List<(string, string, string)>
      {
         // Yōon digraphs
         ("kya", "きゃ", "キャ"), ("kyu", "きゅ", "キュ"), ("kyo", "きょ", "キョ"),
         ("sha", "しゃ", "シャ"), ("shu", "しゅ", "シュ"), ("sho", "しょ", "ショ"),
         ("cha", "ちゃ", "チャ"), ("chu", "ちゅ", "チュ"), ("cho", "ちょ", "チョ"),
         ("nya", "にゃ", "ニャ"), ("nyu", "にゅ", "ニュ"), ("nyo", "にょ", "ニョ"),
         ("hya", "ひゃ", "ヒャ"), ("hyu", "ひゅ", "ヒュ"), ("hyo", "ひょ", "ヒョ"),
         ("mya", "みゃ", "ミャ"), ("myu", "みゅ", "ミュ"), ("myo", "みょ", "ミョ"),
         ("rya", "りゃ", "リャ"), ("ryu", "りゅ", "リュ"), ("ryo", "りょ", "リョ"),
         ("gya", "ぎゃ", "ギャ"), ("gyu", "ぎゅ", "ギュ"), ("gyo", "ぎょ", "ギョ"),
         ("bya", "びゃ", "ビャ"), ("byu", "びゅ", "ビュ"), ("byo", "びょ", "ビョ"),
         ("pya", "ぴゃ", "ピャ"), ("pyu", "ぴゅ", "ピュ"), ("pyo", "ぴょ", "ピョ"),
         ("shi", "し", "シ"),
         ("chi", "ち", "チ"),
         ("tsu", "つ", "ツ"),

         // Multi-char consonant clusters
         ("ja", "じゃ", "ジャ"), ("ju", "じゅ", "ジュ"), ("jo", "じょ", "ジョ"),
         ("fu", "ふ", "フ"),

         // Basic kana
         ("ka", "か", "カ"), ("ki", "き", "キ"), ("ku", "く", "ク"), ("ke", "け", "ケ"), ("ko", "こ", "コ"),
         ("sa", "さ", "サ"), ("si", "し", "シ"), ("su", "す", "ス"), ("se", "せ", "セ"), ("so", "そ", "ソ"),
         ("ta", "た", "タ"), ("ti", "ち", "チ"), ("tu", "つ", "ツ"), ("te", "て", "テ"), ("to", "と", "ト"),
         ("na", "な", "ナ"), ("ni", "に", "ニ"), ("nu", "ぬ", "ヌ"), ("ne", "ね", "ネ"), ("no", "の", "ノ"),
         ("ha", "は", "ハ"), ("hi", "ひ", "ヒ"), ("hu", "ふ", "フ"), ("he", "へ", "ヘ"), ("ho", "ほ", "ホ"),
         ("ma", "ま", "マ"), ("mi", "み", "ミ"), ("mu", "む", "ム"), ("me", "め", "メ"), ("mo", "も", "モ"),
         ("ya", "や", "ヤ"), ("yu", "ゆ", "ユ"), ("yo", "よ", "ヨ"),
         ("ra", "ら", "ラ"), ("ri", "り", "リ"), ("ru", "る", "ル"), ("re", "れ", "レ"), ("ro", "ろ", "ロ"),
         ("wa", "わ", "ワ"), ("wi", "ゐ", "ヰ"), ("we", "ゑ", "ヱ"), ("wo", "を", "ヲ"),
         ("nn", "ん", "ン"),
         // Dakuten
         ("ga", "が", "ガ"), ("gi", "ぎ", "ギ"), ("gu", "ぐ", "グ"), ("ge", "げ", "ゲ"), ("go", "ご", "ゴ"),
         ("za", "ざ", "ザ"), ("ji", "じ", "ジ"), ("zu", "ず", "ズ"), ("ze", "ぜ", "ゼ"), ("zo", "ぞ", "ゾ"),
         ("da", "だ", "ダ"), ("di", "ぢ", "ヂ"), ("du", "づ", "ヅ"), ("de", "で", "デ"), ("do", "ど", "ド"),
         ("ba", "ば", "バ"), ("bi", "び", "ビ"), ("bu", "ぶ", "ブ"), ("be", "べ", "ベ"), ("bo", "ぼ", "ボ"),
         // Handakuten
         ("pa", "ぱ", "パ"), ("pi", "ぴ", "ピ"), ("pu", "ぷ", "プ"), ("pe", "ぺ", "ペ"), ("po", "ぽ", "ポ"),

         // Single vowels
         ("a", "あ", "ア"), ("i", "い", "イ"), ("u", "う", "ウ"), ("e", "え", "エ"), ("o", "お", "オ"),
      };

      // Sort by romaji length descending so longest matches win
      mappings.Sort((a, b) => b.Item1.Length.CompareTo(a.Item1.Length));
      return mappings;
   }
}
