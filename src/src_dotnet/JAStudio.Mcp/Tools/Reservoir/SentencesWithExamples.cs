using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Reservoir;

/// <summary>How many sentences, and a few of them as examples.</summary>
public sealed record SentencesWithExamples(int Count, IReadOnlyList<string> Examples);
