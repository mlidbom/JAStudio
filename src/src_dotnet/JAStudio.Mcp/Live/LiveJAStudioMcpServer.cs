using System;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Threading;
using JAStudio.Core;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Mcp.Live;

/// <summary>
/// JAStudio's MCP server as the live JAStudio, the one inside Anki, runs it: started once the collection has loaded for the first time, so that its<br/>
/// tools never see a collection still loading at startup, and announced (<see cref="LiveJAStudioAnnouncement"/>) for as long as it runs, for the MCP<br/>
/// gateway <c>jastudio-live</c> to connect by.
/// </summary>
public sealed class LiveJAStudioMcpServer
{
   readonly IRootResolver _jastudio;
   readonly IMonitor _monitor = IMonitor.New();
   readonly IDisposable _startingOnceTheCollectionHasLoaded;
   JAStudioMcpServer? _startedServer;

   LiveJAStudioMcpServer(IRootResolver jastudio)
   {
      _jastudio = jastudio;
      _startingOnceTheCollectionHasLoaded = jastudio.Resolve<JPCollection>().ReloadedFromBackend.Subscribe(_ => StartUnlessStarted());
   }

   /// <summary>Starts the server, on a port the system picks, once <paramref name="jastudio"/>'s collection has loaded for the first time.</summary>
   public static LiveJAStudioMcpServer StartOnceTheCollectionHasLoaded(IRootResolver jastudio) => new(jastudio);

   void StartUnlessStarted() => _monitor.Locked(() =>
   {
      if(_startedServer != null) return;

      _startedServer = JAStudioMcpServer.Start(_jastudio, port: 0);
      new LiveJAStudioAnnouncement(_startedServer.Url, Environment.ProcessId, _jastudio.Resolve<IEnvironmentPaths>().DatabaseDir).Write();
   });

   /// <summary>Stops the server, if it started, and withdraws its announcement: as Anki closes. It doesn't start again after that.</summary>
   public void Stop() => _monitor.Locked(() =>
   {
      _startingOnceTheCollectionHasLoaded.Dispose();
      if(_startedServer == null) return;

      LiveJAStudioAnnouncement.Withdraw();
      _startedServer.StopAsync().GetAwaiter().GetResult();
      _startedServer = null;
   });
}
