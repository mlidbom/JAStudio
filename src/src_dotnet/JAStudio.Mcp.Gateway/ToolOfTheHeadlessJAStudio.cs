using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway;

/// <summary>A tool of the headless JAStudio's MCP server, which the gateway offers Claude Code as its own, and forwards each call of to the headless JAStudio.</summary>
sealed class ToolOfTheHeadlessJAStudio : McpServerTool
{
   readonly McpClient _mcpServerConnection;

   /// <param name="tool">The tool as the headless JAStudio's MCP server describes it.</param>
   /// <param name="mcpServerConnection">The gateway's connection to the headless JAStudio's MCP server, which each call is forwarded over.</param>
   public ToolOfTheHeadlessJAStudio(Tool tool, McpClient mcpServerConnection)
   {
      ProtocolTool = tool;
      _mcpServerConnection = mcpServerConnection;
   }

   public override Tool ProtocolTool { get; }

   public override IReadOnlyList<object> Metadata => [];

   public override ValueTask<CallToolResult> InvokeAsync(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken = default) =>
      _mcpServerConnection.CallToolAsync(request.Params ?? throw new ArgumentException("A tool call without its parameters.", nameof(request)), cancellationToken);
}
