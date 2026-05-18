using System;
using Avalonia;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Microsoft.Extensions.Hosting;
using JAStudio.Core;
using Microsoft.Extensions.DependencyInjection;
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
      var resolver = host.Services.GetRequiredService<IRootResolver>();
      TemporaryServiceCollection.Instance = resolver.Resolve<TemporaryServiceCollection>();
      return BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
   }
}
