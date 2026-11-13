using System;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Compze.Internals.Logging;
using JAStudio.UI;

namespace JAStudio.StandaloneHost;

/// <summary>
/// The standalone JAStudio: JAStudio with its workspace window, as inside Anki, but with Anki entirely out of the loop. It's started on an add-on folder,<br/>
/// a source checkout's <c>src</c> folder, and works on it as JAStudio inside Anki does on the add-on folder Anki loads it from: on its corpus checkout,<br/>
/// its configuration, its window places and its workspace's state. So the JAStudios of two checkouts, such as main's and a branch's, can run side by<br/>
/// side, which Anki, one instance at a time, can't. Closing the workspace ends it. See <c>dev_docs/standalone-jastudio.md</c>.
/// </summary>
static class Program
{
   /// <summary>Avalonia's configuration of JAStudio's UI application, which Rider's Avalonia previewer calls too.</summary>
   public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<UIApp>().UsePlatformDetect().LogToTrace();

   /// <param name="args">The add-on folder to work on: a source checkout's <c>src</c> folder.</param>
   [STAThread]
   public static int Main(string[] args)
   {
      Console.OutputEncoding = Encoding.UTF8;
      CompzeLogger.LogLevel = LogLevel.Info;
      EndAtOnceOnAnExceptionNothingHandled();

      var jastudio = StandaloneJAStudio.StartOn(TheAddOnFolderIn(args));
      var exitCode = BuildAvaloniaApp()
                    .AfterSetup(_ => jastudio.ShowTheWorkspace())
                    .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
      jastudio.Stop();
      return exitCode;
   }

   /// <summary>
   /// Ends JAStudio at once on an exception nothing handled, as any program ends, writing the exception to the standard error and Windows' event log.
   /// </summary>
   /// <remarks>
   /// Otherwise the window stays open, hung: the generic hosts, JAStudio's and its card server's, hold the ending process for their shutdown timeouts,<br/>
   /// waiting to be disposed by code the exception skipped.
   /// </remarks>
   static void EndAtOnceOnAnExceptionNothingHandled() =>
      AppDomain.CurrentDomain.UnhandledException += (_, unhandled) =>
         Environment.FailFast("An exception nothing in the standalone JAStudio handled.", unhandled.ExceptionObject as Exception);

   static DirectoryInfo TheAddOnFolderIn(string[] args) =>
      args is [var path] && new DirectoryInfo(path) is { Exists: true } folder
         ? folder
         : throw new ArgumentException($"Expected the add-on folder to work on, a source checkout's src folder, that exists. Got: {string.Join(' ', args)}");
}
