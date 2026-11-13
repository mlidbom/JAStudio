using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JAStudio.Mcp.Discovery;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway.Connections;

/// <summary>
/// One of the gateway's connections, to a JAStudio Claude connected to, under a name (<see cref="ConnectionName"/>). It lasts through the JAStudio's<br/>
/// restarts: while the JAStudio runs, it's connected to the MCP server of its current run, whose tools it offers (<see cref="ConnectionToTheMcpServer"/>),<br/>
/// and while it doesn't, it waits for it to run again, such as for Anki to have restarted.
/// </summary>
sealed class JAStudioConnection : IAsyncDisposable
{
   readonly McpServerPrimitiveCollection<McpServerTool> _toolsOfferedToClaudeCode;
   ConnectionToTheMcpServer? _toItsCurrentRun;

   public JAStudioConnection(ConnectionName name, JAStudioIdentity jastudio, McpServerPrimitiveCollection<McpServerTool> toolsOfferedToClaudeCode)
   {
      Name = name;
      JAStudio = jastudio;
      _toolsOfferedToClaudeCode = toolsOfferedToClaudeCode;
   }

   public ConnectionName Name { get; }

   /// <summary>The JAStudio connected to, which the connection follows through its restarts.</summary>
   public JAStudioIdentity JAStudio { get; }

   /// <summary>
   /// Follows the JAStudio to its run among <paramref name="running"/>: ends the connection to a run that has stopped, withdrawing its tools, and<br/>
   /// connects to the run there is, offering its tools.
   /// </summary>
   public async Task FollowAsync(IReadOnlyList<RunningJAStudio> running, CancellationToken cancellationToken)
   {
      var itsRun = running.SingleOrDefault(it => it.Identity == JAStudio);
      if(_toItsCurrentRun != null && itsRun?.IsTheSameRunAs(_toItsCurrentRun.JAStudio) != true) await EndTheConnectionToItsRunAsync();
      if(_toItsCurrentRun == null && itsRun != null) _toItsCurrentRun = await ConnectionToTheMcpServer.OpenAsync(itsRun, Name, _toolsOfferedToClaudeCode, cancellationToken);
   }

   /// <summary>What the connection is connected to now, and the tools it offers, or that it waits for the JAStudio to run again.</summary>
   public string Report() =>
      _toItsCurrentRun is {} toItsCurrentRun
         ? $"""
            {Name}: {toItsCurrentRun.JAStudio}
              Its tools, offered as {Name.OfItsTool("<tool>")}: {string.Join(", ", toItsCurrentRun.NamesOfTheTools)}
            """
         : $"{Name}: {JAStudio}, which doesn't run now. Its tools are offered again when it runs again.";

   async Task EndTheConnectionToItsRunAsync()
   {
      if(_toItsCurrentRun == null) return;

      await _toItsCurrentRun.DisposeAsync();
      _toItsCurrentRun = null;
   }

   /// <summary>Ends the connection, withdrawing the JAStudio's tools, if it runs. The JAStudio runs on.</summary>
   public async ValueTask DisposeAsync() => await EndTheConnectionToItsRunAsync();
}
