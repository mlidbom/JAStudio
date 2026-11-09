using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway.Headless;

/// <summary>
/// A run of the headless JAStudio while it runs: its process, and the gateway's connection to its MCP server, whose tools the gateway offers<br/>
/// Claude Code from when the run starts until it ends.
/// </summary>
sealed class RunOfTheHeadlessJAStudio : IAsyncDisposable
{
   RunOfTheHeadlessJAStudio(HeadlessJAStudioSetup setup, HeadlessJAStudioProcess process, ConnectionToTheJAStudio connection)
   {
      Setup = setup;
      Process = process;
      Connection = connection;
   }

   public HeadlessJAStudioSetup Setup { get; }
   public HeadlessJAStudioProcess Process { get; }
   public ConnectionToTheJAStudio Connection { get; }

   /// <summary>
   /// Starts the headless JAStudio from <paramref name="runtimeFolder"/>, the copy of its build output, connects to its MCP server, and offers its<br/>
   /// tools in <paramref name="toolsOfferedToClaudeCode"/>, which tells Claude Code that the gateway's tools changed.
   /// </summary>
   public static async Task<RunOfTheHeadlessJAStudio> StartAsync(DirectoryInfo runtimeFolder, HeadlessJAStudioSetup setup, McpServerPrimitiveCollection<McpServerTool> toolsOfferedToClaudeCode, CancellationToken cancellationToken)
   {
      var process = await HeadlessJAStudioProcess.StartAsync(runtimeFolder, setup, cancellationToken);
      var connection = await ConnectionToTheJAStudio.OpenAsync(process.McpServerUrl, toolsOfferedToClaudeCode, cancellationToken);
      return new RunOfTheHeadlessJAStudio(setup, process, connection);
   }

   /// <summary>What <see cref="HeadlessJAStudioTools.StartHeadlessJAStudio"/> answers once the run has started: what it runs, on what, and its tools.</summary>
   public string ReportOfItsStart(SourceCheckout builtFrom, TimeSpan startedIn) =>
      $"""
       The headless JAStudio runs, built from {builtFrom.Folder.FullName}, started in {startedIn.TotalSeconds:F1} seconds.
       Corpus checkout: {Setup.CorpusFolder.FullName}
       Anki collection: {Setup.AnkiCollectionFile.FullName}
       Log: {Process.Log.File.FullName}
       Its tools, now offered: {Connection.NamesOfTheTools}
       """;

   /// <summary>Ends the run: withdraws its tools, and stops the headless JAStudio if it's still running.</summary>
   public async ValueTask DisposeAsync()
   {
      await Connection.DisposeAsync();
      await Process.DisposeAsync();
   }
}
