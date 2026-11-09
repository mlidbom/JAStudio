namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs.Properties;

/// <summary>A property, by its path as the corpus stores notes, such as <c>forms</c>, and how many of the notes a diff compares it changed in.</summary>
public record PropertyChangeCount(string Property, int Notes);
