using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;

namespace JAStudio.Mcp.Tools.Editing.Fields;

/// <summary>
/// A <see cref="WordExclusion"/> in JSON as the corpus stores it: <c>{"word": "...", "index": ...}</c>, where the index is where in the sentence the<br/>
/// excluded word starts, or -1 for wherever it is. Read, the index may be left out, for wherever.
/// </summary>
sealed class WordExclusionJsonConverter : JsonConverter<WordExclusion>
{
   const int Anywhere = -1;

   public override WordExclusion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
   {
      var exclusion = JsonSerializer.Deserialize<WordExclusionJson>(ref reader, options) ?? throw new JsonException("A word exclusion can't be null.");
      return WordExclusion.AtIndex(exclusion.Word ?? throw new JsonException("""A word exclusion must have a "word"."""), exclusion.Index ?? Anywhere);
   }

   public override void Write(Utf8JsonWriter writer, WordExclusion value, JsonSerializerOptions options) =>
      JsonSerializer.Serialize(writer, new WordExclusionJson(value.Word, value.Index), options);

   /// <summary>A <see cref="WordExclusion"/>'s JSON, as read and written.</summary>
   record WordExclusionJson(string? Word, int? Index);
}
