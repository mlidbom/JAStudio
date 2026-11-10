using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using JAStudio.Core.Note.Media;

namespace JAStudio.Core.Anki;

/// <summary>
/// Anki's markup for media in a note's fields: <c>[sound:文句_b.mp3]</c> for audio, <c>&lt;img src="kv_right_img.jpg" alt="…"&gt;</c> for an image.<br/>
/// JAStudio's notes hold references to their files (<see cref="AudioReference"/>, <see cref="ImageReference"/>) rather than markup, so the markup is read<br/>
/// only where Anki's fields come into JAStudio, the import, and written only where they go out, the Anki-projection (<see cref="Note.JPNote.StudyNoteFields"/>).
/// </summary>
/// <remarks>
/// What a field holds besides the media, such as the <c>,</c>, <c>&lt;br&gt;</c> and <c>&amp;nbsp;</c> between files, and an image's height, isn't kept:<br/>
/// it was of no value (Magnus, 2026-11-10). An image's alt text is kept, decoded from HTML, and it may hold <c>&lt;</c> and <c>&gt;</c> within its quotes.
/// </remarks>
public static partial class AnkiMediaMarkup
{
   /// <summary>The audio files <paramref name="field"/> references, each as <c>[sound:…]</c>, in the order it has them. Nothing is known about any of them.</summary>
   public static IReadOnlyList<AudioReference> AudioIn(string field) =>
   [
      ..SoundTag().Matches(field)
                  .Select(it => it.Groups["fileName"].Value.Trim())
                  .Where(it => it != "")
                  .Select(it => new AudioReference(it))
   ];

   /// <summary>The image files <paramref name="field"/> references, each as an <c>&lt;img&gt;</c> tag, in the order it has them, with their alt texts.</summary>
   public static IReadOnlyList<ImageReference> ImagesIn(string field) =>
      [..ImgTag().Matches(field).Select(it => ImageDescribedBy(it.Groups["attributes"].Value)).OfType<ImageReference>()];

   /// <summary><paramref name="audio"/> as Anki's markup: a <c>[sound:…]</c> for each file, one after the other.</summary>
   public static string SoundTagsFor(IEnumerable<AudioReference> audio) => string.Concat(audio.Select(it => $"[sound:{it.FileName}]"));

   /// <summary><paramref name="images"/> as Anki's markup: an <c>&lt;img&gt;</c> tag for each file, with its alt text when it has one.</summary>
   public static string ImgTagsFor(IEnumerable<ImageReference> images) => string.Concat(images.Select(ImgTagFor));

   static string ImgTagFor(ImageReference image) =>
      image.AltText is {} altText
         ? $"<img src=\"{AsAttributeValue(image.FileName)}\" alt=\"{AsAttributeValue(altText)}\">"
         : $"<img src=\"{AsAttributeValue(image.FileName)}\">";

   /// <summary><paramref name="text"/> as the value of a double-quoted HTML attribute: with the characters that would end or break it as their entities.</summary>
   static string AsAttributeValue(string text) => text.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;");

   /// <summary>
   /// The image an <c>&lt;img&gt;</c> tag's <paramref name="attributes"/> describe, or null when they have no <c>src</c>. An attribute given twice has<br/>
   /// its first value, as in HTML.
   /// </summary>
   static ImageReference? ImageDescribedBy(string attributes)
   {
      var attributeValuesByName = Attribute().Matches(attributes)
                                             .DistinctBy(it => it.Groups["name"].Value.ToLowerInvariant())
                                             .ToDictionary(it => it.Groups["name"].Value.ToLowerInvariant(), it => WebUtility.HtmlDecode(it.Groups["value"].Value));
      return attributeValuesByName.GetValueOrDefault("src")?.Trim() is { Length: > 0 } fileName
                ? new ImageReference(fileName) { AltText = attributeValuesByName.GetValueOrDefault("alt") }
                : null;
   }

   [GeneratedRegex(@"\[sound:(?<fileName>[^\]]+)\]")]
   private static partial Regex SoundTag();

   /// <summary>An <c>&lt;img&gt;</c> tag, whose attributes' quoted values may hold <c>&gt;</c>.</summary>
   [GeneratedRegex("""<img\b(?<attributes>(?:[^>"']|"[^"]*"|'[^']*')*)>""", RegexOptions.IgnoreCase)]
   private static partial Regex ImgTag();

   /// <summary>One attribute of a tag, its value double-quoted, single-quoted or bare.</summary>
   [GeneratedRegex("""(?<name>[\w-]+)\s*=\s*(?:"(?<value>[^"]*)"|'(?<value>[^']*)'|(?<value>[^\s"'>]+))""")]
   private static partial Regex Attribute();
}
