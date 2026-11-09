using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using JAStudio.Core.Note.KanjiReadings;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>
/// The values of <see cref="EditableFieldOfANote"/>s as the MCP server's tools take and report them, in JSON: texts, lists of texts, true or false,<br/>
/// a choice by its name in camel case, such as <c>"required"</c>, a vocab's forms as <c>{"text": ..., "isOwned": ...}</c>, and a kanji's<br/>
/// readings as <c>{"kana": ..., "isPrimary": ...}</c>.
/// </summary>
static class FieldValueJson
{
   static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) } };

   /// <summary><paramref name="value"/>, a field's value, as JSON.</summary>
   public static JsonNode? JsonOf(object value) => JsonSerializer.SerializeToNode(value, value.GetType(), Options);

   /// <summary>The value for <paramref name="field"/> that <paramref name="json"/> holds, or, when it holds none, a failure that says what the field takes.</summary>
   public static object ValueFor(EditableFieldOfANote field, JsonNode? json) =>
      json.Deserialize(field.ValueType, Options) ?? throw new ArgumentException($"The field '{field.Name}' takes {DescriptionOf(field.ValueType)}, not null.");

   /// <summary>What a field's value of the type <paramref name="valueType"/> is, in JSON, such as "a list of texts".</summary>
   public static string DescriptionOf(Type valueType) => valueType switch
   {
      _ when valueType == typeof(string)                      => "a text",
      _ when valueType == typeof(bool)                        => "true or false",
      _ when valueType == typeof(IReadOnlyList<string>)       => "a list of texts",
      _ when valueType == typeof(IReadOnlyList<EnteredForm>) => """a list of forms, each {"text": "...", "isOwned": true or false}""",
      _ when valueType == typeof(IReadOnlyList<KanjiReading>) => """a list of readings, each {"kana": "...", "isPrimary": true or false}""",
      { IsEnum: true }                                        => $"one of {string.Join(", ", Enum.GetNames(valueType).Select(it => $"\"{JsonNamingPolicy.CamelCase.ConvertName(it)}\""))}",
      _                                                       => throw new ArgumentOutOfRangeException(nameof(valueType), valueType, "No editable field has values of this type.")
   };
}
