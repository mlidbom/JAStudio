using JAStudio.Core.Reservoir;
using JAStudio.Core.Reservoir.Feedback;

namespace JAStudio.Mcp.Tools.Reservoir;

/// <summary>A reservoir the MCP server's tools have opened (<see cref="OpenReservoirs"/>): its file, and its sentences' texts, loaded in memory.</summary>
public sealed record OpenReservoir(ReservoirFile File, ReservoirSentenceTexts Texts);
