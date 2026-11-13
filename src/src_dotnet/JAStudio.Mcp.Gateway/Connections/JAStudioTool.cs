using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway.Connections;

/// <summary>
/// A tool of the MCP server of a JAStudio the gateway is connected to, which the gateway offers Claude Code as its own, named after the connection,<br/>
/// such as <c>anki-main__search_notes</c>, and forwards each call of to that JAStudio (<see cref="ConnectionToTheMcpServer"/>).
/// </summary>
sealed class JAStudioTool : McpServerTool
{
   readonly McpClient _mcpServerConnection;

   /// <param name="tool">The tool as the JAStudio's MCP server describes it, which this takes over, renamed.</param>
   /// <param name="connection">The name of the gateway's connection to the JAStudio, which the tool is offered under.</param>
   /// <param name="mcpServerConnection">The gateway's connection to the JAStudio's MCP server, which each call is forwarded over.</param>
   public JAStudioTool(Tool tool, ConnectionName connection, McpClient mcpServerConnection)
   {
      NameInTheJAStudio = tool.Name;
      _mcpServerConnection = mcpServerConnection;
      tool.Name = connection.OfItsTool(tool.Name);
      ProtocolTool = tool;
   }

   public override Tool ProtocolTool { get; }

   /// <summary>The tool's name in the JAStudio, such as <c>search_notes</c>, which calls of it are forwarded as.</summary>
   public string NameInTheJAStudio { get; }

   public override IReadOnlyList<object> Metadata => [];

   public override ValueTask<CallToolResult> InvokeAsync(RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken = default) =>
      CallInTheJAStudioAsync(request.Params ?? throw new ArgumentException("A tool call without its parameters.", nameof(request)), cancellationToken);

   /// <summary>Calls the tool in the JAStudio, with the arguments of <paramref name="call"/>, a call of it as the gateway offers it.</summary>
   public ValueTask<CallToolResult> CallInTheJAStudioAsync(CallToolRequestParams call, CancellationToken cancellationToken) =>
      _mcpServerConnection.CallToolAsync(new CallToolRequestParams { Name = NameInTheJAStudio, Arguments = call.Arguments, Meta = call.Meta }, cancellationToken);
}
