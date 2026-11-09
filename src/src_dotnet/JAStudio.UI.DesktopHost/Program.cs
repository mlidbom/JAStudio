using System;
using Avalonia;
using Compze.DependencyInjection.Microsoft.Extensions.Hosting;
using JAStudio.Core;
using Microsoft.Extensions.Hosting;

namespace JAStudio.UI.DesktopHost;

class Program
{
   // Avalonia configuration, don't remove; also used by visual designer.
   public static AppBuilder BuildAvaloniaApp()
      => AppBuilder.Configure<UIApp>()
                   .UsePlatformDetect()
                   .LogToTrace();

   // The entry point. Things aren't ready yet, so at this point
   // you shouldn't use any Avalonia types or anything that expects
   // a SynchronizationContext to be ready
   [STAThread]
   public static int Main(string[] args)
   {
      var plan = AppBootstrapper.PrepareForTests();
      using var host = Host.CreateDefaultBuilder(args)
                           .UseServiceProviderFactory(new MicrosoftServiceProviderFactory(plan.Builder))
                           .Build();
      host.StartAsync().GetAwaiter().GetResult();
      //todo: Make this run JAStudio without Anki, on the real corpus (Magnus, 2026-11-09). Today it starts the specifications' services
      //   (PrepareForTests), gives them to nothing, and opens no window: UIApp opens the workspace only from UIApp.InitializeWorkspaceWindow.
      //   JAStudio.HeadlessHost's Program shows the rest: the production services, loading the collection, and starting a CardServer.
      //   Rider's Avalonia previewer uses this project too (.idea/.idea.JAStudio/.idea/avalonia.xml), but calls only BuildAvaloniaApp.
      return BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
   }
}
