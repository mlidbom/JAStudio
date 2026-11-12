using System;

namespace JAStudio.Mcp.Tools.Memory;

/// <summary>
/// Megabytes as every tool of the MCP server reports them: millions of bytes, as <c>dotnet-dump</c> counts them, rounded to two decimals.<br/>
/// Not the 1024 &#215; 1024 bytes Windows's Task Manager shows as one, which is 4.9% more.
/// </summary>
public static class Megabytes
{
   /// <summary>The megabytes that <paramref name="bytes"/> bytes make.</summary>
   public static double Of(long bytes) => Math.Round(bytes / 1_000_000.0, 2);
}
