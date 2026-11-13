using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JAStudio.Mcp.Discovery;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway.Connections;

/// <summary>
/// The JAStudios the gateway is connected to (<see cref="JAStudioConnection"/>), several at once, each under a name of its own, which their tools<br/>
/// are offered under: <c>anki-main__search_notes</c> and <c>standalone-unidic__search_notes</c> are the same tool in two JAStudios. It finds them<br/>
/// among the <see cref="RunningJAStudios"/>, and follows them as they run: when one stops, its tools are withdrawn, and when it runs again, such as<br/>
/// when Anki has restarted, they're offered again.
/// </summary>
sealed class ConnectedJAStudios : IAsyncDisposable
{
   /// <summary>
   /// How long the gateway waits at most before looking at the running JAStudios again: a JAStudio announcing itself or withdrawing its announcement<br/>
   /// wakes it at once, but a JAStudio that crashed wakes no one.
   /// </summary>
   static readonly TimeSpan HowOftenTheRunningJAStudiosAreLookedAt = TimeSpan.FromSeconds(1);

   readonly RunningJAStudios _runningJAStudios;
   readonly McpServerPrimitiveCollection<McpServerTool> _toolsOfferedToClaudeCode;
   readonly DirectoryInfo _theSourceCheckoutsCorpusCheckout;
   readonly OneAtATime _changes = new();
   readonly List<JAStudioConnection> _connections = [];
   readonly CancellationTokenSource _stopFollowing = new();
   Task _following;

   /// <param name="runningJAStudios">The running JAStudios, as they announced themselves.</param>
   /// <param name="toolsOfferedToClaudeCode">The gateway's tools: the connected JAStudios' tools are offered as its own by adding them here.</param>
   /// <param name="theSourceCheckoutsCorpusCheckout">
   /// The corpus checkout of the source checkout the gateway runs in, <c>src/jas_database</c>, which the JAStudio connected to when Claude names none<br/>
   /// works on.
   /// </param>
   public ConnectedJAStudios(RunningJAStudios runningJAStudios, McpServerPrimitiveCollection<McpServerTool> toolsOfferedToClaudeCode, DirectoryInfo theSourceCheckoutsCorpusCheckout)
   {
      _runningJAStudios = runningJAStudios;
      _toolsOfferedToClaudeCode = toolsOfferedToClaudeCode;
      _theSourceCheckoutsCorpusCheckout = theSourceCheckoutsCorpusCheckout;
      _following = FollowTheRunningJAStudiosAsync(_stopFollowing.Token);
   }

   /// <summary>The running JAStudios, each by the name to connect to it by, those connected to, and those connected to that don't run now.</summary>
   public Task<string> ReportOfTheRunningJAStudiosAsync(CancellationToken cancellationToken) => _changes.RunAsync(async () =>
   {
      FollowAgainIfFollowingFailed();
      return ReportOf(await NamedAsync(_runningJAStudios.All, cancellationToken));
   });

   string ReportOf(List<NamedRunningJAStudio> running)
   {
      var connectedButNotRunning = _connections.Where(connection => running.All(it => it.JAStudio.Identity != connection.JAStudio)).ToList();

      var report = running.Count == 0
                      ? "No JAStudio runs."
                      : $"""
                         The running JAStudios, each by the name to connect to it by (connect_to_jastudio):
                         {string.Join(Environment.NewLine, running.Select(it => $"- {it.Name}{(ConnectionTo(it.JAStudio.Identity) != null ? ", connected" : "")}: {it.JAStudio}"))}
                         """;
      return connectedButNotRunning.Count == 0
                ? report
                : $"""
                   {report}
                   Connected to, but not running now:
                   {string.Join(Environment.NewLine, connectedButNotRunning.Select(it => $"- {it.Report()}"))}
                   """;
   }

   /// <summary>
   /// Connects to the running JAStudio named <paramref name="name"/>, as <see cref="ReportOfTheRunningJAStudiosAsync"/> names it, or with none named, to<br/>
   /// the one working on the corpus checkout of the gateway's source checkout. Returns a report of what it connected to.
   /// </summary>
   public Task<string> ConnectAsync(string? name, CancellationToken cancellationToken) => _changes.RunAsync(async () =>
   {
      FollowAgainIfFollowingFailed();
      var running = await NamedAsync(_runningJAStudios.All, cancellationToken);
      var jastudio = name == null
                        ? running.SingleOrDefault(it => it.JAStudio.Identity.WorksOn(_theSourceCheckoutsCorpusCheckout))
                          ?? throw new McpException($"No JAStudio runs on this source checkout's corpus checkout, {_theSourceCheckoutsCorpusCheckout.FullName}.{Environment.NewLine}{ReportOf(running)}")
                        : running.SingleOrDefault(it => it.Name == ConnectionName.Parse(name))
                          ?? throw new McpException($"No running JAStudio is named {name}.{Environment.NewLine}{ReportOf(running)}");
      return await ConnectAsync(jastudio, cancellationToken);
   });

   /// <summary>Connects to <paramref name="jastudio"/>, a running JAStudio, such as the headless JAStudio the gateway has just started. Returns a report of what it connected to.</summary>
   public Task<string> ConnectAsync(RunningJAStudio jastudio, CancellationToken cancellationToken) => _changes.RunAsync(async () =>
   {
      FollowAgainIfFollowingFailed();
      var running = await NamedAsync(_runningJAStudios.All, cancellationToken);
      return await ConnectAsync(running.Single(it => it.JAStudio.IsTheSameRunAs(jastudio)), cancellationToken);
   });

   async Task<string> ConnectAsync(NamedRunningJAStudio jastudio, CancellationToken cancellationToken)
   {
      if(ConnectionTo(jastudio.JAStudio.Identity) is {} connectedAlready) return $"Connected to it already, as {connectedAlready.Report()}";

      var connection = new JAStudioConnection(jastudio.Name, jastudio.JAStudio.Identity, _toolsOfferedToClaudeCode);
      await connection.FollowAsync([jastudio.JAStudio], cancellationToken);
      _connections.Add(connection);
      return $"""
              Connected to {connection.Report()}
              It stays connected through its restarts: while it doesn't run, its tools are withdrawn, and when it runs again, they're offered again.
              """;
   }

   /// <summary>Ends the connection named <paramref name="name"/>, withdrawing its JAStudio's tools. The JAStudio runs on.</summary>
   public Task<string> DisconnectAsync(string name) => _changes.RunAsync(async () =>
   {
      FollowAgainIfFollowingFailed();
      var connectionName = ConnectionName.Parse(name);
      var connection = _connections.SingleOrDefault(it => it.Name == connectionName)
                       ?? throw new McpException($"No connection is named {name}. The connections: {string.Join(", ", _connections.Select(it => it.Name))}");
      await EndAsync(connection);
      return $"The connection {name} has ended: its tools are withdrawn.";
   });

   /// <summary>Ends the connection to <paramref name="jastudio"/>, if there is one, withdrawing its tools, such as as the gateway stops the headless JAStudio.</summary>
   public Task DisconnectFromAsync(JAStudioIdentity jastudio) => _changes.RunAsync(async () =>
   {
      if(ConnectionTo(jastudio) is {} connection) await EndAsync(connection);
   });

   async Task EndAsync(JAStudioConnection connection)
   {
      _connections.Remove(connection);
      await connection.DisposeAsync();
   }

   JAStudioConnection? ConnectionTo(JAStudioIdentity jastudio) => _connections.SingleOrDefault(it => it.JAStudio == jastudio);

   /// <summary>
   /// Keeps every connection following its JAStudio (<see cref="JAStudioConnection.FollowAsync"/>): looks at the running JAStudios each time they<br/>
   /// change, and at least every <see cref="HowOftenTheRunningJAStudiosAreLookedAt"/>, until the gateway stops.
   /// </summary>
   async Task FollowTheRunningJAStudiosAsync(CancellationToken stopping)
   {
      await Task.Yield(); // The constructor starting it returns at once.
      while(!stopping.IsCancellationRequested)
      {
         var running = _runningJAStudios.All;
         await _changes.RunAsync(async () =>
         {
            foreach(var connection in _connections) await connection.FollowAsync(running, stopping);
         });
         await _runningJAStudios.AwaitAChangeFromAsync(running, HowOftenTheRunningJAStudiosAreLookedAt, stopping);
      }
   }

   /// <summary>
   /// Fails with what made following the running JAStudios fail, such as a JAStudio that stopped as the gateway connected to it, and starts following<br/>
   /// them again: the failure reaches Claude in the answer to the next call of one of the gateway's tools, and the connections follow their JAStudios on.
   /// </summary>
   void FollowAgainIfFollowingFailed()
   {
      if(_following.Exception?.InnerException is not {} failure) return;

      _following = FollowTheRunningJAStudiosAsync(_stopFollowing.Token);
      throw new InvalidOperationException("Following the running JAStudios failed, so connections to them may not have followed them. The gateway follows them again now.", failure);
   }

   /// <summary>
   /// <paramref name="running"/>, each with the name to connect to it by: the name it's connected as, or else the name for it (<see cref="ConnectionName.ForAsync"/>),<br/>
   /// numbered when another JAStudio has that name already.
   /// </summary>
   async Task<List<NamedRunningJAStudio>> NamedAsync(IReadOnlyList<RunningJAStudio> running, CancellationToken cancellationToken)
   {
      var named = new List<NamedRunningJAStudio>();
      foreach(var jastudio in running)
      {
         var name = ConnectionTo(jastudio.Identity)?.Name ?? TheFirstNameNotTaken(await ConnectionName.ForAsync(jastudio, cancellationToken));
         named.Add(new NamedRunningJAStudio(name, jastudio));
      }

      return named;

      ConnectionName TheFirstNameNotTaken(ConnectionName name)
      {
         var numbered = name;
         for(var number = 2; IsTaken(numbered); number++) numbered = name.Numbered(number);
         return numbered;
      }

      bool IsTaken(ConnectionName name) => _connections.Any(it => it.Name == name) || named.Any(it => it.Name == name);
   }

   /// <summary>A running JAStudio, with the name to connect to it by.</summary>
   sealed record NamedRunningJAStudio(ConnectionName Name, RunningJAStudio JAStudio);

   /// <summary>Stops following the running JAStudios, and ends every connection, withdrawing their tools. The JAStudios run on.</summary>
   public async ValueTask DisposeAsync()
   {
      await _stopFollowing.CancelAsync();
      try
      {
         await _following;
      }
      catch(OperationCanceledException) when(_stopFollowing.IsCancellationRequested) //How following ends: the wait for a change is cancelled.
      {
      }

      await _changes.RunAsync(async () =>
      {
         foreach(var connection in _connections) await connection.DisposeAsync();
         _connections.Clear();
      });
      _stopFollowing.Dispose();
   }
}
