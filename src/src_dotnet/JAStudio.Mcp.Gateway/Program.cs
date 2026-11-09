using System;
using System.Threading.Tasks;
using JAStudio.Mcp.Gateway.Headless;
using JAStudio.Mcp.Gateway.Live;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace JAStudio.Mcp.Gateway;

/// <summary>
/// The MCP gateway: the MCP server Claude Code starts, over stdio, for working with a JAStudio, and which offers that JAStudio's tools as its own,<br/>
/// forwarding their calls to it. Claude Code starts one gateway for each of the two JAStudios, chosen by its <c>--jastudio</c> argument:
/// <list type="bullet">
///   <item><c>headless</c>, <c>jastudio-headless</c> in <c>.mcp.json</c>: its own tools start and stop the headless JAStudio (<see cref="HeadlessJAStudio"/>).</item>
///   <item><c>live</c>, <c>jastudio-live</c>: its own tool connects to the live JAStudio, the one inside Anki (<see cref="LiveJAStudio"/>).</item>
/// </list>
/// </summary>
/// <remarks>
/// It exists because Claude Code can't reconnect to an MCP server by itself: a server that wasn't running when the session started, or that<br/>
/// restarted, needs Magnus to reconnect it. The gateway runs for the whole session, so the JAStudio behind it can be restarted at will, to take in<br/>
/// a new build, and Claude Code learns of its tools through the gateway's tool list changing. The design is in <c>dev_docs/jastudio-mcp.md</c>.
/// <br/><br/>
/// Claude Code starts it through <c>start-the-gateway.ps1</c>, which builds it into a folder of its own for each start and runs it from there, so that<br/>
/// building JAStudio, in Rider or for the Definition of Done, never needs to replace a file a running gateway has loaded. It references no JAStudio<br/>
/// assembly, so that what changes with JAStudio lives in the JAStudios behind it, which restart at will, not in the gateway.
/// </remarks>
static class Program
{
   public static async Task Main(string[] args)
   {
      var builder = Host.CreateApplicationBuilder(args);
      builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace); // standard output is the stdio connection to Claude Code

      var gateway = builder.Services.AddMcpServer(options => options.ServerInfo = new Implementation { Name = "JAStudio MCP gateway", Version = "1" })
                           .WithStdioServerTransport()
                           .WithRequestFilters(filters => filters.AddCallToolFilter(ToolExceptionReporting.ReportsTheWholeException));

      switch(builder.Configuration["jastudio"])
      {
         case "headless":
            builder.Services.AddSingleton(SourceCheckout.In(builder.Configuration["source-checkout"]));
            builder.Services.AddSingleton<HeadlessJAStudio>();
            gateway.WithTools<HeadlessJAStudioTools>();
            break;
         case "live":
            builder.Services.AddSingleton<LiveJAStudio>();
            gateway.WithTools<LiveJAStudioTools>();
            break;
         case var other:
            throw new ArgumentException($"The gateway serves the tools of the headless or the live JAStudio, chosen by --jastudio headless or --jastudio live, not '{other}'.");
      }

      await builder.Build().RunAsync();
   }
}
