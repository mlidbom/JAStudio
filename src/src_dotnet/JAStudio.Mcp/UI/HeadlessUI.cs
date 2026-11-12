using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using JAStudio.Core;
using JAStudio.Core.UI;
using JAStudio.Core.UI.Web;
using JAStudio.Mcp.UI.Screen;
using JAStudio.UI;

namespace JAStudio.Mcp.UI;

/// <summary>
/// JAStudio's UI where there is no screen, the headless JAStudio's: its windows open, are laid out and are drawn with Skia as on a screen, on<br/>
/// Avalonia's headless platform, and web pages, such as the workspace's note pages, are rendered as images by a browser (<see cref="WebPageImages"/>),<br/>
/// so that Claude can look at them, and drive them through <see cref="Screen"/>, on the real corpus.
/// </summary>
/// <remarks>
/// It runs on a thread of its own, as the UI inside Anki does on the thread <c>JAStudioAnkiAppRoot</c> starts, so <see cref="Dispatcher.UIThread"/><br/>
/// is that thread here too. Avalonia sets its application up only once in a process, so a process starts one headless UI at most.
/// </remarks>
/// <remarks>
/// Popups, such as a menu or a drop-down's list, are top levels of their own, as on a real screen, not drawn in their window's overlay layer as<br/>
/// the headless platform draws them by default: JAStudio's menus open from a window of one pixel (<c>PopupMenuWindow</c>), which would clip them.
/// </remarks>
public sealed class HeadlessUI
{
   static readonly TimeSpan HowLongStartingMayTake = TimeSpan.FromSeconds(30);

   static int _startedInThisProcess;

   readonly Dispatcher _uiThread;

   readonly FailuresNoCallWaitedFor _failuresNoCallWaitedFor;

   /// <summary>
   /// How long the UI is given, after it has handled what an action started, for work the action started in the background, such as a search:<br/>
   /// there's no telling when that work is done, and it reports back on the UI thread.
   /// </summary>
   readonly TimeSpan _howLongBackgroundWorkIsWaitedFor;

   HeadlessUI(Dispatcher uiThread, HeadlessScreen screen, IWebPageImages webPageImages, TimeSpan howLongBackgroundWorkIsWaitedFor)
   {
      _uiThread = uiThread;
      _failuresNoCallWaitedFor = FailuresNoCallWaitedFor.KeptFrom(uiThread);
      Screen = screen;
      WebPageImages = webPageImages;
      _howLongBackgroundWorkIsWaitedFor = howLongBackgroundWorkIsWaitedFor;
   }

   /// <summary>The dispatcher of the UI thread, which every window lives on, and which the work JAStudio posts to its UI thread runs on.</summary>
   public Dispatcher UIThread => _uiThread;

   /// <summary>How the headless UI renders web pages, which it has no native web view to show in.</summary>
   public IWebPageImages WebPageImages { get; }

   /// <summary>The windows the headless UI has open, and their popups, where they are: what a person would see on a screen, and act on.</summary>
   /// <remarks>Used on the UI thread only, through <see cref="OnTheUIThread{TResult}"/>.</remarks>
   public HeadlessScreen Screen { get; }

   /// <summary>
   /// Runs <paramref name="function"/> on the UI thread, and returns its result. Fails with the failures of work the UI did on its own<br/>
   /// (<see cref="FailuresNoCallWaitedFor"/>), whether before the call or while it ran.
   /// </summary>
   public TResult OnTheUIThread<TResult>(Func<TResult> function)
   {
      _failuresNoCallWaitedFor.ThrowAnyKept();
      var result = _uiThread.Invoke(function);
      _failuresNoCallWaitedFor.ThrowAnyKept();
      return result;
   }

   /// <summary>Runs <paramref name="action"/> on the UI thread, as <see cref="OnTheUIThread{TResult}"/> runs a function.</summary>
   public void OnTheUIThread(Action action) => OnTheUIThread(() =>
   {
      action();
      return true;
   });

   /// <summary>
   /// Runs <paramref name="action"/> on the UI thread, such as a click, then waits until the UI has handled what it started, and has had<br/>
   /// <see cref="_howLongBackgroundWorkIsWaitedFor"/> for work in the background, so that what follows sees the UI as the action left it.
   /// </summary>
   public void Do(Action action) => Do(() =>
   {
      action();
      return true;
   });

   /// <summary>Runs <paramref name="function"/> on the UI thread as <see cref="Do(Action)"/> runs an action, and returns its result.</summary>
   public TResult Do<TResult>(Func<TResult> function)
   {
      var result = OnTheUIThread(function);
      WaitUntilTheUIThreadIsIdle();
      Thread.Sleep(_howLongBackgroundWorkIsWaitedFor);
      WaitUntilTheUIThreadIsIdle();
      _failuresNoCallWaitedFor.ThrowAnyKept();
      return result;
   }

   /// <summary>Opens a window with <paramref name="open"/>, on the UI thread, renders it as it is once it has handled what opening it started, and closes it.</summary>
   public PngImage RenderAndClose(Func<Window> open) => OnTheUIThread(() =>
   {
      var window = open();
      _uiThread.RunJobs();
      var image = ImageOf(window);
      window.Close();
      return image;
   });

   /// <summary>
   /// Shows a window with <paramref name="show"/>, on the UI thread, lets <paramref name="prepare"/> change it once it has handled what showing it<br/>
   /// started, such as choose one of its tabs, and renders it as it then is. The window stays open, on the <see cref="Screen"/>.
   /// </summary>
   public PngImage Render(Func<Window> show, Action<Window> prepare) => OnTheUIThread(() =>
   {
      var window = show();
      _uiThread.RunJobs();
      prepare(window);
      _uiThread.RunJobs();
      return ImageOf(window);
   });

   /// <summary>
   /// Creates the workspace window of the JAStudio whose services are <paramref name="services"/>, as JAStudio inside Anki does as it starts,<br/>
   /// with its tabs showing their pages as images.
   /// </summary>
   public void CreateTheWorkspaceWindow(TemporaryServiceCollection services) =>
      OnTheUIThread(() => UIApp.InitializeWorkspaceWindowShowingPagesAsImages(services, WebPageImages));

   /// <summary>
   /// Starts the headless UI, rendering web pages with <paramref name="webPageImages"/>, and waits until Avalonia's application is set up on its thread.<br/>
   /// <paramref name="howLongBackgroundWorkIsWaitedFor"/> is how long an action (<see cref="Do{TResult}"/>) gives the work it started in the background:<br/>
   /// a quarter of a second does for JAStudio's searches; the specifications' windows do no work in the background.
   /// </summary>
   /// <remarks>The signal that it's set up isn't disposed: if setting up takes too long, the UI thread may still signal after the wait has given up.</remarks>
   public static HeadlessUI Start(IWebPageImages webPageImages, TimeSpan howLongBackgroundWorkIsWaitedFor)
   {
      if(Interlocked.Exchange(ref _startedInThisProcess, 1) == 1)
         throw new InvalidOperationException("A headless UI has been started in this process already: Avalonia sets its application up only once in a process.");

      var setUp = new ManualResetEventSlim();
      HeadlessUI? started = null;
      var uiThread = new Thread(() =>
      {
         AppBuilder.Configure<UIApp>()
                   .UseSkia()
                   .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false, OverlayPopups = false })
                   .SetupWithoutStarting();
         started = new HeadlessUI(Dispatcher.UIThread, HeadlessScreen.FollowingTheWindowsFromNowOn(), webPageImages, howLongBackgroundWorkIsWaitedFor);
         setUp.Set();
         Dispatcher.UIThread.MainLoop(CancellationToken.None);
      })
      {
         IsBackground = true,
         Name = "HeadlessAvaloniaUIThread"
      };
      uiThread.Start();

      return setUp.Wait(HowLongStartingMayTake) && started is {} headlessUI
                ? headlessUI
                : throw new TimeoutException($"JAStudio's headless UI didn't start within {HowLongStartingMayTake.TotalSeconds}s.");
   }

   /// <summary><paramref name="window"/> as it's drawn at this moment.</summary>
   static PngImage ImageOf(Window window) =>
      (window.CaptureRenderedFrame() ?? throw new InvalidOperationException($"The window {window.Title} drew no frame.")).ToPngImage();

   /// <summary>
   /// Waits until the UI thread has handled everything queued on it, such as the layout, rendering and input a click caused: a job queued at<br/>
   /// <see cref="DispatcherPriority.ApplicationIdle"/> runs only once nothing of a higher priority is waiting.
   /// </summary>
   void WaitUntilTheUIThreadIsIdle() => _uiThread.InvokeAsync(() => {}, DispatcherPriority.ApplicationIdle).GetTask().GetAwaiter().GetResult();
}
