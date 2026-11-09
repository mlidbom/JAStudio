using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Client;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway;

/// <summary>
/// A run of the headless JAStudio while it runs: its process, the gateway's connection to its MCP server, and its tools, which the gateway offers<br/>
/// Claude Code from when the run starts until it ends.
/// </summary>
sealed class RunOfTheHeadlessJAStudio : IAsyncDisposable
{
   readonly McpClient _mcpServerConnection;
   readonly McpServerPrimitiveCollection<McpServerTool> _toolsOfferedToClaudeCode;

   RunOfTheHeadlessJAStudio(HeadlessJAStudioSetup setup, HeadlessJAStudioProcess process, McpClient mcpServerConnection, IReadOnlyList<McpServerTool> tools, McpServerPrimitiveCollection<McpServerTool> toolsOfferedToClaudeCode)
   {
      Setup = setup;
      Process = process;
      _mcpServerConnection = mcpServerConnection;
      Tools = tools;
      _toolsOfferedToClaudeCode = toolsOfferedToClaudeCode;
   }

   public HeadlessJAStudioSetup Setup { get; }
   public HeadlessJAStudioProcess Process { get; }

   /// <summary>The headless JAStudio's tools, as the gateway offers them.</summary>
   public IReadOnlyList<McpServerTool> Tools { get; }

   /// <summary>
   /// Starts the headless JAStudio from <paramref name="runtimeFolder"/>, the copy of its build output, connects to its MCP server, and offers its<br/>
   /// tools in <paramref name="toolsOfferedToClaudeCode"/>, which tells Claude Code that the gateway's tools changed.
   /// </summary>
   public static async Task<RunOfTheHeadlessJAStudio> StartAsync(DirectoryInfo runtimeFolder, HeadlessJAStudioSetup setup, McpServerPrimitiveCollection<McpServerTool> toolsOfferedToClaudeCode, CancellationToken cancellationToken)
   {
      var process = await HeadlessJAStudioProcess.StartAsync(runtimeFolder, setup, cancellationToken);
      var mcpServerConnection = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions { Endpoint = process.McpServerUrl }), cancellationToken: cancellationToken);
      var tools = await ToolsOfferedOverAsync(mcpServerConnection, cancellationToken);

      var run = new RunOfTheHeadlessJAStudio(setup, process, mcpServerConnection, tools, toolsOfferedToClaudeCode);
      run.OfferTheTools();
      return run;
   }

   /// <summary>What <see cref="HeadlessJAStudioTools.StartHeadlessJAStudio"/> answers once the run has started: what it runs, on what, and its tools.</summary>
   public string ReportOfItsStart(SourceCheckout builtFrom, TimeSpan startedIn) =>
      $"""
       The headless JAStudio runs, built from {builtFrom.Folder.FullName}, started in {startedIn.TotalSeconds:F1} seconds.
       Corpus checkout: {Setup.CorpusFolder.FullName}
       Anki collection: {Setup.AnkiCollectionFile.FullName}
       Log: {Process.Log.File.FullName}
       Its tools, now offered: {string.Join(", ", Tools.Select(it => it.ProtocolTool.Name))}
       """;

   /// <summary>The tools of the headless JAStudio's MCP server that <paramref name="mcpServerConnection"/> connects to, each forwarding its calls over it.</summary>
   static async Task<IReadOnlyList<McpServerTool>> ToolsOfferedOverAsync(McpClient mcpServerConnection, CancellationToken cancellationToken) =>
      [.. (await mcpServerConnection.ListToolsAsync(cancellationToken: cancellationToken)).Select(it => new ToolOfTheHeadlessJAStudio(it.ProtocolTool, mcpServerConnection))];

   void OfferTheTools()
   {
      using var _ = _toolsOfferedToClaudeCode.DeferChangedEvents();
      foreach(var tool in Tools) _toolsOfferedToClaudeCode.Add(tool);
   }

   void WithdrawTheTools()
   {
      using var _ = _toolsOfferedToClaudeCode.DeferChangedEvents();
      foreach(var tool in Tools) _toolsOfferedToClaudeCode.Remove(tool);
   }

   /// <summary>Ends the run: withdraws its tools, and stops the headless JAStudio if it's still running.</summary>
   public async ValueTask DisposeAsync()
   {
      WithdrawTheTools();
      await _mcpServerConnection.DisposeAsync();
      await Process.DisposeAsync();
   }
}
