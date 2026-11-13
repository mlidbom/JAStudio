using System;
using JAStudio.Mcp.Discovery;
using JAStudio.Mcp.Gateway.Connections;
using JAStudio.Mcp.Gateway.Headless;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway;

/// <summary>What the gateway is made of: its MCP server's tools, and the services behind them. How it's served, over stdio, is up to its host (<see cref="Program"/>).</summary>
static class GatewayServices
{
   extension(IServiceCollection @this)
   {
      /// <summary>
      /// Adds the gateway, running in <paramref name="sourceCheckout"/>, which finds the running JAStudios with <paramref name="openTheRunningJAStudios"/>:<br/>
      /// those of this user, or, in specifications, those announced apart from them.
      /// </summary>
      /// <returns>The gateway's MCP server, for its host to choose how it's served.</returns>
      internal IMcpServerBuilder AddTheGateway(SourceCheckout sourceCheckout, Func<RunningJAStudios> openTheRunningJAStudios)
      {
         var mcpServer = @this.AddMcpServer(options => options.ServerInfo = new Implementation { Name = "JAStudio MCP gateway", Version = "1" })
                              .WithRequestFilters(filters => filters.AddCallToolFilter(ToolExceptionReporting.ReportsTheWholeException))
                              .WithTools<ConnectionTools>()
                              .WithTools<HeadlessJAStudioTools>();

         @this.AddSingleton(sourceCheckout);
         @this.AddSingleton(_ => openTheRunningJAStudios());
         @this.AddSingleton(services => new ConnectedJAStudios(services.GetRequiredService<RunningJAStudios>(), TheGatewaysTools(services), sourceCheckout.CorpusRepository));
         @this.AddSingleton<HeadlessJAStudio>();
         return mcpServer;
      }
   }

   /// <summary>The gateway's tools, which the connected JAStudios' tools are offered among.</summary>
   internal static McpServerPrimitiveCollection<McpServerTool> TheGatewaysTools(IServiceProvider services) =>
      services.GetRequiredService<IOptions<McpServerOptions>>().Value.ToolCollection
      ?? throw new InvalidOperationException("The gateway's MCP server has no tool collection to offer the JAStudios' tools in.");
}
