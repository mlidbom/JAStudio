using System;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway;

/// <summary>Reports an exception one of the gateway's tools throws to Claude in full, so that Claude can see what went wrong and where.</summary>
/// <remarks>
/// Without it, the MCP SDK reports only that the tool failed, with no message, unless the exception is an <c>McpException</c>.<br/>
/// The exception isn't swallowed: it becomes the tool's error result, which is how MCP reports a failed tool call.<br/>
/// JAStudio's MCP server has the same, <c>JAStudio.Mcp.ToolExceptionReporting</c>: the gateway can't reference it, since the gateway must load no<br/>
/// assembly that building JAStudio replaces, or the build would fail on the file the gateway has loaded.
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
