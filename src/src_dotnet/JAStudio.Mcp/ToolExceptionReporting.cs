using System;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp;

/// <summary>Reports an exception a tool throws to Claude in full, so that Claude can see what went wrong and where.</summary>
/// <remarks>
/// Without it, the MCP SDK reports only that the tool failed, with no message, unless the exception is an <c>McpException</c>.<br/>
/// The exception isn't swallowed: it becomes the tool's error result, which is how MCP reports a failed tool call.
/// </remarks>
static class ToolExceptionReporting
{
   public static McpRequestHandler<CallToolRequestParams, CallToolResult> ReportsTheWholeException(McpRequestHandler<CallToolRequestParams, CallToolResult> callTheTool) =>
      async (request, cancellationToken) =>
      {
         try
         {
            return await callTheTool(request, cancellationToken);
         }
         catch(Exception exception) when(exception is not OperationCanceledException)
         {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = exception.ToString() }] };
         }
      };
}
