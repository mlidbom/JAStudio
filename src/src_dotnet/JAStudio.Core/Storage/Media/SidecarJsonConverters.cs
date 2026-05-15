using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JAStudio.Core.Storage.Media;

sealed class SourceTagJsonConverter : JsonConverter<SourceTag>
{
   public override SourceTag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => SourceTag.Parse(reader.GetString()!);

   public override void Write(Utf8JsonWriter writer, SourceTag value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
