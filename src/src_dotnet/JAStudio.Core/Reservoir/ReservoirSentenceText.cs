using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace JAStudio.Core.Reservoir;

/// <summary>
/// The text of a sentence in the reservoir: what a source gave, cleaned of what the source wrapped it in. Decks wrap their sentences in HTML,<br/>
/// and subtitles in formatting and in brackets holding a speaker's name or a sound, such as （ユーリ）. Two sources giving one sentence give the<br/>
/// same text, which is how the reservoir knows a sentence it already has.
/// </summary>
/// <remarks>
/// What goes: HTML tags and entities, ruby readings, the invisible marks of writing direction, subtitle formatting such as <c>{\an8}</c>, short<br/>
/// bracketed asides, music notes and the dashes that start a line of dialogue. Half-width katakana become full-width, and every run of whitespace,<br/>
/// the ideographic space included, a single space.<br/>
/// The conventions of Japanese TV captions go too, as the kitsunekko backup's subtitles have them: the arrow continuing a line on the next, the<br/>
/// code of a character the caption couldn't show, such as <c>[外:0BE4F3…]</c>, a sound's source in square brackets, such as <c>[TEL]</c>, a<br/>
/// speaker's name before a colon, such as <c>お妙：</c>, and the marks around speech heard off screen or in thought, such as <c>((…))</c> and<br/>
/// <c>≪…≫</c>, whose speech stays.
/// </remarks>
public sealed partial record ReservoirSentenceText
{
   /// <summary>The longest a line of kanji with no kana is and still a sentence, such as 了解: a longer one is more likely Chinese.</summary>
   const int LongestSentenceOfKanjiAlone = 4;

   ReservoirSentenceText(string text) => Text = text;

   public string Text { get; }

   /// <summary>
   /// <paramref name="raw"/> cleaned, or null when it's no Japanese sentence: a line in English, a speaker's name alone, or a line in Chinese, as<br/>
   /// fansubs' credits are, which has kanji but no kana. A line of kanji alone is a sentence only when it's short, such as 了解.
   /// </summary>
   public static ReservoirSentenceText? CleanedFrom(string raw) => Cleaned(raw) is var cleaned && IsAJapaneseSentence(cleaned) ? new ReservoirSentenceText(cleaned) : null;

   /// <summary>The average length of <paramref name="texts"/>, in characters: 0 when there are none.</summary>
   public static double AverageLengthOf(IReadOnlyCollection<ReservoirSentenceText> texts) => texts.Count == 0 ? 0 : texts.Average(it => it.Text.Length);

   public override string ToString() => Text;

   static string Cleaned(string raw)
   {
      var text = WithoutHtml(raw);
      text = WithoutSubtitleFormatting(text);
      text = WithoutCaptionMarks(text);
      text = WithoutAsidesAndMusicNotes(text);
      text = WithoutSpeakersNameBeforeAColon(WithSingleSpaces(text));
      text = WithoutInvisibleMarks(text);
      text = WithFullWidthKatakana(text);
      return WithoutDialogueDash(WithSingleSpaces(text));

      static string WithoutHtml(string text) => WebUtility.HtmlDecode(Replaced(text, (RubyReading(), ""), (LineBreakTag(), " "), (AnyTag(), "")));
      static string WithoutSubtitleFormatting(string text) => Replaced(text, (SubtitleOverride(), ""), (SubtitleLineBreak(), " "));
      static string WithoutCaptionMarks(string text) => Replaced(text, (MissingCharacterCode(), ""), (ContinuationArrow(), " "), (SoundSource(), " "), (SpeechMark(), " "));
      static string WithoutAsidesAndMusicNotes(string text) => Replaced(text, (BracketedAside(), " "), (MusicNote(), " "), (EmptyBrackets(), " "));
      static string WithoutSpeakersNameBeforeAColon(string text) => SpeakersNameBeforeAColon().Replace(text, "");
      static string WithoutInvisibleMarks(string text) => InvisibleMark().Replace(text, "");
      static string WithFullWidthKatakana(string text) => HalfWidthKatakana().Replace(text, it => it.Value.Normalize(NormalizationForm.FormKC));
      static string WithSingleSpaces(string text) => Whitespace().Replace(text, " ").Trim();
      static string WithoutDialogueDash(string text) => DialogueDash().Replace(text, "");
   }

   /// <summary><paramref name="text"/> with each of <paramref name="replacements"/> made in turn: every match of its pattern replaced by its replacement.</summary>
   static string Replaced(string text, params (Regex Pattern, string Replacement)[] replacements) =>
      replacements.Aggregate(text, (replaced, it) => it.Pattern.Replace(replaced, it.Replacement));

   /// <summary>Whether <paramref name="cleaned"/> is a Japanese sentence: it has kana, or it's short and has kanji (<see cref="LongestSentenceOfKanjiAlone"/>).</summary>
   static bool IsAJapaneseSentence(string cleaned) => cleaned.Any(IsKana) || cleaned.Length <= LongestSentenceOfKanjiAlone && cleaned.Any(IsJapanese);

   /// <summary>
   /// Hiragana and katakana, full-width and half-width, but not the katakana middle dot ・, which isn't one of them, and which a foreign<br/>
   /// subtitle file misread as Shift JIS puts in its Latin text, as in J・esperava por isso.
   /// </summary>
   static bool IsKana(char character) => character is >= '\u3040' and <= '\u30FF' and not '\u30FB' or >= '\uFF66' and <= '\uFF9F';

   /// <summary>Hiragana, katakana, full-width and half-width, and the CJK ideographs, kanji among them.</summary>
   static bool IsJapanese(char character) => character is >= '\u3040' and <= '\u30FF' or >= '\u4E00' and <= '\u9FFF' or >= '\u3400' and <= '\u4DBF' or >= '\uFF66' and <= '\uFF9F';

   [GeneratedRegex(@"<(rt|rp)>.*?</\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline)] private static partial Regex RubyReading();
   [GeneratedRegex(@"<br\s*/?>|</div>|</p>", RegexOptions.IgnoreCase)] private static partial Regex LineBreakTag();
   [GeneratedRegex(@"<[^>]*>")] private static partial Regex AnyTag();
   [GeneratedRegex(@"\{[^}]*\}")] private static partial Regex SubtitleOverride();
   [GeneratedRegex(@"\\[Nnh]")] private static partial Regex SubtitleLineBreak();
   [GeneratedRegex(@"（[^（）]{1,15}）|\([^()]{1,15}\)|【[^【】]{1,15}】|〔[^〔〕]{1,15}〕")] private static partial Regex BracketedAside();
   [GeneratedRegex(@"[♪♫♬～〜]*[♪♫♬][～〜]*")] private static partial Regex MusicNote();
   [GeneratedRegex(@"[（(]\s*[）)]")] private static partial Regex EmptyBrackets();
   [GeneratedRegex(@"\[外:[0-9A-Fa-f]+\]|[\uE000-\uF8FF]")] private static partial Regex MissingCharacterCode();
   [GeneratedRegex(@"[→➡⇒]")] private static partial Regex ContinuationArrow();
   [GeneratedRegex(@"\[[^\[\]]{1,12}\]|［[^［］]{1,12}］")] private static partial Regex SoundSource();
   [GeneratedRegex(@"\(\(|\)\)|（（|））|[≪≫＜＞〈〉《》<>]")] private static partial Regex SpeechMark();
   [GeneratedRegex(@"^[^\s：:「」、。0-9０-９]{1,10}[：:]\s*")] private static partial Regex SpeakersNameBeforeAColon();
   [GeneratedRegex(@"[\u200B-\u200F\u202A-\u202E\u2066-\u2069\uFEFF]")] private static partial Regex InvisibleMark();
   [GeneratedRegex(@"[\uFF61-\uFF9F]+")] private static partial Regex HalfWidthKatakana();
   [GeneratedRegex(@"\s+")] private static partial Regex Whitespace();
   [GeneratedRegex(@"^[-－―–—]+\s*")] private static partial Regex DialogueDash();
}
