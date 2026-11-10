using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
using JAStudio.Core.Note.Collection;
using JAStudio.Core.SysUtils.Threading;
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
   readonly BackgroundRefresh _readingTheStudyNotesFromAnkiAgain;
   CancellationTokenSource? _pendingCatchUpWithAnki;

   /// <summary>How long a profile that opened waits before catching up with Anki, since Anki often syncs as a profile opens, which catches up anyway.</summary>
   static readonly TimeSpan CatchUpDelayAfterAProfileOpened = TimeSpan.FromMilliseconds(500);

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
      _readingTheStudyNotesFromAnkiAgain = new BackgroundRefresh(coreApp.Services.BackgroundTaskManager, coreApp.Collection.ReadTheStudyNotesFromAnkiAgain);
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

   bool _profileOpen = false;

   // ReSharper disable once UnusedMember.Global used from python
   public void HandleAnkiLifecycleEvent(AnkiLifecycleEvent lifecycleEvent)
   {
      this.Log().Info($"HandleAnkiLifecycleEvent({lifecycleEvent})");

      switch(lifecycleEvent)
      {
         case AnkiLifecycleEvent.ProfileOpened:
            _profileOpen = true;
            ScheduleTheCatchUpWithAnki();
            break;

         case AnkiLifecycleEvent.SyncCompleted:
            if(_profileOpen)
            {
               CancelThePendingCatchUpWithAnki();
               _coreApp.Services.BackgroundTaskManager.Run(CatchUpWithAnki);
            }

            break;

         case AnkiLifecycleEvent.ProfileClosing:
            _profileOpen = false;
            CancelThePendingCatchUpWithAnki();
            StudyNoteWrites.Hold();
            break;
         case AnkiLifecycleEvent.SyncStarting:
            CancelThePendingCatchUpWithAnki();
            StudyNoteWrites.Hold();
            break;

         default:
            throw new ArgumentOutOfRangeException(nameof(lifecycleEvent), lifecycleEvent, null);
      }
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

   /// <summary>
   /// An Anki operation changed notes, such as marking one, editing tags in Anki's browser or a review that made a card a leech, which tags its note, or<br/>
   /// an undo or redo, which run as operations too: reads the study-notes from Anki again, in the background, one read at a time<br/>
   /// (<see cref="JPCollection.ReadTheStudyNotesFromAnkiAgain"/>). Python calls it from Anki's <c>operation_did_execute</c> hook.
   /// </summary>
   // ReSharper disable once UnusedMember.Global used from python
   public void AnkiChangedNotes() => _readingTheStudyNotesFromAnkiAgain.Run();

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

   /// <summary>
   /// Once Anki's collection can take writes again, after a sync or as a profile opens: loads the collection, the first time, or else reads the study-notes<br/>
   /// from Anki again, then makes the study-note writes held meanwhile. A sync or another profile doesn't change the corpus, so it isn't loaded again:<br/>
   /// what may have changed in Anki is only the study-notes.
   /// </summary>
   void CatchUpWithAnki()
   {
      if(_coreApp.Collection.IsInitialized)
         _coreApp.Collection.ReadTheStudyNotesFromAnkiAgain();
      else
         _coreApp.Collection.ReloadFromBackend();
      StudyNoteWrites.MakeThem();
   }

   /// <summary>Catches up with Anki in the background after <see cref="CatchUpDelayAfterAProfileOpened"/>, unless a sync starts first.</summary>
   void ScheduleTheCatchUpWithAnki()
   {
      CancelThePendingCatchUpWithAnki();
      var cancellation = new CancellationTokenSource();
      _pendingCatchUpWithAnki = cancellation;

      _coreApp.Services.BackgroundTaskManager.RunAsync(async () =>
      {
         await Task.Delay(CatchUpDelayAfterAProfileOpened, cancellation.Token);
         this.Log().Info("The profile opened: catching up with Anki");
         CatchUpWithAnki();
      });
   }

   void CancelThePendingCatchUpWithAnki()
   {
      if(_pendingCatchUpWithAnki != null)
      {
         _pendingCatchUpWithAnki.Cancel();
         _pendingCatchUpWithAnki.Dispose();
         _pendingCatchUpWithAnki = null;
      }
   }
}
