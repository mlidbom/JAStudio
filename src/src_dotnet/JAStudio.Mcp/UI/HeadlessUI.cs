using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using JAStudio.UI;

namespace JAStudio.Mcp.UI;

/// <summary>
/// JAStudio's UI on Avalonia's headless platform: its windows open, are laid out and are drawn with Skia as on a screen, but on no screen, so<br/>
/// the headless JAStudio can render them, on the real corpus, to images Claude looks at.
/// </summary>
/// <remarks>
/// It runs on a thread of its own, as the UI inside Anki does on the thread <c>JAStudioAnkiAppRoot</c> starts, so <see cref="Dispatcher.UIThread"/><br/>
/// is that thread here too. Avalonia sets its application up once in a process, so there is one headless UI a process, started the first time<br/>
/// it's asked for.
/// </remarks>
public sealed class HeadlessUI
{
   static readonly TimeSpan HowLongStartingMayTake = TimeSpan.FromSeconds(30);
   static readonly Lazy<HeadlessUI> TheOneOfThisProcess = new(Start);

   /// <summary>The dispatcher of the UI thread, which every window lives on.</summary>
   readonly Dispatcher _uiThread;

   HeadlessUI(Dispatcher uiThread) => _uiThread = uiThread;

   /// <summary>The headless UI of this process, started now if it isn't yet.</summary>
   public static HeadlessUI OfThisProcess() => TheOneOfThisProcess.Value;

   /// <summary>Opens a window with <paramref name="open"/>, on the UI thread, renders it as it is once it has handled what opening it started, and closes it.</summary>
   public WindowImage Render(Func<Window> open) => _uiThread.Invoke(() =>
   {
      var window = open();
      _uiThread.RunJobs();
      var image = WindowImage.Of(window.CaptureRenderedFrame() ?? throw new InvalidOperationException($"The window {window.Title} drew no frame."));
      window.Close();
      return image;
   });

   /// <summary>Starts the UI thread, and waits until Avalonia's application is set up on it.</summary>
   /// <remarks>The signal that it's set up isn't disposed: if setting up takes too long, the UI thread may still signal after the wait has given up.</remarks>
   static HeadlessUI Start()
   {
      var setUp = new ManualResetEventSlim();
      Dispatcher? uiThreadDispatcher = null;
      var uiThread = new Thread(() =>
      {
         AppBuilder.Configure<UIApp>()
                   .UseSkia()
                   .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                   .SetupWithoutStarting();
         uiThreadDispatcher = Dispatcher.UIThread;
         setUp.Set();
         Dispatcher.UIThread.MainLoop(CancellationToken.None);
      })
      {
         IsBackground = true,
         Name = "HeadlessAvaloniaUIThread"
      };
      uiThread.Start();

      return setUp.Wait(HowLongStartingMayTake) && uiThreadDispatcher is {} dispatcher
                ? new HeadlessUI(dispatcher)
                : throw new TimeoutException($"JAStudio's headless UI didn't start within {HowLongStartingMayTake.TotalSeconds}s.");
   }
}
