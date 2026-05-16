using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Threading;
using Compze.Utilities.Logging;
using Compze.Utilities.SystemCE;
using JAStudio.Anki;
using JAStudio.Core;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;
using JAStudio.UI;
using JAStudio.Web;

namespace JAStudio.Anki.PythonInterop;

// ReSharper disable once UnusedType.Global used from python
public class JAStudioAnkiAppRoot
{
   readonly CoreApp _coreApp;
   readonly CardServer _cardServer = new();
   CancellationTokenSource? _reloadCts;
   static readonly TimeSpan ReloadDebounceDelay = TimeSpan.FromMilliseconds(500);

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

   // ReSharper disable once UnusedAutoPropertyAccessor.Global used from python
   // ReSharper disable once UnusedMember.Global used from python
   public bool IsInitialized => _coreApp.Collection.IsInitialized;

   JAStudioAnkiAppRoot(CoreApp coreApp)
   {
      _coreApp = coreApp;
      Dialogs = new AnkiDialogs(coreApp);
      Menus = new PythonAnkiMenus(new AnkiMenus(coreApp.Services));
   }

   // ReSharper disable once UnusedMember.Global // ReSharper disable once UnusedAutoPropertyAccessor.Global used from python
   public static JAStudioAnkiAppRoot Initialize(string configJson, Action<string> configUpdateCallback)
   {
      if(!Environment.GetEnvironmentVariable("DEBUG_JASTUDIO").IsNullEmptyOrWhiteSpace())
      {
         Debugger.Launch();
      }

      var app = AppBootstrapper.BootstrapProduction(new AnkiAddonEnvironmentDependenciesRegistrar(configJson, configUpdateCallback));
      CompzeLogger.LogLevel = LogLevel.Info;
      Console.OutputEncoding = System.Text.Encoding.UTF8;

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
      Dispatcher.UIThread.Invoke(() => UIApp.InitializeMainWindow(app.Services));

      var root = new JAStudioAnkiAppRoot(app) { _uiThread = uiThread };

      root._coreApp.Collection.OnInitialized(() => AnkiFacade.UIUtils.Refresh());

      root._cardServer.OpenNoteInPreviewerAction = noteId =>
      {
         var domainId = NoteId.Parse(noteId);
         var externalId = root._coreApp.Services.ExternalNoteIdMap.ToExternalId(domainId);
         if(externalId.HasValue)
            AnkiFacade.Browser.ExecuteLookupAndShowPreviewer($"{AnkiFieldNames.NoteId}:{externalId.Value}");
      };

      root._cardServer.Start();

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
            ScheduleDebouncedReload();
            break;

         case AnkiLifecycleEvent.SyncCompleted:
            if(_profileOpen)
            {
               CancelPendingReload();
               _coreApp.Services.BackgroundTaskManager.Run(() => _coreApp.Collection.ReloadFromBackend());
            }

            break;

         case AnkiLifecycleEvent.ProfileClosing:
            _profileOpen = false;
            CancelPendingReload();
            _coreApp.Collection.ClearCaches();
            break;
         case AnkiLifecycleEvent.SyncStarting:
            CancelPendingReload();
            _coreApp.Collection.ClearCaches();
            break;

         default:
            throw new ArgumentOutOfRangeException(nameof(lifecycleEvent), lifecycleEvent, null);
      }
   }

   // ReSharper disable once UnusedMember.Global used from python
   public void ShutDown()
   {
      using var _ = this.Log().Info().LogMethodExecutionTime();
      _cardServer.StopAsync().GetAwaiter().GetResult();
      _coreApp.Dispose();
      Dispatcher.UIThread.InvokeShutdown();
   }

   void ScheduleDebouncedReload()
   {
      CancelPendingReload();
      var cts = new CancellationTokenSource();
      _reloadCts = cts;

      _coreApp.Services.BackgroundTaskManager.RunAsync(async () =>
      {
         await Task.Delay(ReloadDebounceDelay, cts.Token);
         this.Log().Info("Debounce elapsed reloading from backend");
         _coreApp.Collection.ReloadFromBackend();
      });
   }

   void CancelPendingReload()
   {
      if(_reloadCts != null)
      {
         _reloadCts.Cancel();
         _reloadCts.Dispose();
         _reloadCts = null;
      }
   }
}
