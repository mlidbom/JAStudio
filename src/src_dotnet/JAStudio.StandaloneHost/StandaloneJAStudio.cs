using System.IO;
using System.Threading.Tasks;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Microsoft.Extensions.Hosting;
using JAStudio.Core;
using JAStudio.Mcp.Discovery;
using JAStudio.Mcp.Live;
using JAStudio.UI;
using JAStudio.UI.Views.Workspace;
using JAStudio.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JAStudio.StandaloneHost;

/// <summary>
/// The standalone JAStudio, started on an add-on folder: its services, the card server that serves its workspace's pages, and its MCP server, which<br/>
/// starts once the collection has loaded. Its workspace window it shows once Avalonia is set up (<see cref="ShowTheWorkspace"/>), and once that has<br/>
/// closed, it's stopped (<see cref="Stop"/>).
/// </summary>
sealed class StandaloneJAStudio
{
   readonly IHost _host;
   readonly CoreApp _coreApp;
   readonly CardServer _cardServer;
   readonly LiveJAStudioMcpServer _mcpServer;

   StandaloneJAStudio(IHost host, CoreApp coreApp, CardServer cardServer, LiveJAStudioMcpServer mcpServer)
   {
      _host = host;
      _coreApp = coreApp;
      _cardServer = cardServer;
      _mcpServer = mcpServer;
   }

   /// <summary>
   /// Starts JAStudio's services on <paramref name="addOnFolder"/>, and its card server, on a port the system picks, leaving alone the one the card server<br/>
   /// inside Anki remembers. Its MCP server starts once the collection has loaded, and is announced among the <see cref="RunningJAStudios"/>.
   /// </summary>
   public static StandaloneJAStudio StartOn(DirectoryInfo addOnFolder)
   {
      var plan = AppBootstrapper.PrepareProduction(new EnvironmentDependenciesRegistrar(addOnFolder));
      var host = Host.CreateDefaultBuilder()
                     .UseServiceProviderFactory(new MicrosoftServiceProviderFactory(plan.Builder))
                     .Build();
      host.StartAsync().GetAwaiter().GetResult();

      var resolver = host.Services.GetRequiredService<IRootResolver>();
      var coreApp = resolver.Resolve<CoreApp>();
      var cardServer = new CardServer(resolver, new NoteLinkOpener(coreApp.Services));
      cardServer.StartOnAPortTheSystemPicks();
      var mcpServer = LiveJAStudioMcpServer.StartOnceTheCollectionHasLoaded(resolver, JAStudioHost.Standalone);
      return new StandaloneJAStudio(host, coreApp, cardServer, mcpServer);
   }

   /// <summary>
   /// Creates the workspace window, JAStudio's main window, shows it, and loads the collection in the background, as JAStudio inside Anki does once an<br/>
   /// Anki profile opens. Called on the UI thread, once Avalonia is set up.
   /// </summary>
   public void ShowTheWorkspace()
   {
      UIApp.InitializeWorkspaceWindow(_coreApp.Services);
      WorkspaceWindow.Shown();
      _coreApp.Services.BackgroundTaskManager.Run(() => _coreApp.Collection.ReloadFromBackend());
   }

   /// <summary>
   /// Stops JAStudio, once its workspace has closed: its MCP server, its card server, its services and its host. On a thread of its own, as JAStudio inside Anki is<br/>
   /// stopped from Python's: this is called on the UI thread, which no longer runs its dispatcher, so the stops' continuations, posted to it, would never run.
   /// </summary>
   public void Stop() => Task.Run(StopOnThisThread).GetAwaiter().GetResult();

   void StopOnThisThread()
   {
      _mcpServer.Stop();
      _cardServer.StopAsync().GetAwaiter().GetResult();
      _coreApp.Dispose();
      _host.StopAsync().GetAwaiter().GetResult();
      _host.Dispose();
   }
}
