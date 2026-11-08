using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using JAStudio.Core;
using JAStudio.Core.Specifications;

namespace JAStudio.UI.Views.Specifications;

/// <summary>
/// A specification that opens JAStudio's windows: the application's real services with an empty collection, as for<br/>
/// <see cref="SpecificationStartingWithAnEmptyCollection"/>, and the windows on Avalonia's headless platform (<see cref="HeadlessUIApp"/>).<br/>
/// The windows are laid out and drawn as on a screen, so a specification can see whether they open at all, and <see cref="Render"/> them to an image to look at.
/// </summary>
/// <remarks>
/// Every window lives on the one headless UI thread of the process, so everything done with a window is done through <see cref="OnTheUIThread"/>.<br/>
/// A specification waits for work a window does in the background with <see cref="WaitUntil"/>, off the UI thread, since that work reports back on it.
/// </remarks>
public abstract class SpecificationOpeningWindows : SpecificationStartingWithAnEmptyCollection
{
   static readonly HeadlessUnitTestSession HeadlessSession = HeadlessUnitTestSession.StartNew(typeof(HeadlessUIApp), AvaloniaTestIsolationLevel.PerAssembly);

   /// <summary>The headless session's one thread, which every window lives on.</summary>
   static readonly Thread TheUIThread = HeadlessSession.Dispatch(() => Thread.CurrentThread, CancellationToken.None).GetAwaiter().GetResult();
   static readonly TimeSpan HowLongAWindowMayTake = TimeSpan.FromSeconds(30);

   /// <summary>The folder in the output folder that <see cref="Render"/> puts the images of the windows in.</summary>
   static readonly string RenderedWindowsFolder = Path.Combine(AppContext.BaseDirectory, "RenderedWindows");

   readonly List<Window> _openedWindows = [];

   protected TemporaryServiceCollection Services => GetService<TemporaryServiceCollection>();

   /// <summary>Opens a window on the UI thread with <paramref name="open"/>, as the application does, and closes it when the specification ends.</summary>
   protected TWindow Open<TWindow>(Func<TWindow> open) where TWindow : Window
   {
      var window = OnTheUIThread(open);
      _openedWindows.Add(window);
      return window;
   }

   /// <summary>The view model <paramref name="window"/> shows, its <see cref="StyledElement.DataContext"/>.</summary>
   protected static TViewModel ViewModelOf<TViewModel>(Window window) where TViewModel : class =>
      OnTheUIThread(() => window.DataContext as TViewModel ?? throw new InvalidOperationException($"The window shows no {typeof(TViewModel).Name}."));

   protected static void OnTheUIThread(Action action) => OnTheUIThread(() =>
   {
      action();
      return true;
   });

   /// <summary>
   /// Runs <paramref name="function"/> on the UI thread and returns its result: at once when called on the UI thread, as from within another<br/>
   /// <see cref="OnTheUIThread{TResult}"/>, which would otherwise wait for itself.
   /// </summary>
   protected static TResult OnTheUIThread<TResult>(Func<TResult> function) =>
      Thread.CurrentThread == TheUIThread ? function() : HeadlessSession.Dispatch(function, CancellationToken.None).GetAwaiter().GetResult();

   /// <summary>Waits until <paramref name="condition"/>, read on the UI thread, holds: as when a window has finished work it started in the background.</summary>
   protected static void WaitUntil(Func<bool> condition)
   {
      var waited = Stopwatch.StartNew();
      while(!OnTheUIThread(condition))
      {
         if(waited.Elapsed > HowLongAWindowMayTake) throw new TimeoutException($"The windows didn't get there within {HowLongAWindowMayTake.TotalSeconds}s.");
         Thread.Sleep(10);
      }
   }

   /// <summary>
   /// Renders <paramref name="window"/> as it is now to <c>RenderedWindows/<paramref name="name"/>.png</c> in the output folder, to look at,<br/>
   /// and returns the image's path.
   /// </summary>
   protected static string Render(Window window, string name) => OnTheUIThread(() =>
   {
      Dispatcher.UIThread.RunJobs();
      var frame = window.CaptureRenderedFrame() ?? throw new InvalidOperationException($"The window rendered as {name} drew no frame.");
      Directory.CreateDirectory(RenderedWindowsFolder);
      var imagePath = Path.Combine(RenderedWindowsFolder, $"{name}.png");
      frame.Save(imagePath, PngBitmapEncoderOptions.Default);
      return imagePath;
   });

   public override void Dispose()
   {
      OnTheUIThread(() => _openedWindows.ForEach(it => it.Close()));
      base.Dispose();
      GC.SuppressFinalize(this);
   }
}
