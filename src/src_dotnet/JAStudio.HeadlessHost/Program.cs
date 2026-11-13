using System;
using System.Text;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Microsoft.Extensions.Hosting;
using Compze.Internals.Logging;
using JAStudio.Core;
using JAStudio.Mcp;
using JAStudio.Mcp.Discovery;
using JAStudio.Mcp.UI;
using JAStudio.UI.Views.Workspace;
using JAStudio.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JAStudio.HeadlessHost;

/// <summary>
/// The headless JAStudio: JAStudio without Anki or a screen, which loads a corpus checkout and serves JAStudio's MCP server, for Claude to work<br/>
/// with JAStudio on its own. The MCP gateway (<c>JAStudio.Mcp.Gateway</c>) starts it, from a copy of this build output, and restarts it to<br/>
/// take in a new build. See <c>dev_docs/jastudio-mcp.md</c>.
/// </summary>
static class Program
{
   /// <summary>
   /// What the headless JAStudio writes to standard output, followed by its MCP server's address, once its MCP server listens: the MCP gateway<br/>
   /// waits for it, and reads the address from it (<c>JAStudio.Mcp.Gateway.Headless.HeadlessJAStudioProcess</c>, which can't share this constant).
   /// </summary>
   const string McpServerListensAt = "JAStudio's MCP server listens at ";

   /// <summary>How long an action on the headless UI, such as a click, gives the work it started in the background, such as a search.</summary>
   static readonly TimeSpan HowLongBackgroundWorkIsWaitedFor = TimeSpan.FromMilliseconds(250);

   public static int Main(string[] args)
   {
      Console.OutputEncoding = Encoding.UTF8;
      CompzeLogger.LogLevel = LogLevel.Info;
      return args switch
      {
         [ReservoirCommands.CommandName, .. var reservoirCommand] => ReservoirCommands.Run(reservoirCommand),
         [TokenizerDictionaryCommands.CommandName, .. var tokenizerDictionaryCommand] => TokenizerDictionaryCommands.Run(tokenizerDictionaryCommand),
         _ => ServeJAStudiosMcpServer(HeadlessHostArguments.From(args))
      };
   }

   static int ServeJAStudiosMcpServer(HeadlessHostArguments arguments)
   {
      using var loaded = LoadedHeadlessJAStudio.From(arguments.Setup);
      var jastudio = loaded.Root;
      var services = jastudio.Resolve<TemporaryServiceCollection>();

      var cardServer = new CardServer(jastudio, new NoteLinkOpener(services));
      cardServer.StartOnAPortTheSystemPicks();

      using var webPageImages = new PlaywrightWebPageImages();
      var headlessUI = HeadlessUI.Start(webPageImages, HowLongBackgroundWorkIsWaitedFor);
      loaded.UIThreadDispatcher.UseTheThreadOf(headlessUI);
      headlessUI.CreateTheWorkspaceWindow(services);

      var mcpServer = JAStudioMcpServer.StartWithTheHeadlessUI(jastudio, arguments.McpServerPort, headlessUI);
      mcpServer.AnnounceAs(JAStudioHost.Headless);
      Console.WriteLine($"{McpServerListensAt}{mcpServer.Url}");

      arguments.ProcessToExitWith.WaitForExit();

      mcpServer.StopAsync().GetAwaiter().GetResult();
      cardServer.StopAsync().GetAwaiter().GetResult();
      return 0;
   }
}
