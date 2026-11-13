using System.Threading.Tasks;
using JAStudio.Mcp.Discovery;
using JAStudio.Mcp.Gateway.Connections;
using JAStudio.Mcp.Gateway.Headless;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace JAStudio.Mcp.Gateway;

/// <summary>
/// The MCP gateway: the MCP server Claude Code starts, over stdio, <c>jastudio</c> in <c>.mcp.json</c>, for working with JAStudios. It connects to<br/>
/// any of the JAStudios running on this computer, several at once, and offers their tools as its own, each under its connection's name, forwarding<br/>
/// their calls to them (<see cref="ConnectedJAStudios"/>). Its own tools list the running JAStudios and connect to them (<see cref="ConnectionTools"/>),<br/>
/// and start and stop a headless JAStudio of its own (<see cref="HeadlessJAStudioTools"/>). What it's made of is in <see cref="GatewayServices"/>.
/// </summary>
/// <remarks>
/// It exists because Claude Code can't reconnect to an MCP server by itself: a server that wasn't running when the session started, or that<br/>
/// restarted, needs Magnus to reconnect it. The gateway runs for the whole session, so the JAStudios behind it can be restarted at will, to take in<br/>
/// a new build, and Claude Code learns of their tools through the gateway's tool list changing. The design is in <c>dev_docs/jastudio-mcp.md</c>.
/// <br/><br/>
/// Claude Code starts it through <c>start-the-gateway.ps1</c>, which builds it into a folder of its own for each start and runs it from there, so that<br/>
/// building JAStudio, in Rider or for the Definition of Done, never needs to replace a file a running gateway has loaded. Of JAStudio's assemblies it<br/>
/// references only <c>JAStudio.Mcp.Discovery</c>, the running JAStudios, which it shares with them, so that what changes with JAStudio lives in the<br/>
/// JAStudios behind it, which restart at will, not in the gateway.
/// </remarks>
static class Program
{
   public static async Task Main(string[] args)
   {
      var builder = Host.CreateApplicationBuilder(args);
      builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace); // standard output is the stdio connection to Claude Code

      builder.Services.AddTheGateway(SourceCheckout.In(builder.Configuration["source-checkout"]), RunningJAStudios.OfThisUser)
             .WithStdioServerTransport();

      await builder.Build().RunAsync();
   }
}
