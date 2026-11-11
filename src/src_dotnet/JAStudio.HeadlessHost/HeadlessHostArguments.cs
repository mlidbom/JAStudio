using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace JAStudio.HeadlessHost;

/// <summary>
/// What the headless JAStudio is started with to serve JAStudio's MCP server: what it loads (<see cref="HeadlessJAStudioSetup"/>), where its MCP<br/>
/// server listens, and which process it lives as long as. The MCP gateway starts it with all of them, as <c>--name value</c> pairs.
/// </summary>
/// <remarks>Every argument is read and checked when the headless JAStudio starts, so a missing or wrong one fails it before it loads anything.</remarks>
sealed class HeadlessHostArguments
{
   HeadlessHostArguments(CommandLineValues values)
   {
      Setup = HeadlessJAStudioSetup.From(values);
      McpServerPort = int.Parse(values.Value("mcp-server-port"), CultureInfo.InvariantCulture);
      ProcessToExitWith = Process.GetProcessById(int.Parse(values.Value("exit-with-process"), CultureInfo.InvariantCulture));
   }

   public static HeadlessHostArguments From(IReadOnlyList<string> args) => new(CommandLineValues.From(args));

   /// <summary>What the headless JAStudio loads.</summary>
   public HeadlessJAStudioSetup Setup { get; }

   /// <summary>The port of this computer's loopback address that the MCP server listens on, or 0 for a free port the system picks.</summary>
   public int McpServerPort { get; }

   /// <summary>The process the headless JAStudio exits with, the MCP gateway that started it, so that it never outlives it.</summary>
   public Process ProcessToExitWith { get; }
}
