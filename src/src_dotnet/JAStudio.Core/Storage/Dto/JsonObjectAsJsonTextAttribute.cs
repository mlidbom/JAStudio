using System.Text.Json.Nodes;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>Has MemoryPack store a DTO's <see cref="JsonObject"/> property as its JSON text, with <see cref="JsonObjectAsJsonTextFormatter"/>.</summary>
public sealed class JsonObjectAsJsonTextAttribute : MemoryPackCustomFormatterAttribute<JsonObjectAsJsonTextFormatter, JsonObject?>
{
   public override JsonObjectAsJsonTextFormatter GetFormatter() => new();
}
