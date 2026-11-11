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
   /// <summary>How every line ends, on every platform, so that a deck kept anywhere gives the same bytes.</summary>
   const string LineEnd = "\n";

   static readonly JsonSerializerOptions LineOptions = new()
                                                       {
                                                          PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                          Encoder = FullUnicodeJsonEncoder.Instance,
                                                          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                                          NewLine = LineEnd
                                                       };

   static readonly JsonSerializerOptions DocumentOptions = new(LineOptions) { WriteIndented = true };

   /// <summary>Writes <paramref name="value"/> to <paramref name="file"/>, indented.</summary>
   public static void WriteDocument<T>(FileInfo file, T value) => File.WriteAllText(file.FullName, JsonSerializer.Serialize(value, DocumentOptions));

   /// <summary>The value <paramref name="file"/>, a document written by <see cref="WriteDocument{T}"/>, holds.</summary>
   public static T Read<T>(FileInfo file) =>
      JsonSerializer.Deserialize<T>(File.ReadAllText(file.FullName), DocumentOptions) ?? throw new InvalidDataException($"{file.FullName} holds JSON null.");

   /// <summary>Writes <paramref name="values"/> to <paramref name="file"/>, each on a line of its own.</summary>
   public static void WriteLines<T>(FileInfo file, IEnumerable<T> values)
   {
      File.WriteAllText(file.FullName, string.Concat(values.Select(it => JsonSerializer.Serialize(it, LineOptions) + LineEnd)));
   }
}
