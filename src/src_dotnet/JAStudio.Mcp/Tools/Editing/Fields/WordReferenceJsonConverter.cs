using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Mcp.Tools.Editing.Fields;

/// <summary>
/// A <see cref="WordReference"/> in JSON as the corpus stores it: <c>{"text": "...", "disambiguation": "..."}</c>, the disambiguation empty for none.<br/>
/// Read, the disambiguation may be left out, for none.
/// </summary>
sealed class WordReferenceJsonConverter : JsonConverter<WordReference>
{
   public override WordReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
   {
      var reference = JsonSerializer.Deserialize<WordReferenceJson>(ref reader, options) ?? throw new JsonException("A word-reference can't be null.");
      return new WordReference(reference.Text ?? throw new JsonException("""A word-reference must have a "text"."""), reference.Disambiguation ?? "");
   }

   public override void Write(Utf8JsonWriter writer, WordReference value, JsonSerializerOptions options) =>
      JsonSerializer.Serialize(writer, new WordReferenceJson(value.Text, value.Disambiguation), options);

   /// <summary>A <see cref="WordReference"/>'s JSON, as read and written.</summary>
   record WordReferenceJson(string? Text, string? Disambiguation);
}
