using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Client;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway;

/// <summary>
/// The gateway's connection to the MCP server of the JAStudio it serves, headless or live, and that server's tools, which the gateway offers Claude<br/>
/// Code as its own for as long as the connection lasts, forwarding each call over it (<see cref="ToolOfTheJAStudio"/>).
/// </summary>
sealed class ConnectionToTheJAStudio : IAsyncDisposable
{
   readonly McpClient _mcpServerConnection;

   /// <summary>The JAStudio's tools, as the gateway offers them.</summary>
   readonly IReadOnlyList<McpServerTool> _tools;

   readonly McpServerPrimitiveCollection<McpServerTool> _toolsOfferedToClaudeCode;

   ConnectionToTheJAStudio(McpClient mcpServerConnection, IReadOnlyList<McpServerTool> tools, McpServerPrimitiveCollection<McpServerTool> toolsOfferedToClaudeCode)
   {
      _mcpServerConnection = mcpServerConnection;
      _tools = tools;
      _toolsOfferedToClaudeCode = toolsOfferedToClaudeCode;
   }

   /// <summary>The names of the JAStudio's tools, which the gateway offers, for the gateway's reports.</summary>
   public string NamesOfTheTools => string.Join(", ", _tools.Select(it => it.ProtocolTool.Name));

   /// <summary>
   /// Connects to the JAStudio's MCP server at <paramref name="mcpServerUrl"/>, and offers its tools in <paramref name="toolsOfferedToClaudeCode"/>,<br/>
   /// which tells Claude Code that the gateway's tools changed.
   /// </summary>
   public static async Task<ConnectionToTheJAStudio> OpenAsync(Uri mcpServerUrl, McpServerPrimitiveCollection<McpServerTool> toolsOfferedToClaudeCode, CancellationToken cancellationToken)
   {
      var mcpServerConnection = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions { Endpoint = mcpServerUrl }), cancellationToken: cancellationToken);
      IReadOnlyList<McpServerTool> tools = [.. (await mcpServerConnection.ListToolsAsync(cancellationToken: cancellationToken)).Select(it => new ToolOfTheJAStudio(it.ProtocolTool, mcpServerConnection))];

      var connection = new ConnectionToTheJAStudio(mcpServerConnection, tools, toolsOfferedToClaudeCode);
      connection.OfferTheTools();
      return connection;
   }

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
