using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;

namespace JAStudio.Core.Reservoir.Sources.Subtitles;

/// <summary>
/// A subtitle file, .srt, .ass or .ssa, as the sentences it shows: each line of Japanese, with the time it's shown at as its position. Lines<br/>
/// split over several rows of text are one sentence. The file's encoding is read from its byte order mark, or is UTF-8, or else Shift JIS.
/// </summary>
public sealed partial class SubtitleFile
{
   static readonly string[] Extensions = [".srt", ".ass", ".ssa"];
   static readonly Encoding ShiftJis = ShiftJisEncoding();

   readonly string _text;

   public SubtitleFile(string name, byte[] content)
   {
      Name = name;
      _text = Decoded(content);
   }

   /// <summary>The file's name, without the folders it's in.</summary>
   public string Name { get; }

   /// <summary>Whether <paramref name="fileName"/> names a subtitle file of a format this reads.</summary>
   public static bool IsOne(string fileName) => Extensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());

   /// <summary>The sentences the file shows, each line that's Japanese, read as they're enumerated.</summary>
   public IEnumerable<SourceSentence> Sentences() => ShownLines().Select(SentenceOrNone).OfType<SourceSentence>();

   /// <summary>The sentence <paramref name="line"/> shows, cleaned, or none when it's no Japanese sentence.</summary>
   SourceSentence? SentenceOrNone(ShownLine line) =>
      ReservoirSentenceText.CleanedFrom(line.Text) is {} text ? new SourceSentence(text, $"{Name} {line.StartTime}", Translation: null, AudioFile: null, ImageFile: null) : null;

   IEnumerable<ShownLine> ShownLines() => Path.GetExtension(Name).ToLowerInvariant() == ".srt" ? SrtLines() : AssLines();

   /// <summary>An .srt file's blocks: a number, a row with the times, such as <c>00:01:02,345 --> 00:01:04,000</c>, and the rows of text.</summary>
   IEnumerable<ShownLine> SrtLines()
   {
      string? startTime = null;
      var rows = new List<string>();
      foreach(var row in _text.Split('\n').Select(it => it.Trim()).Append(""))
      {
         if(row.Contains("-->", StringComparison.Ordinal))
         {
            startTime = row[..row.IndexOf("-->", StringComparison.Ordinal)].Trim();
         } else if(row.Length == 0)
         {
            if(startTime is not null && rows.Count > 0) yield return new ShownLine(startTime, string.Join(" ", rows));
            startTime = null;
            rows.Clear();
         } else if(startTime is not null)
         {
            rows.Add(row);
         }
      }
   }

   /// <summary>An .ass or .ssa file's <c>Dialogue:</c> rows, whose tenth comma-separated field, the last, is the text, and the second the start time.</summary>
   IEnumerable<ShownLine> AssLines() =>
      _text.Split('\n')
           .Where(it => it.StartsWith("Dialogue:", StringComparison.Ordinal))
           .Select(it => it.Split(',', 10))
           .Where(it => it.Length == 10)
           .Select(it => new ShownLine(it[1].Trim(), it[9]));

   static string Decoded(byte[] content)
   {
      if(content is [0xEF, 0xBB, 0xBF, ..]) return Encoding.UTF8.GetString(content, 3, content.Length - 3);
      if(content is [0xFF, 0xFE, ..]) return Encoding.Unicode.GetString(content, 2, content.Length - 2);
      if(content is [0xFE, 0xFF, ..]) return Encoding.BigEndianUnicode.GetString(content, 2, content.Length - 2);
      return Utf8.IsValid(content) ? Encoding.UTF8.GetString(content) : ShiftJis.GetString(content);
   }

   static Encoding ShiftJisEncoding()
   {
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      return Encoding.GetEncoding(932);
   }
}
