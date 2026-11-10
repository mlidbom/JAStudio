using System;
using System.Diagnostics;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Threading;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Microsoft.Extensions.Hosting;
using Compze.Internals.Logging;
using Compze.Internals.SystemCE;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core;
using JAStudio.Core.Importing;
using JAStudio.Mcp.Live;
using JAStudio.UI;
using JAStudio.UI.Shortcuts;
using JAStudio.UI.Views.Importing;
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
   readonly CatchingUpWithAnki _catchingUpWithAnki;

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

   /// <summary>The writes of the notes' study-notes into Anki, which Python makes, held during a sync and while no Anki profile is open.</summary>
   // ReSharper disable once MemberCanBePrivate.Global used from python
   public StudyNoteWrites StudyNoteWrites { get; } = new();

   // ReSharper disable once UnusedAutoPropertyAccessor.Global used from python
   // ReSharper disable once UnusedMember.Global used from python
   public bool IsInitialized => _coreApp.Collection.IsInitialized;

   JAStudioAnkiAppRoot(IHost host, CoreApp coreApp, CardServer cardServer, LiveJAStudioMcpServer liveMcpServer)
   {
      _host = host;
      _coreApp = coreApp;
      _cardServer = cardServer;
      _liveMcpServer = liveMcpServer;
      _catchingUpWithAnki = new CatchingUpWithAnki(coreApp.Collection, StudyNoteWrites, coreApp.Services.BackgroundTaskManager);
      Dialogs = new AnkiDialogs();
      Menus = new PythonAnkiMenus(new AnkiMenus(coreApp.Services));
      NoteViewShortcuts = new NoteViewShortcuts(coreApp.Config);
   }

   // ReSharper disable once UnusedMember.Global // ReSharper disable once UnusedAutoPropertyAccessor.Global used from python
   public static JAStudioAnkiAppRoot Initialize(string configJson, Action<string> configUpdateCallback)
   {
      if(!Environment.GetEnvironmentVariable("DEBUG_JASTUDIO").IsNullEmptyOrWhiteSpace())
      {
         Debugger.Launch();
      }

      var plan = AppBootstrapper.PrepareProduction(new AnkiAddonEnvironmentDependenciesRegistrar(configJson, configUpdateCallback));

      var host = Host.CreateDefaultBuilder()
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
#pragma warning disable CS0618 // Type or member is obsolete
      DefaultMenuInteractionHandler.MenuShowDelay = TimeSpan.Zero;
#pragma warning restore CS0618 // Type or member is obsolete
      Dispatcher.UIThread.Invoke(() => UIApp.InitializeWorkspaceWindow(coreApp.Services));
      AutomaticImportRunReports.StartShowing(coreApp.Services);

      var cardServer = new CardServer(resolver, new NoteLinkOpener(coreApp.Services));

      var liveMcpServer = LiveJAStudioMcpServer.StartOnceTheCollectionHasLoaded(resolver);

      var root = new JAStudioAnkiAppRoot(host, coreApp, cardServer, liveMcpServer) { _uiThread = uiThread };

      root._coreApp.Collection.ReloadedFromBackend.Subscribe(_ => AnkiFacade.UIUtils.Refresh());

      cardServer.Start();

      return root;
   }

   // ReSharper disable once UnusedMember.Global used from python
   public void HandleAnkiLifecycleEvent(AnkiLifecycleEvent lifecycleEvent)
   {
      this.Log().Info($"HandleAnkiLifecycleEvent({lifecycleEvent})");
      _catchingUpWithAnki.Handle(lifecycleEvent);
   }

   /// <summary>
   /// Anki finished importing a file, such as an .apkg, through its import dialog: runs the <see cref="AutomaticImportRuns"/>. Python calls it from Anki's<br/>
   /// <c>operation_did_execute</c> hook, since Anki's importer adds the notes in its Rust backend without announcing each one.
   /// </summary>
   // ReSharper disable once UnusedMember.Global used from python
   public void AnkiImportedAFile() => _coreApp.Services.Resolve<AutomaticImportRuns>().AfterAnkiImportedAFile();

   /// <summary>
   /// A note of the note type <paramref name="ankiNoteType"/> was added in Anki, as AnkiConnect adds them: runs the <see cref="AutomaticImportRuns"/> of that<br/>
   /// note type. Python calls it once the note is in Anki's database.
   /// </summary>
   // ReSharper disable once UnusedMember.Global used from python
   public void ANoteWasAddedInAnki(string ankiNoteType) => _coreApp.Services.Resolve<AutomaticImportRuns>().AfterANoteWasAddedInAnki(ankiNoteType);

   /// <inheritdoc cref="CatchingUpWithAnki.AnkiChangedNotes"/>
   /// <remarks>Python calls it from Anki's <c>operation_did_execute</c> hook.</remarks>
   // ReSharper disable once UnusedMember.Global used from python
   public void AnkiChangedNotes() => _catchingUpWithAnki.AnkiChangedNotes();

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
