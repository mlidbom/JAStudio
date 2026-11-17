using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.KanjiReadings;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Mcp.Tools.Editing.Fields;

/// <summary>
/// The values of <see cref="EditableFieldOfANote"/>s as the MCP server's tools take and report them, in JSON: texts, lists of texts, true or false,<br/>
/// a choice by its name in camel case, such as <c>"required"</c>, a vocab's forms as <c>{"text": ..., "isOwned": ...}</c>, a kanji's<br/>
/// readings as <c>{"kana": ..., "isPrimary": ...}</c>, a sentence's word exclusions as <c>{"word": ..., "index": ...}</c>, and a vocab's<br/>
/// related vocab as word-references, <c>{"text": ..., "disambiguation": ...}</c>.
/// </summary>
static class FieldValueJson
{
   static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false), new WordExclusionJsonConverter(), new WordReferenceJsonConverter() } };

   /// <summary>JSON as it was written, Japanese included, rather than escaped: for a failure to quote what it was given.</summary>
   static readonly JsonSerializerOptions AsWritten = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

   /// <summary><paramref name="value"/>, a field's value, as JSON.</summary>
   public static JsonNode? JsonOf(object value) => JsonSerializer.SerializeToNode(value, value.GetType(), Options);

   /// <summary>
   /// The value for <paramref name="field"/> that <paramref name="json"/> holds, or, when it holds none, or one of another shape, such as a text for<br/>
   /// a flag, a failure that names the field and says what it takes.
   /// </summary>
   public static object ValueFor(EditableFieldOfANote field, JsonNode? json)
   {
      try
      {
         return json.Deserialize(field.ValueType, Options) ?? throw new ArgumentException(WhatTheFieldTakes("null"));
      }
      catch(JsonException exception)
      {
         throw new ArgumentException(WhatTheFieldTakes(json?.ToJsonString(AsWritten) ?? "null"), exception);
      }

      string WhatTheFieldTakes(string givenInstead) => $"The field '{field.Name}' takes {DescriptionOf(field.ValueType)}, not {givenInstead}.";
   }

   /// <summary>What a field's value of the type <paramref name="valueType"/> is, in JSON, such as "a list of texts".</summary>
   public static string DescriptionOf(Type valueType) => valueType switch
   {
      _ when valueType == typeof(string)                      => "a text",
      _ when valueType == typeof(bool)                        => "true or false",
      _ when valueType == typeof(IReadOnlyList<string>)       => "a list of texts",
      _ when valueType == typeof(IReadOnlyList<EnteredForm>) => """a list of forms, each {"text": "...", "isOwned": true or false}""",
      _ when valueType == typeof(IReadOnlyList<KanjiReading>) => """a list of readings, each {"kana": "...", "isPrimary": true or false}""",
      _ when valueType == typeof(IReadOnlyList<WordReference>) => """a list of words, each {"text": "...", "disambiguation": "..."}, the disambiguation empty for none, or left out""",
      _ when valueType == typeof(IReadOnlyList<WordExclusion>) => """a list of word exclusions, each {"word": "...", "index": where in the sentence the word starts, or -1 for wherever it is}""",
      { IsEnum: true }                                        => $"one of {string.Join(", ", Enum.GetNames(valueType).Select(it => $"\"{JsonNamingPolicy.CamelCase.ConvertName(it)}\""))}",
      _                                                       => throw new ArgumentOutOfRangeException(nameof(valueType), valueType, "No editable field has values of this type.")
   };
}
