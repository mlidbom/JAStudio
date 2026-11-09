using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Compze.DependencyInjection.Abstractions;
using JAStudio.Core.Specifications;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace JAStudio.Mcp.Specifications;

/// <summary>
/// A collection that starts empty, served by JAStudio's MCP server, with a client connected to the server over HTTP as the MCP gateway connects<br/>
/// to it: specifications call the tools as Claude does, through the gateway.
/// </summary>
public abstract class McpServerSpecification : SpecificationStartingWithAnEmptyCollection
{
   /// <summary>How tool results are read back: as the server writes them, with property names in camel case and enum values by name.</summary>
   static readonly JsonSerializerOptions ToolResultJsonOptions = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

   readonly JAStudioMcpServer _server;
   readonly McpClient _client;

   protected McpServerSpecification()
   {
      _server = JAStudioMcpServer.Start(GetService<IRootResolver>(), port: 0);
      _client = McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions { Endpoint = _server.Url })).GetAwaiter().GetResult();
   }

   /// <summary>The tools the server offers, as the gateway lists them.</summary>
   protected IReadOnlyList<Tool> ListTools() => [.. _client.ListToolsAsync().AsTask().GetAwaiter().GetResult().Select(it => it.ProtocolTool)];

   protected CallToolResult CallTool(string toolName, Dictionary<string, object?> arguments) =>
      _client.CallToolAsync(toolName, arguments).AsTask().GetAwaiter().GetResult();

   /// <summary>The text a tool answered with: every tool here answers with a single text.</summary>
   protected static string TextOf(CallToolResult result) => result.Content.OfType<TextContentBlock>().Single().Text;

   /// <summary>A tool's answer read back into the type the tool answered with, which the tool wrote as JSON.</summary>
   protected static TResult ResultOf<TResult>(CallToolResult result) =>
      JsonSerializer.Deserialize<TResult>(TextOf(result), ToolResultJsonOptions) ?? throw new InvalidOperationException($"The tool answered null: {TextOf(result)}");

   public override void Dispose()
   {
      _client.DisposeAsync().AsTask().GetAwaiter().GetResult();
      _server.StopAsync().GetAwaiter().GetResult();
      base.Dispose();
      GC.SuppressFinalize(this);
   }
}
