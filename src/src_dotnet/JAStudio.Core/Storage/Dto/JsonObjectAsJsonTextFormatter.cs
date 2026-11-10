using System.Buffers;
using System.Text.Json.Nodes;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>
/// Stores a <see cref="JsonObject"/> in MemoryPack's binary format as its JSON text, since MemoryPack can't take JSON nodes apart itself. A DTO's<br/>
/// property uses it through <see cref="JsonObjectAsJsonTextAttribute"/>.
/// </summary>
public sealed class JsonObjectAsJsonTextFormatter : MemoryPackFormatter<JsonObject?>
{
   public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref JsonObject? value) => writer.WriteString(value?.ToJsonString());

   public override void Deserialize(ref MemoryPackReader reader, scoped ref JsonObject? value) => value = reader.ReadString() is {} text ? JsonNode.Parse(text)?.AsObject() : null;
}
