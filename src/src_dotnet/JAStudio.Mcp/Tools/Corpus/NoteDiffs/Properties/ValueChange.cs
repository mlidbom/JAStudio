using System.Text.Json.Nodes;

namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs.Properties;

/// <summary>
/// A property of a note whose value changed, with its value before and after, as the corpus stores it. A list whose items are the same, but in<br/>
/// another order, is reported as a value that changed too.
/// </summary>
/// <param name="Property">The property's path in the note as the corpus stores it, such as <c>matching.flags.isPoisonWord</c>.</param>
/// <param name="Before">The value before. Null when the note didn't have the property.</param>
/// <param name="After">The value after. Null when the note no longer has the property.</param>
public record ValueChange(string Property, JsonNode? Before, JsonNode? After);
