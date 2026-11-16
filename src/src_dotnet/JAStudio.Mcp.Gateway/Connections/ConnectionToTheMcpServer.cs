using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JAStudio.Mcp.Discovery;
using ModelContextProtocol.Client;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway.Connections;

/// <summary>
/// The gateway's connection to the MCP server of a run of a JAStudio, and that server's tools, which the gateway offers Claude Code as its own, named<br/>
/// after the connection (<see cref="JAStudioTool"/>), for as long as the connection lasts, forwarding each call over it.
/// </summary>
sealed class ConnectionToTheMcpServer : IAsyncDisposable
{
   readonly McpClient _mcpServerConnection;

   /// <summary>The JAStudio's tools, as the gateway offers them.</summary>
   readonly IReadOnlyList<JAStudioTool> _tools;

   readonly McpServerPrimitiveCollection<McpServerTool> _toolsOfferedToClaudeCode;

   ConnectionToTheMcpServer(RunningJAStudio jastudio, McpClient mcpServerConnection, IReadOnlyList<JAStudioTool> tools, McpServerPrimitiveCollection<McpServerTool> toolsOfferedToClaudeCode)
   {
      JAStudio = jastudio;
      _mcpServerConnection = mcpServerConnection;
      _tools = tools;
      _toolsOfferedToClaudeCode = toolsOfferedToClaudeCode;
   }

   /// <summary>The run of the JAStudio whose MCP server this is connected to.</summary>
   public RunningJAStudio JAStudio { get; }

   /// <summary>The names the JAStudio's tools have in the JAStudio, for the gateway's reports.</summary>
   public IEnumerable<string> NamesOfTheTools => _tools.Select(it => it.NameInTheJAStudio);

   /// <summary>
   /// Connects to the MCP server of <paramref name="jastudio"/>, and offers its tools in <paramref name="toolsOfferedToClaudeCode"/>, named after the<br/>
   /// connection <paramref name="connection"/>: changing the collection tells Claude Code that the gateway's tools changed.
   /// </summary>
   public static async Task<ConnectionToTheMcpServer> OpenAsync(RunningJAStudio jastudio,
                                                                ConnectionName connection,
                                                                McpServerPrimitiveCollection<McpServerTool> toolsOfferedToClaudeCode,
                                                                CancellationToken cancellationToken)
   {
      var mcpServerConnection = await ClientConnectedTo(jastudio.McpServer, cancellationToken);
      IReadOnlyList<JAStudioTool> tools = [.. (await mcpServerConnection.ListToolsAsync(cancellationToken: cancellationToken)).Select(it => new JAStudioTool(it.ProtocolTool, connection, mcpServerConnection))];

      var connectionToTheMcpServer = new ConnectionToTheMcpServer(jastudio, mcpServerConnection, tools, toolsOfferedToClaudeCode);
      connectionToTheMcpServer.OfferTheTools();
      return connectionToTheMcpServer;
   }

   /// <summary>
   /// An MCP client connected to the MCP server of a JAStudio at <paramref name="mcpServer"/>. It waits for the server's first answer for as long as<br/>
   /// connecting may take, without the MCP SDK's separate time limit of 5 seconds on it.
   /// </summary>
   /// <remarks>
   /// By default the SDK's client gives the server 5 seconds to answer its first request, <c>server/discover</c>, then takes it for a server of an<br/>
   /// older revision of the protocol, and falls back to that revision's handshake (<c>McpClientOptions.DiscoverProbeTimeout</c>). A JAStudio's<br/>
   /// server speaks the current revision, so its clients need no fallback, and the fallback has failed against it: on 2026-11-15 specifications<br/>
   /// that took 7 seconds to connect, right after a build, failed in the fallback's handshake with "The MCP-Protocol-Version header value<br/>
   /// '2026-07-28' does not match body params.protocolVersion value '2025-11-25'". A limit of milliseconds didn't reproduce it, so what more than<br/>
   /// the limit passing it takes isn't known.
   /// </remarks>
   internal static Task<McpClient> ClientConnectedTo(Uri mcpServer, CancellationToken cancellationToken) =>
      McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions { Endpoint = mcpServer }),
                            new McpClientOptions { DiscoverProbeTimeout = Timeout.InfiniteTimeSpan },
                            cancellationToken: cancellationToken);

   void OfferTheTools()
   {
      using var _ = _toolsOfferedToClaudeCode.DeferChangedEvents();
      foreach(var tool in _tools) _toolsOfferedToClaudeCode.Add(tool);
   }

   void WithdrawTheTools()
   {
      using var _ = _toolsOfferedToClaudeCode.DeferChangedEvents();
      foreach(var tool in _tools) _toolsOfferedToClaudeCode.Remove(tool);
   }

   /// <summary>Ends the connection: withdraws the tools, and closes the connection to the JAStudio's MCP server.</summary>
   public async ValueTask DisposeAsync()
   {
      WithdrawTheTools();
      await _mcpServerConnection.DisposeAsync();
   }
}
