using System.Text.Json.Nodes;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>One field of a note that an edit or a command changed, by its name, with its value before and after.</summary>
public record FieldChange(string Field, JsonNode? Before, JsonNode? After);
