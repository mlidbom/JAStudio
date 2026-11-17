using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Mcp.Tools.Editing.Fields;

/// <summary>
/// A <see cref="WordReference"/> in JSON as its name (<see cref="WordReference.DisambiguationName"/>): its text, and after a colon its disambiguation<br/>
/// if it has one, such as <c>"入る:masu"</c> or <c>"走る"</c>. That's how a person types a word in the workspace's editors, and how the fields of a<br/>
/// single word, such as a vocab's derived-from word and a kanji's primary vocab, take it as text: so every field of words takes them alike.
/// </summary>
sealed class WordReferenceJsonConverter : JsonConverter<WordReference>
{
   public override WordReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
   {
      if(reader.TokenType != JsonTokenType.String) throw new JsonException($"A word is its name, as a text, not {reader.TokenType}.");

      var name = reader.GetString() ?? throw new JsonException("A word's name can't be null.");
      return WordReference.FromDisambiguationNameOrNone(name) ?? throw new JsonException($"\"{name}\" can't be a word's name.");
   }

   public override void Write(Utf8JsonWriter writer, WordReference value, JsonSerializerOptions options) => writer.WriteStringValue(value.DisambiguationName);
}
