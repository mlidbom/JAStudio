using System.Net;
using System.Text.RegularExpressions;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages;

/// <summary>
/// What Anki writes into a note's field besides its text: HTML, a reference to an audio file, such as <c>[sound:show_01.mp3]</c>, an image, such<br/>
/// as <c>&lt;img src="show_01.jpg"&gt;</c>, and furigana, such as <c>漢字[かんじ]</c>.
/// </summary>
static partial class AnkiFieldMarkup
{
   /// <summary>Whether <paramref name="field"/> refers to an audio file.</summary>
   public static bool HoldsASound(string field) => SoundReference().IsMatch(field);

   /// <summary>Whether <paramref name="field"/> holds an image.</summary>
   public static bool HoldsAnImage(string field) => ImageReference().IsMatch(field);

   /// <summary>Whether <paramref name="field"/> holds furigana: a reading in brackets right after the text it's of.</summary>
   public static bool HoldsFurigana(string field) => Furigana().IsMatch(field);

   /// <summary>
   /// <paramref name="field"/> with its furigana gone, the text they're readings of kept: Anki's <c>友人帳[ゆうじんちょう]を 寄越[よこ]せ</c> is<br/>
   /// <c>友人帳を寄越せ</c>, and Migaku's <c>ちょっと[;o] &amp;ensp; 急[いそ,いそぐ;k2]い でる[,でる;k1] から</c> is <c>ちょっと&amp;ensp;急いでるから</c>.
   /// </summary>
   /// <remarks>
   /// Anki's furigana take the space before the text they're a reading of, which only separates that text from the text before it. Migaku's,<br/>
   /// which give a word's dictionary form and pitch accent too, after a comma and a semicolon, come with a space between every two words, and<br/>
   /// the sentence's own spaces written as <c>&amp;ensp;</c>, so every plain space goes with them.
   /// </remarks>
   public static string TextWithoutFurigana(string field) =>
      MigakuFurigana().IsMatch(field) ? AnyFurigana().Replace(field, "").Replace(" ", "", System.StringComparison.Ordinal) : AnkiFurigana().Replace(field, "$1");

   /// <summary>The name of the audio file <paramref name="field"/> refers to first, or none when it refers to none.</summary>
   public static string? SoundFileIn(string field) => SoundReference().Match(field) is { Success: true } sound ? sound.Groups[1].Value : null;

   /// <summary>The name of the image file of the first image <paramref name="field"/> holds, or none when it holds none.</summary>
   public static string? ImageFileIn(string field) => ImageReference().Match(field) is { Success: true } image ? image.Groups[1].Value : null;

   /// <summary><paramref name="field"/>'s text, its HTML tags gone and its entities their characters, or none when there's no text.</summary>
   public static string? TextIn(string field) => AnyTag().Replace(field, " ").Trim() is { Length: > 0 } text ? WebUtility.HtmlDecode(text) : null;

   [GeneratedRegex(@"\[sound:([^\]]+)\]")] private static partial Regex SoundReference();
   [GeneratedRegex(@"<img[^>]*?src\s*=\s*[""']?([^""'>]+)", RegexOptions.IgnoreCase)] private static partial Regex ImageReference();
   [GeneratedRegex(@"\S\[[^\]\s]+\]")] private static partial Regex Furigana();
   [GeneratedRegex(@" ?([^ >]+?)\[([^\]]+)\]")] private static partial Regex AnkiFurigana();
   [GeneratedRegex(@"\S\[[^\]\s]*[,;][^\]\s]*\]")] private static partial Regex MigakuFurigana();
   [GeneratedRegex(@"\[[^\]\s]*\]")] private static partial Regex AnyFurigana();
   [GeneratedRegex(@"<[^>]*>")] private static partial Regex AnyTag();
}
