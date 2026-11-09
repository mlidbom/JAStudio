using System.Text.Json.Nodes;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>One field <see cref="NoteEditingTools.EditANote"/> wrote, by its name, with its value before and after the edit.</summary>
public record FieldChange(string Field, JsonNode? Before, JsonNode? After);
