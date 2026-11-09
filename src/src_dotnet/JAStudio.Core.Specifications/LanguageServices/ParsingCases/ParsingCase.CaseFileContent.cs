using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Specifications.LanguageServices.ParsingCases.Breakdowns;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.SysUtils.Json;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

public sealed partial class ParsingCase
{
   /// <summary>
   /// What a parsing case's file holds, as JSON: indented, but each word on a line of its own, so that a change to a word shows in git's diff as<br/>
   /// that word's line, whole.
   /// </summary>
   sealed record CaseFileContent
   {
      static readonly JsonSerializerOptions JsonOptions = new()
                                                          {
                                                             PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                             DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                                             Encoder = FullUnicodeJsonEncoder.Instance
                                                          };

      public string Sentence { get; init; } = string.Empty;
      public string Why { get; init; } = string.Empty;
      public SentenceConfigDto? Configuration { get; init; }
      public List<ParsedWordReported> ParsedWords { get; init; } = [];
      public List<BreakdownUnderOptions> Breakdowns { get; init; } = [];

      public static CaseFileContent ReadFrom(FileInfo file) =>
         JsonSerializer.Deserialize<CaseFileContent>(File.ReadAllText(file.FullName), JsonOptions) ?? throw new JsonException($"The parsing case {file.FullName} is empty.");

      public void WriteTo(FileInfo file) => File.WriteAllText(file.FullName, ToJson());

      string ToJson()
      {
         using var json = new MemoryStream();
         using(var writer = new Utf8JsonWriter(json, new JsonWriterOptions { Indented = true, NewLine = "\n", Encoder = FullUnicodeJsonEncoder.Instance }))
         {
            writer.WriteStartObject();
            writer.WriteString(JsonPropertyNameOf(nameof(Sentence)), Sentence);
            writer.WriteString(JsonPropertyNameOf(nameof(Why)), Why);
            if(Configuration != null)
            {
               writer.WritePropertyName(JsonPropertyNameOf(nameof(Configuration)));
               JsonSerializer.Serialize(writer, Configuration, JsonOptions);
            }

            WriteEachOnALineOfItsOwn(JsonPropertyNameOf(nameof(ParsedWords)), ParsedWords, writer);

            writer.WriteStartArray(JsonPropertyNameOf(nameof(Breakdowns)));
            foreach(var breakdown in Breakdowns) WriteTheBreakdown(breakdown, writer);
            writer.WriteEndArray();

            writer.WriteEndObject();
         }

         return Encoding.UTF8.GetString(json.ToArray()) + "\n";
      }

      /// <summary>Writes <paramref name="breakdown"/> as an item of the array <paramref name="writer"/> is writing: its options on one line, then each of its words on one.</summary>
      static void WriteTheBreakdown(BreakdownUnderOptions breakdown, Utf8JsonWriter writer)
      {
         writer.WriteStartObject();
         writer.WritePropertyName(JsonPropertyNameOf(nameof(BreakdownUnderOptions.Options)));
         writer.WriteRawValue(JsonSerializer.Serialize(breakdown.Options, JsonOptions));
         WriteEachOnALineOfItsOwn(JsonPropertyNameOf(nameof(BreakdownUnderOptions.DisplayedWords)), breakdown.DisplayedWords, writer);
         writer.WriteEndObject();
      }

      /// <summary>
      /// Writes <paramref name="items"/> as the array <paramref name="propertyName"/>, each item on a line of its own. A JSON writer lays out a<br/>
      /// value written raw not at all, so each item's JSON brings its own line break and indentation.
      /// </summary>
      static void WriteEachOnALineOfItsOwn<TItem>(string propertyName, IEnumerable<TItem> items, Utf8JsonWriter writer)
      {
         writer.WriteStartArray(propertyName);
         var indentation = new string(' ', 2 * writer.CurrentDepth);
         foreach(var item in items) writer.WriteRawValue($"\n{indentation}{JsonSerializer.Serialize(item, JsonOptions)}");
         writer.WriteEndArray();
      }

      static string JsonPropertyNameOf(string property) => JsonNamingPolicy.CamelCase.ConvertName(property);
   }
}
