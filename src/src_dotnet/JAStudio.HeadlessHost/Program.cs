using System;
using System.Text;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Microsoft.Extensions.Hosting;
using Compze.Internals.Logging;
using JAStudio.Core;
using JAStudio.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JAStudio.HeadlessHost;

/// <summary>
/// The headless JAStudio: JAStudio without Anki or a UI, which loads a corpus checkout and serves JAStudio's MCP server, for Claude to work<br/>
/// with JAStudio on its own. The MCP gateway (<c>JAStudio.Mcp.Gateway</c>) starts it, from a copy of this build output, and restarts it to<br/>
/// take in a new build. See <c>dev_docs/jastudio-mcp.md</c>.
/// </summary>
static class Program
{
   /// <summary>
   /// What the headless JAStudio writes to standard output, followed by its MCP server's address, once its MCP server listens: the MCP gateway<br/>
   /// waits for it, and reads the address from it (<c>JAStudio.Mcp.Gateway.HeadlessJAStudioProcess</c>, which can't share this constant).
   /// </summary>
   const string McpServerListensAt = "JAStudio's MCP server listens at ";

   public static int Main(string[] args)
   {
      Console.OutputEncoding = Encoding.UTF8;
      CompzeLogger.LogLevel = LogLevel.Info;
      var arguments = HeadlessHostArguments.From(args);

      using var host = BuildJAStudio(arguments);
      var jastudio = StartJAStudioAndLoadTheCollection(host);

      var mcpServer = JAStudioMcpServer.Start(jastudio, arguments.McpServerPort);
      Console.WriteLine($"{McpServerListensAt}{mcpServer.Url}");

      arguments.ProcessToExitWith.WaitForExit();

      mcpServer.StopAsync().GetAwaiter().GetResult();
      host.StopAsync().GetAwaiter().GetResult();
      return 0;
   }

   static IHost BuildJAStudio(HeadlessHostArguments arguments)
   {
      var plan = AppBootstrapper.PrepareProduction(new HeadlessEnvironmentDependenciesRegistrar(arguments));
      return Host.CreateDefaultBuilder()
                 .UseServiceProviderFactory(new MicrosoftServiceProviderFactory(plan.Builder))
                 .Build();
   }

   static IRootResolver StartJAStudioAndLoadTheCollection(IHost host)
   {
      host.StartAsync().GetAwaiter().GetResult();
      var jastudio = host.Services.GetRequiredService<IRootResolver>();
      TemporaryServiceCollection.Instance = jastudio.Resolve<TemporaryServiceCollection>();
      jastudio.Resolve<CoreApp>().Collection.ReloadFromBackend();
      return jastudio;
   }
}
