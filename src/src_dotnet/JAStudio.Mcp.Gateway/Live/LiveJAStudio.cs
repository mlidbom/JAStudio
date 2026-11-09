using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway.Live;

/// <summary>
/// The live JAStudio, the one inside Anki, as this gateway connects to it: by the file it announces its MCP server in (<see cref="LiveJAStudioAnnouncement"/>).<br/>
/// While connected, the gateway offers its tools as its own, until Anki exits or the gateway connects anew.
/// </summary>
sealed class LiveJAStudio : IAsyncDisposable
{
   readonly McpServerPrimitiveCollection<McpServerTool> _toolsOfferedToClaudeCode;
   readonly OneAtATime _connectionsAndDisconnections = new();
   ConnectionToTheLiveJAStudio? _connection;

   public LiveJAStudio(IOptions<McpServerOptions> gatewayOptions) =>
      _toolsOfferedToClaudeCode = gatewayOptions.Value.ToolCollection ?? throw new InvalidOperationException("The gateway's MCP server has no tool collection to offer the live JAStudio's tools in.");

   /// <summary>Connects to the live JAStudio, ending the connection there was first, and offers its tools. Returns a report of what it connected to.</summary>
   public Task<string> ConnectAsync(CancellationToken cancellationToken) => _connectionsAndDisconnections.RunAsync(async () =>
   {
      await EndTheConnectionAsync();

      var connection = await ConnectionToTheLiveJAStudio.OpenAsync(_toolsOfferedToClaudeCode, cancellationToken);
      _connection = connection;
      EndTheConnectionWhenAnkiExits(connection);

      return connection.ReportOfItsOpening();
   });

   /// <summary>
   /// Ends <paramref name="connection"/> when Anki exits: its tools are withdrawn, since they can't work anymore. When the gateway has connected<br/>
   /// anew since, <paramref name="connection"/> is no longer <see cref="_connection"/>, and there's nothing left to end.
   /// </summary>
   void EndTheConnectionWhenAnkiExits(ConnectionToTheLiveJAStudio connection)
   {
      _ = connection.AnkiExited.ContinueWith(_ => _connectionsAndDisconnections.RunAsync(EndItUnlessConnectedAnewAsync), TaskScheduler.Default);
      return;

      async Task EndItUnlessConnectedAnewAsync()
      {
         if(_connection == connection) await EndTheConnectionAsync();
      }
   }

   async Task EndTheConnectionAsync()
   {
      if(_connection == null) return;

      await _connection.DisposeAsync();
      _connection = null;
   }

   public async ValueTask DisposeAsync() => await _connectionsAndDisconnections.RunAsync(EndTheConnectionAsync);
}
