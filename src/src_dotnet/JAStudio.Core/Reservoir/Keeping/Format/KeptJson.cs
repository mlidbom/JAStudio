using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using JAStudio.Core.SysUtils.Json;

namespace JAStudio.Core.Reservoir.Keeping.Format;

/// <summary>
/// How the kept files are written as JSON: their properties in camelCase, every character as itself rather than escaped<br/>
/// (<see cref="FullUnicodeJsonEncoder"/>), and no property whose value is null. A document, such as <c>deck.json</c>, is indented; a file of lines,<br/>
/// <c>notes.jsonl</c>, holds a value on each line.
/// </summary>
static class KeptJson
{
   static readonly JsonSerializerOptions LineOptions = new()
                                                       {
                                                          PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                          Encoder = FullUnicodeJsonEncoder.Instance,
                                                          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                                                       };

   static readonly JsonSerializerOptions DocumentOptions = new(LineOptions) { WriteIndented = true };

   /// <summary>Writes <paramref name="value"/> to <paramref name="file"/>, indented.</summary>
   public static void WriteDocument<T>(FileInfo file, T value) => File.WriteAllText(file.FullName, JsonSerializer.Serialize(value, DocumentOptions));

   /// <summary>Writes <paramref name="values"/> to <paramref name="file"/>, each on a line of its own.</summary>
   public static void WriteLines<T>(FileInfo file, IEnumerable<T> values)
   {
      //review: the lines end as the platform's do, CRLF on Windows and LF elsewhere, and so do an indented document's, as
      //   JsonSerializerOptions.NewLine is Environment.NewLine unless set: the same deck kept on two platforms gives different bytes, in a format
      //   meant never to change. Writing LF on every platform would settle it.
      File.WriteAllLines(file.FullName, values.Select(it => JsonSerializer.Serialize(it, LineOptions)));
   }
}
