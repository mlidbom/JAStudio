using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using JAStudio.Mcp.Discovery;
using JAStudio.Mcp.Gateway;
using JAStudio.Mcp.Gateway.Connections;
using JAStudio.Mcp.Gateway.Headless;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Specifications.Gateway;

/// <summary>
/// Claude's MCP gateway, wired as it runs (<see cref="GatewayServices"/>), in a stand-in for a source checkout, <c>checkout</c>, with the JAStudio of the<br/>
/// specification running on its corpus checkout, <c>checkout/src/jas_database</c>, as the standalone JAStudio: its MCP server announced among the<br/>
/// <see cref="RunningJAStudios"/>, in a folder of the specification's own, apart from the JAStudios really running.
/// </summary>
public abstract class GatewaySpecification : McpServerSpecification
{
   /// <summary>How long a specification waits at most for the gateway to follow a change to the running JAStudios.</summary>
   static readonly TimeSpan HowLongTheGatewayIsWaitedFor = TimeSpan.FromSeconds(10);

   readonly DirectoryInfo _folder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"JAStudio_gateway_{Guid.NewGuid():N}"));
   readonly ServiceProvider _gateway;

   protected GatewaySpecification()
   {
      var sourceCheckout = _folder.CreateSubdirectory("checkout");
      File.WriteAllText(Path.Combine(sourceCheckout.FullName, "JAStudio.slnx"), "<Solution />");
      AddOnFolder = sourceCheckout.CreateSubdirectory("src");
      var corpusCheckout = AddOnFolder.CreateSubdirectory("jas_database");
      var registryFolder = _folder.CreateSubdirectory("running_jastudios");

      RunningJAStudios = RunningJAStudios.In(registryFolder);
      TheJAStudio = RunningJAStudio.InThisProcess(JAStudioHost.Standalone, ServerUrl, corpusCheckout, AddOnFolder);
      ItsAnnouncement = RunningJAStudios.Announce(TheJAStudio);

      var services = new ServiceCollection();
      services.AddTheGateway(SourceCheckout.In(sourceCheckout.FullName), () => RunningJAStudios.In(registryFolder));
      _gateway = services.BuildServiceProvider();
   }

   /// <summary>The add-on folder of the stand-in source checkout, <c>checkout/src</c>.</summary>
   protected DirectoryInfo AddOnFolder { get; }

   /// <summary>The running JAStudios, as the JAStudios announce themselves among them.</summary>
   protected RunningJAStudios RunningJAStudios { get; }

   /// <summary>The JAStudio of the specification, as it announced itself.</summary>
   protected RunningJAStudio TheJAStudio { get; }

   protected RunningJAStudios.Announcement ItsAnnouncement { get; set; }

   ConnectedJAStudios ConnectedJAStudios => _gateway.GetRequiredService<ConnectedJAStudios>();

   protected string ReportOfTheRunningJAStudios() => ConnectedJAStudios.ReportOfTheRunningJAStudiosAsync(CancellationToken.None).GetAwaiter().GetResult();

   protected string ConnectTo(string? name) => ConnectedJAStudios.ConnectAsync(name, CancellationToken.None).GetAwaiter().GetResult();

   protected string DisconnectFrom(string name) => ConnectedJAStudios.DisconnectAsync(name).GetAwaiter().GetResult();

   /// <summary>The names, as the JAStudio has them, of the tools the gateway offers under the connection <paramref name="connection"/>, in alphabetical order.</summary>
   protected IReadOnlyList<string> ToolsOfferedUnder(string connection) =>
      [.. TheGatewaysTools.Select(it => it.ProtocolTool.Name).Where(it => it.StartsWith($"{connection}__", StringComparison.Ordinal)).Select(it => it[$"{connection}__".Length..]).Order(StringComparer.Ordinal)];

   /// <summary>The names of the JAStudio's tools, in alphabetical order.</summary>
   protected IReadOnlyList<string> TheJAStudiosTools() => [.. ListTools().Select(it => it.Name).Order(StringComparer.Ordinal)];

   /// <summary>Calls <paramref name="tool"/>, a tool the gateway offers, such as <c>standalone-checkout__describe_collection</c>, as Claude Code does, with no arguments.</summary>
   protected CallToolResult CallThroughTheGateway(string tool) =>
      TheGatewaysTools.OfType<JAStudioTool>().Single(it => it.ProtocolTool.Name == tool).CallInTheJAStudioAsync(new CallToolRequestParams { Name = tool }, CancellationToken.None).AsTask().GetAwaiter().GetResult();

   /// <summary>
   /// Waits until <paramref name="hasFollowed"/> holds, or <see cref="HowLongTheGatewayIsWaitedFor"/> has passed: the gateway follows the running<br/>
   /// JAStudios on a task of its own, woken as they change. The specification asserts what came of it.
   /// </summary>
   protected static void WaitUntilTheGatewayHasFollowed(Func<bool> hasFollowed)
   {
      var waited = Stopwatch.StartNew();
      while(!hasFollowed() && waited.Elapsed < HowLongTheGatewayIsWaitedFor) Thread.Sleep(20);
   }

   McpServerPrimitiveCollection<McpServerTool> TheGatewaysTools => GatewayServices.TheGatewaysTools(_gateway);

   public override void Dispose()
   {
      _gateway.DisposeAsync().AsTask().GetAwaiter().GetResult();
      ItsAnnouncement.Dispose();
      RunningJAStudios.Dispose();
      _folder.Delete(recursive: true);
      base.Dispose();
      GC.SuppressFinalize(this);
   }
}
