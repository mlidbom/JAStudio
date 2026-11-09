using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs.Properties;

/// <summary>A list in a note that items were added to or removed from, such as a vocab's forms or a note's tags, with those items, as the corpus stores them.</summary>
/// <param name="Property">The list's path in the note as the corpus stores it, such as <c>partsOfSpeech</c>.</param>
public record ListChange(string Property, IReadOnlyList<JsonNode?> Added, IReadOnlyList<JsonNode?> Removed);
