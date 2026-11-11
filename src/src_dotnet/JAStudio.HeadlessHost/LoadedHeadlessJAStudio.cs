using System;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Microsoft.Extensions.Hosting;
using JAStudio.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JAStudio.HeadlessHost;

/// <summary>
/// The headless JAStudio, built for a <see cref="HeadlessJAStudioSetup"/>, started, and with its collection loaded, whether it goes on to serve<br/>
/// JAStudio's MCP server or to run a batch command. <see cref="Dispose"/> stops it.
/// </summary>
sealed class LoadedHeadlessJAStudio : IDisposable
{
   readonly IHost _host;

   LoadedHeadlessJAStudio(IHost host, IRootResolver root)
   {
      _host = host;
      Root = root;
   }

   public static LoadedHeadlessJAStudio From(HeadlessJAStudioSetup setup)
   {
      HeadlessUserFiles.ReplaceWithACopyOfTheUserFilesOf(setup.AddonFolder);
      var plan = AppBootstrapper.PrepareProduction(new HeadlessEnvironmentDependenciesRegistrar(setup));
      var host = Host.CreateDefaultBuilder()
                     .UseServiceProviderFactory(new MicrosoftServiceProviderFactory(plan.Builder))
                     .Build();
      host.StartAsync().GetAwaiter().GetResult();
      var root = host.Services.GetRequiredService<IRootResolver>();
      root.Resolve<CoreApp>().Collection.ReloadFromBackend();
      return new LoadedHeadlessJAStudio(host, root);
   }

   /// <summary>JAStudio's services, to resolve what's needed from.</summary>
   public IRootResolver Root { get; }

   public void Dispose()
   {
      _host.StopAsync().GetAwaiter().GetResult();
      _host.Dispose();
   }
}
