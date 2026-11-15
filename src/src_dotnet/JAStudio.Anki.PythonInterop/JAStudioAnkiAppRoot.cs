using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Microsoft.Extensions.Hosting;
using Compze.Internals.Logging;
using Compze.Internals.SystemCE;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core;
using JAStudio.Mcp.Discovery;
using JAStudio.Mcp.Live;
using JAStudio.UI;
using JAStudio.UI.Shortcuts;
using JAStudio.UI.Views.Importing;
using JAStudio.UI.Views.Workspace;
using JAStudio.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JAStudio.Anki.PythonInterop;

// ReSharper disable once UnusedType.Global used from python
// ReSharper disable once UnusedMember.Global
public class JAStudioAnkiAppRoot
{
   readonly IHost _host;
   readonly CoreApp _coreApp;
   readonly CardServer _cardServer;
   readonly LiveJAStudioMcpServer _liveMcpServer;
   readonly FollowingAnkisLifecycle _followingAnkisLifecycle;

   // Stored to keep the UI thread rooted (prevent GC). Not accessed directly.
#pragma warning disable CS0414
   Thread? _uiThread;
#pragma warning restore CS0414

   // ReSharper disable once MemberCanBePrivate.Global used from python
   // ReSharper disable once UnusedMember.Global
   public TemporaryServiceCollection Services => _coreApp.Services;

   // ReSharper disable once UnusedAutoPropertyAccessor.Global used from python
   public AnkiDialogs Dialogs { get; }

   // ReSharper disable once UnusedAutoPropertyAccessor.Global used from python
   public PythonAnkiMenus Menus { get; }

   /// <summary>The keyboard shortcuts for a note being viewed, which Python binds in Anki's reviewer and previewer.</summary>
   public NoteViewShortcuts NoteViewShortcuts { get; }

   // ReSharper disable once UnusedAutoPropertyAccessor.Global used from python
   // ReSharper disable once UnusedMember.Global used from python
   public bool IsInitialized => _coreApp.Collection.IsInitialized;

   JAStudioAnkiAppRoot(IHost host, CoreApp coreApp, CardServer cardServer, LiveJAStudioMcpServer liveMcpServer)
   {
      _host = host;
      _coreApp = coreApp;
      _cardServer = cardServer;
      _liveMcpServer = liveMcpServer;
      _followingAnkisLifecycle = new FollowingAnkisLifecycle(coreApp.Collection, coreApp.Services.Resolve<AnkisAvailability>(), coreApp.Services.BackgroundTaskManager);
      Dialogs = new AnkiDialogs();
      Menus = new PythonAnkiMenus(new AnkiMenus(coreApp.Services));
      NoteViewShortcuts = new NoteViewShortcuts(coreApp.Config);
   }

   /// <summary>
   /// The host's content root, which it has to be given: left to itself the host takes the current directory, or, when that is Windows' system<br/>
   /// directory, <see cref="AppContext.BaseDirectory"/>, which is empty when Python hosts .NET, as inside Anki. The host then fails to build,<br/>
   /// with "The path must be absolute". Windows' Start menu starts Anki in the system directory.
   /// </summary>
   /// <remarks>JAStudio reads nothing from the content root: any folder that exists will do.</remarks>
   static string TheFolderJAStudiosAssembliesAreIn =>
      Path.GetDirectoryName(typeof(JAStudioAnkiAppRoot).Assembly.Location) ?? throw new InvalidOperationException("JAStudio's assembly wasn't loaded from a file.");

   // ReSharper disable once UnusedMember.Global // ReSharper disable once UnusedAutoPropertyAccessor.Global used from python
   public static JAStudioAnkiAppRoot Initialize()
   {
      if(!Environment.GetEnvironmentVariable("DEBUG_JASTUDIO").IsNullEmptyOrWhiteSpace())
      {
         Debugger.Launch();
      }

      var plan = AppBootstrapper.PrepareProduction(new AnkiAddonEnvironmentDependenciesRegistrar());

      var host = Host.CreateDefaultBuilder()
                     .UseContentRoot(TheFolderJAStudiosAssembliesAreIn)
                     .UseServiceProviderFactory(new MicrosoftServiceProviderFactory(plan.Builder))
                     .Build();

      // The Anki addon doesn't run a console loop; Anki owns the process. Start the
      // host so hosted services (none today) get a chance to fire OnStarted callbacks
      // but we never RunAsync().
      host.StartAsync().GetAwaiter().GetResult();

      CompzeLogger.LogLevel = LogLevel.Info;
      Console.OutputEncoding = System.Text.Encoding.UTF8;

      var resolver = host.Services.GetRequiredService<IRootResolver>();
      var coreApp = resolver.Resolve<CoreApp>();

      var uiThread = new Thread(() =>
                     {
                        AppBuilder.Configure<UIApp>()
                                  .UsePlatformDetect()
                                  .StartWithClassicDesktopLifetime([], ShutdownMode.OnExplicitShutdown);
                     })
                     {
                        IsBackground = true,
                        Name = "AvaloniaUIThread"
                     };

      if(OperatingSystem.IsWindows())
      {
         uiThread.SetApartmentState(ApartmentState.STA);
      }

      uiThread.Start();

      UIApp.WaitForInitialization(TimeSpan.FromSeconds(30));
      Dispatcher.UIThread.Invoke(() => UIApp.InitializeWorkspaceWindow(coreApp.Services));
      AutomaticImportRunReports.StartShowing(coreApp.Services);

      var cardServer = new CardServer(resolver, new NoteLinkOpener(coreApp.Services));

      var liveMcpServer = LiveJAStudioMcpServer.StartOnceTheCollectionHasLoaded(resolver, JAStudioHost.Anki);

      var root = new JAStudioAnkiAppRoot(host, coreApp, cardServer, liveMcpServer) { _uiThread = uiThread };

      root._coreApp.Collection.ReloadedFromBackend.Subscribe(_ => AnkiFacade.UIUtils.Refresh());

      cardServer.Start();

      return root;
   }

   /// <summary>
   /// An Anki profile opened or is closing, or Anki's own sync is starting or has finished: all that Anki tells JAStudio of, which Python passes on<br/>
   /// from Anki's hooks (<see cref="FollowingAnkisLifecycle"/>).
   /// </summary>
   // ReSharper disable once UnusedMember.Global used from python
   public void HandleAnkiLifecycleEvent(AnkiLifecycleEvent lifecycleEvent)
   {
      this.Log().Info($"HandleAnkiLifecycleEvent({lifecycleEvent})");
      _followingAnkisLifecycle.Handle(lifecycleEvent);
   }

   // ReSharper disable once UnusedMember.Global used from python
   public void ShutDown() => this.Log().Info().MethodExecutionTime(() =>
   {
      _liveMcpServer.Stop();
      _cardServer.StopAsync().GetAwaiter().GetResult();
      _coreApp.Dispose();
      _host.StopAsync().GetAwaiter().GetResult();
      _host.Dispose();
      Dispatcher.UIThread.InvokeShutdown();
   });
}
