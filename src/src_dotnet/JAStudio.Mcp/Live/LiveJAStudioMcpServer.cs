using System;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Threading;
using JAStudio.Core.Note.Collection;
using JAStudio.Mcp.Discovery;

namespace JAStudio.Mcp.Live;

/// <summary>
/// JAStudio's MCP server as a live JAStudio runs it, one with a person working in it, inside Anki or standalone: started once the collection has<br/>
/// loaded for the first time, so that its tools never see a collection still loading at startup, and announced among the <see cref="RunningJAStudios"/><br/>
/// for as long as it runs, for Claude's MCP gateway to find and connect to.
/// </summary>
public sealed class LiveJAStudioMcpServer
{
   readonly IRootResolver _jastudio;
   readonly JAStudioHost _host;
   readonly IMonitor _monitor = IMonitor.New();
   readonly IDisposable _startingOnceTheCollectionHasLoaded;
   JAStudioMcpServer? _startedServer;

   LiveJAStudioMcpServer(IRootResolver jastudio, JAStudioHost host)
   {
      _jastudio = jastudio;
      _host = host;
      _startingOnceTheCollectionHasLoaded = jastudio.Resolve<JPCollection>().ReloadedFromBackend.Subscribe(_ => StartUnlessStarted());
   }

   /// <summary>
   /// Starts the server, on a port the system picks, once <paramref name="jastudio"/>'s collection has loaded for the first time, and announces it as the<br/>
   /// MCP server of a JAStudio hosted by <paramref name="host"/>: Anki, or the standalone host.
   /// </summary>
   public static LiveJAStudioMcpServer StartOnceTheCollectionHasLoaded(IRootResolver jastudio, JAStudioHost host) => new(jastudio, host);

   void StartUnlessStarted() => _monitor.Locked(() =>
   {
      if(_startedServer != null) return;

      _startedServer = JAStudioMcpServer.Start(_jastudio, port: 0);
      _startedServer.AnnounceAs(_host);
   });

   /// <summary>Stops the server, if it started, withdrawing its announcement: as the JAStudio stops. It doesn't start again after that.</summary>
   public void Stop() => _monitor.Locked(() =>
   {
      _startingOnceTheCollectionHasLoaded.Dispose();
      if(_startedServer == null) return;

      _startedServer.StopAsync().GetAwaiter().GetResult();
      _startedServer = null;
   });
}
