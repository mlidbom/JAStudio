using Compze.DependencyInjection.Abstractions;
using JAStudio.Mcp.Discovery;

namespace JAStudio.Mcp.Live;

/// <summary>
/// JAStudio's MCP server as a live JAStudio runs it, one with a person working in it, inside Anki or standalone: started as the JAStudio starts,<br/>
/// and announced among the <see cref="RunningJAStudios"/> for as long as it runs, for Claude's MCP gateway to find and connect to.
/// </summary>
/// <remarks>
/// It starts before the collection has loaded, so that the tools that diagnose a load that fails are there when it does, such as<br/>
/// reload_the_collection and list_changed_notes. Until a load has succeeded, the tools that read the notes answer with why there are none to use<br/>
/// yet (<see cref="ToolsThatReadTheNotes"/>).
/// </remarks>
public sealed class LiveJAStudioMcpServer
{
   readonly JAStudioMcpServer _server;

   LiveJAStudioMcpServer(JAStudioMcpServer server) => _server = server;

   /// <summary>
   /// Starts the server of <paramref name="jastudio"/>, on a port the system picks, and announces it as the MCP server of a JAStudio hosted by<br/>
   /// <paramref name="host"/>: Anki, or the standalone host.
   /// </summary>
   public static LiveJAStudioMcpServer Start(IRootResolver jastudio, JAStudioHost host)
   {
      var server = JAStudioMcpServer.Start(jastudio, port: 0);
      server.AnnounceAs(host);
      return new LiveJAStudioMcpServer(server);
   }

   /// <summary>Stops the server, withdrawing its announcement: as the JAStudio stops.</summary>
   public void Stop() => _server.StopAsync().GetAwaiter().GetResult();
}
