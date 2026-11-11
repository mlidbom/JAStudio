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
/// the ideographic space included, a single space.
/// </remarks>
public sealed partial record ReservoirSentenceText
{
   ReservoirSentenceText(string text) => Text = text;

   public string Text { get; }

   /// <summary><paramref name="raw"/> cleaned, or null when no Japanese is left, such as for a line in English, or a speaker's name alone.</summary>
   public static ReservoirSentenceText? CleanedFrom(string raw)
   {
      var cleaned = Cleaned(raw);
      return cleaned.Any(IsJapanese) ? new ReservoirSentenceText(cleaned) : null;
   }

   public override string ToString() => Text;

   static string Cleaned(string raw)
   {
      var text = WithoutHtml(raw);
      text = WithoutSubtitleFormatting(text);
      text = WithoutAsidesAndMusicNotes(text);
      text = WithoutInvisibleMarks(text);
      text = WithFullWidthKatakana(text);
      return WithoutDialogueDash(WithSingleSpaces(text));

      static string WithoutHtml(string text) => WebUtility.HtmlDecode(AnyTag().Replace(LineBreakTag().Replace(RubyReading().Replace(text, ""), " "), ""));
      static string WithoutSubtitleFormatting(string text) => SubtitleLineBreak().Replace(SubtitleOverride().Replace(text, ""), " ");
      static string WithoutAsidesAndMusicNotes(string text) => MusicNote().Replace(BracketedAside().Replace(text, " "), " ");
      static string WithoutInvisibleMarks(string text) => InvisibleMark().Replace(text, "");
      static string WithFullWidthKatakana(string text) => HalfWidthKatakana().Replace(text, it => it.Value.Normalize(NormalizationForm.FormKC));
      static string WithSingleSpaces(string text) => Whitespace().Replace(text, " ").Trim();
      static string WithoutDialogueDash(string text) => DialogueDash().Replace(text, "");
   }

   /// <summary>Hiragana, katakana, full-width and half-width, and the CJK ideographs, kanji among them.</summary>
   static bool IsJapanese(char character) => character is >= '\u3040' and <= '\u30FF' or >= '\u4E00' and <= '\u9FFF' or >= '\u3400' and <= '\u4DBF' or >= '\uFF66' and <= '\uFF9F';

   [GeneratedRegex(@"<(rt|rp)>.*?</\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline)] private static partial Regex RubyReading();
   [GeneratedRegex(@"<br\s*/?>|</div>|</p>", RegexOptions.IgnoreCase)] private static partial Regex LineBreakTag();
   [GeneratedRegex(@"<[^>]*>")] private static partial Regex AnyTag();
   [GeneratedRegex(@"\{[^}]*\}")] private static partial Regex SubtitleOverride();
   [GeneratedRegex(@"\\[Nnh]")] private static partial Regex SubtitleLineBreak();
   [GeneratedRegex(@"（[^（）]{1,15}）|\([^()]{1,15}\)|【[^【】]{1,15}】|〔[^〔〕]{1,15}〕")] private static partial Regex BracketedAside();
   [GeneratedRegex(@"[♪♫♬～〜]*[♪♫♬][～〜]*")] private static partial Regex MusicNote();
   [GeneratedRegex(@"[\u200B-\u200F\u202A-\u202E\u2066-\u2069\uFEFF]")] private static partial Regex InvisibleMark();
   [GeneratedRegex(@"[\uFF61-\uFF9F]+")] private static partial Regex HalfWidthKatakana();
   [GeneratedRegex(@"\s+")] private static partial Regex Whitespace();
   [GeneratedRegex(@"^[-－―–—]+\s*")] private static partial Regex DialogueDash();
}
