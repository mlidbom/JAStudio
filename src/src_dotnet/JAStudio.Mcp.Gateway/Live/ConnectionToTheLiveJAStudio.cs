using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway.Live;

/// <summary>
/// The gateway's connection to the live JAStudio, the one inside Anki, while it lasts: what the live JAStudio announced, Anki's process, and the<br/>
/// connection to its MCP server, whose tools the gateway offers Claude Code from when the connection opens until it ends.
/// </summary>
sealed class ConnectionToTheLiveJAStudio : IAsyncDisposable
{
   readonly LiveJAStudioAnnouncement _announcement;
   readonly Process _ankiProcess;
   readonly ConnectionToTheJAStudio _connectionToItsMcpServer;

   ConnectionToTheLiveJAStudio(LiveJAStudioAnnouncement announcement, Process ankiProcess, ConnectionToTheJAStudio connectionToItsMcpServer)
   {
      _announcement = announcement;
      _ankiProcess = ankiProcess;
      _connectionToItsMcpServer = connectionToItsMcpServer;
      AnkiExited = ankiProcess.WaitForExitAsync();
   }

   /// <summary>Completes when Anki exits, after which the live JAStudio's tools can't work anymore.</summary>
   public Task AnkiExited { get; }

   /// <summary>
   /// Connects to the live JAStudio that <see cref="LiveJAStudioAnnouncement"/> announces, and offers its tools in <paramref name="toolsOfferedToClaudeCode"/>,<br/>
   /// which tells Claude Code that the gateway's tools changed.
   /// </summary>
   public static async Task<ConnectionToTheLiveJAStudio> OpenAsync(McpServerPrimitiveCollection<McpServerTool> toolsOfferedToClaudeCode, CancellationToken cancellationToken)
   {
      var announcement = LiveJAStudioAnnouncement.Read();
      var ankiProcess = Process.GetProcessById(announcement.ProcessId);
      var connectionToItsMcpServer = await ConnectionToTheJAStudio.OpenAsync(announcement.McpServerUrl, toolsOfferedToClaudeCode, cancellationToken);
      return new ConnectionToTheLiveJAStudio(announcement, ankiProcess, connectionToItsMcpServer);
   }

   /// <summary>What <see cref="LiveJAStudioTools.ConnectToLiveJAStudio"/> answers once the connection has opened: what it connected to, and its tools.</summary>
   public string ReportOfItsOpening() =>
      $"""
       Connected to the live JAStudio, in Anki's process {_announcement.ProcessId}.
       Corpus checkout: {_announcement.CorpusCheckout}
       MCP server: {_announcement.McpServerUrl}
       Its tools, now offered: {_connectionToItsMcpServer.NamesOfTheTools}
       """;

   /// <summary>Ends the connection: withdraws the live JAStudio's tools, closes the connection to its MCP server, and lets go of Anki's process, which runs on.</summary>
   public async ValueTask DisposeAsync()
   {
      await _connectionToItsMcpServer.DisposeAsync();
      _ankiProcess.Dispose();
   }
}
