using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace JAStudio.Mcp.Gateway;

/// <summary>
/// The MCP gateway: the MCP server Claude Code starts, over stdio, for working with JAStudio. Its own tools start and stop the headless JAStudio,<br/>
/// and while that runs, the gateway offers its tools as its own and forwards their calls to it.
/// </summary>
/// <remarks>
/// It exists because Claude Code can't reconnect to an MCP server by itself: a server that wasn't running when the session started, or that<br/>
/// restarted, needs Magnus to reconnect it. The gateway runs for the whole session, so the headless JAStudio behind it can be restarted at will, to<br/>
/// take in a new build, and Claude Code learns of its tools through the gateway's tool list changing. The design is in <c>dev_docs/jastudio-mcp.md</c>.
/// <br/><br/>
/// It references no JAStudio assembly: it runs from its own build output, and building JAStudio while it runs must never need to replace a file it<br/>
/// has loaded.
/// </remarks>
static class Program
{
   public static async Task Main(string[] args)
   {
      var builder = Host.CreateApplicationBuilder(args);
      builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace); // standard output is the stdio connection to Claude Code

      builder.Services.AddSingleton(SourceCheckout.TheGatewaysOwn());
      builder.Services.AddSingleton<HeadlessJAStudio>();
      builder.Services.AddMcpServer(options => options.ServerInfo = new Implementation { Name = "JAStudio MCP gateway", Version = "1" })
             .WithStdioServerTransport()
             .WithRequestFilters(filters => filters.AddCallToolFilter(ToolExceptionReporting.ReportsTheWholeException))
             .WithTools<HeadlessJAStudioTools>();

      await builder.Build().RunAsync();
   }
}
