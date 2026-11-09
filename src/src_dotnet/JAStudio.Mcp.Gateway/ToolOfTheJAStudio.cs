using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway;

/// <summary>
/// A tool of the MCP server of the JAStudio the gateway serves, headless or live, which the gateway offers Claude Code as its own, and forwards<br/>
/// each call of to that JAStudio (<see cref="ConnectionToTheJAStudio"/>).
/// </summary>
sealed class ToolOfTheJAStudio : McpServerTool
{
   readonly McpClient _mcpServerConnection;

   /// <param name="tool">The tool as the JAStudio's MCP server describes it.</param>
   /// <param name="mcpServerConnection">The gateway's connection to the JAStudio's MCP server, which each call is forwarded over.</param>
   public ToolOfTheJAStudio(Tool tool, McpClient mcpServerConnection)
   {
      ProtocolTool = tool;
      _mcpServerConnection = mcpServerConnection;
   }

   public override Tool ProtocolTool { get; }

   public override IReadOnlyList<object> Metadata => [];

   public override ValueTask<CallToolResult> InvokeAsync(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken = default) =>
      _mcpServerConnection.CallToolAsync(request.Params ?? throw new ArgumentException("A tool call without its parameters.", nameof(request)), cancellationToken);
}
