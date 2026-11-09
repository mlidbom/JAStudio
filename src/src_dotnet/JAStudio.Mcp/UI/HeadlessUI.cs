using System;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using JAStudio.Core;
using JAStudio.Core.UI;
using JAStudio.Core.UI.Web;
using JAStudio.UI;

namespace JAStudio.Mcp.UI;

/// <summary>
/// JAStudio's UI where there is no screen, the headless JAStudio's: its windows open, are laid out and are drawn with Skia as on a screen, on<br/>
/// Avalonia's headless platform, and web pages, such as the workspace's note pages, are rendered as images by a browser (<see cref="WebPageImages"/>),<br/>
/// so that Claude can look at them, on the real corpus.
/// </summary>
/// <remarks>
/// It runs on a thread of its own, as the UI inside Anki does on the thread <c>JAStudioAnkiAppRoot</c> starts, so <see cref="Dispatcher.UIThread"/><br/>
/// is that thread here too. Avalonia sets its application up only once in a process, so a process starts one headless UI at most.
/// </remarks>
public sealed class HeadlessUI
{
   static readonly TimeSpan HowLongStartingMayTake = TimeSpan.FromSeconds(30);
   static int _startedInThisProcess;

   /// <summary>The dispatcher of the UI thread, which every window lives on.</summary>
   readonly Dispatcher _uiThread;

   HeadlessUI(Dispatcher uiThread, IWebPageImages webPageImages)
   {
      _uiThread = uiThread;
      WebPageImages = webPageImages;
   }

   /// <summary>How the headless UI renders web pages, which it has no native web view to show in.</summary>
   public IWebPageImages WebPageImages { get; }

   /// <summary>Opens a window with <paramref name="open"/>, on the UI thread, renders it as it is once it has handled what opening it started, and closes it.</summary>
   public PngImage Render(Func<Window> open) => _uiThread.Invoke(() =>
   {
      var window = open();
      _uiThread.RunJobs();
      var image = PngImageOf(window.CaptureRenderedFrame() ?? throw new InvalidOperationException($"The window {window.Title} drew no frame."));
      window.Close();
      return image;
   });

   /// <summary>
   /// Creates the workspace window of the JAStudio whose services are <paramref name="services"/>, as JAStudio inside Anki does as it starts,<br/>
   /// with its tabs showing their pages as images.
   /// </summary>
   public void CreateTheWorkspaceWindow(TemporaryServiceCollection services) =>
      _uiThread.Invoke(() => UIApp.InitializeWorkspaceWindowShowingPagesAsImages(services, WebPageImages));

   /// <summary>Starts the headless UI, rendering web pages with <paramref name="webPageImages"/>, and waits until Avalonia's application is set up on its thread.</summary>
   /// <remarks>The signal that it's set up isn't disposed: if setting up takes too long, the UI thread may still signal after the wait has given up.</remarks>
   public static HeadlessUI Start(IWebPageImages webPageImages)
   {
      if(Interlocked.Exchange(ref _startedInThisProcess, 1) == 1)
         throw new InvalidOperationException("A headless UI has been started in this process already: Avalonia sets its application up only once in a process.");

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
                ? new HeadlessUI(dispatcher, webPageImages)
                : throw new TimeoutException($"JAStudio's headless UI didn't start within {HowLongStartingMayTake.TotalSeconds}s.");
   }

   static PngImage PngImageOf(Bitmap frame)
   {
      using var png = new MemoryStream();
      frame.Save(png, PngBitmapEncoderOptions.Default);
      return PngImage.Of(png.ToArray());
   }
}
