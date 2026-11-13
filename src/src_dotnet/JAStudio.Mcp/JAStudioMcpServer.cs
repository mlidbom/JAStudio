using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Compze.Contracts;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Extensions.Hosting;
using JAStudio.Core;
using JAStudio.Core.SysUtils.Json;
using JAStudio.Mcp.Corpus;
using JAStudio.Mcp.Discovery;
using JAStudio.Mcp.Tools.Analysis;
using JAStudio.Mcp.Tools.Cards;
using JAStudio.Mcp.Tools.Collection;
using JAStudio.Mcp.Tools.Corpus;
using JAStudio.Mcp.Tools.Dictionary;
using JAStudio.Mcp.Tools.Editing;
using JAStudio.Mcp.Tools.Matching;
using JAStudio.Mcp.Tools.Memory;
using JAStudio.Mcp.Tools.Notes;
using JAStudio.Mcp.Tools.Rebuilding;
using JAStudio.Mcp.Tools.Reservoir;
using JAStudio.Mcp.Tools.Saving;
using JAStudio.Mcp.Tools.Reloading;
using JAStudio.Mcp.Tools.Screen;
using JAStudio.Mcp.Tools.StudyNotes;
using JAStudio.Mcp.Tools.Tagging;
using JAStudio.Mcp.Tools.Windows;
using JAStudio.Mcp.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Protocol;

namespace JAStudio.Mcp;

/// <summary>
/// JAStudio's MCP server: the tools Claude works with JAStudio through, such as <see cref="NoteTools.SearchNotes"/>, served over HTTP to this<br/>
/// computer only. The same server runs in every JAStudio: inside Anki and in the standalone JAStudio (<see cref="Live.LiveJAStudioMcpServer"/>), and<br/>
/// in the headless JAStudio (<c>JAStudio.HeadlessHost</c>). Each announces it among the <see cref="RunningJAStudios"/> (<see cref="AnnounceAs"/>).
/// </summary>
/// <remarks>
/// Claude Code doesn't connect to it directly: Claude's MCP gateway (<c>JAStudio.Mcp.Gateway</c>) does, finding it among the running JAStudios,<br/>
/// and forwards its tools to Claude Code, so the JAStudio behind it can be restarted without Claude Code losing them. The design is in<br/>
/// <c>dev_docs/jastudio-mcp.md</c>.
/// </remarks>
public sealed partial class JAStudioMcpServer
{
   /// <summary>How the tools' results are written as JSON: the SDK's own options, but with Japanese written as is, not escaped.</summary>
   static readonly JsonSerializerOptions ToolResultJsonOptions = new(McpJsonUtilities.DefaultOptions) { Encoder = FullUnicodeJsonEncoder.Instance };

   readonly IRootResolver _jastudio;
   readonly WebApplication _webApplication;
   AnnouncementOfTheServer? _announcement;

   JAStudioMcpServer(IRootResolver jastudio, WebApplication startedWebApplication)
   {
      _jastudio = jastudio;
      _webApplication = startedWebApplication;
      Url = new Uri(startedWebApplication.Urls.First());
   }

   /// <summary>The address the MCP endpoint listens at.</summary>
   public Uri Url { get; }

   /// <summary>
   /// Starts serving the tools, which use the services of <paramref name="jastudio"/>, such as its collection, on <paramref name="port"/> of this<br/>
   /// computer's loopback address, or on a free port the system picks when it's 0.
   /// </summary>
   public static JAStudioMcpServer Start(IRootResolver jastudio, int port) => Start(jastudio, port, headlessUI: null);

   /// <summary>
   /// Starts serving the tools, as <see cref="Start(IRootResolver,int)"/> does, and the tools of a JAStudio whose UI is <paramref name="headlessUI"/>,<br/>
   /// such as <see cref="WindowTools.RenderWindow"/> and <see cref="CardTools.RenderCard"/>: the headless JAStudio's.
   /// </summary>
   public static JAStudioMcpServer StartWithTheHeadlessUI(IRootResolver jastudio, int port, HeadlessUI headlessUI) => Start(jastudio, port, headlessUI);

   /// <summary>Starts serving the tools, and the tools of a headless UI too when there is one, <paramref name="headlessUI"/>.</summary>
   static JAStudioMcpServer Start(IRootResolver jastudio, int port, HeadlessUI? headlessUI)
   {
      var webApplication = WebApplicationServingTheTools(jastudio, port, headlessUI);
      webApplication.StartAsync().GetAwaiter().GetResult();
      return new JAStudioMcpServer(jastudio, webApplication);
   }

   /// <summary>
   /// Announces the server among the <see cref="RunningJAStudios"/> of this user, as the MCP server of a JAStudio hosted by <paramref name="host"/>,<br/>
   /// until it stops, for Claude's MCP gateway to find and connect to. Specifications don't: they'd show among the JAStudios really running.
   /// </summary>
   public void AnnounceAs(JAStudioHost host)
   {
      Contract.State.Assert(_announcement == null, () => "The server is announced already.");
      _announcement = AnnouncementOfTheServer.Make(host, Url, _jastudio.Resolve<IEnvironmentPaths>());
   }

   /// <summary>Stops the server, withdrawing its announcement first, if it was announced.</summary>
   public async Task StopAsync()
   {
      _announcement?.Dispose();
      await _webApplication.StopAsync(CancellationToken.None);
      await _webApplication.DisposeAsync();
   }

   static WebApplication WebApplicationServingTheTools(IRootResolver jastudio, int port, HeadlessUI? headlessUI)
   {
      var builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
         EnvironmentName = "Production",
         ApplicationName = typeof(JAStudioMcpServer).Assembly.GetName().Name
      });
      builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, port));
      builder.Logging.SetMinimumLevel(LogLevel.Warning);

      var mcpServer = builder.Services.AddMcpServer(options => options.ServerInfo = new Implementation { Name = "JAStudio", Version = "1" })
             .WithHttpTransport(options => options.SessionMode = HttpServerSessionMode.Stateless)
             .WithRequestFilters(filters => filters.AddCallToolFilter(ToolExceptionReporting.ReportsTheWholeException))
             .WithTools<NoteTools>(ToolResultJsonOptions)
             .WithTools<CollectionTools>(ToolResultJsonOptions)
             .WithTools<TagTools>(ToolResultJsonOptions)
             .WithTools<CorpusTools>(ToolResultJsonOptions)
             .WithTools<RebuildTools>(ToolResultJsonOptions)
             .WithTools<SaveTools>(ToolResultJsonOptions)
             .WithTools<ReloadTools>(ToolResultJsonOptions)
             .WithTools<AnalysisTools>(ToolResultJsonOptions)
             .WithTools<DictionaryTools>(ToolResultJsonOptions)
             .WithTools<NoteEditingTools>(ToolResultJsonOptions)
             .WithTools<NoteCommandTools>(ToolResultJsonOptions)
             .WithTools<NoteCreationTools>(ToolResultJsonOptions)
             .WithTools<UniversalNoteActionTools>(ToolResultJsonOptions)
             .WithTools<StudyNoteTools>(ToolResultJsonOptions)
             .WithTools<ReservoirTools>(ToolResultJsonOptions)
             .WithTools<MemoryTools>(ToolResultJsonOptions)
             .WithTools<SharedConditionTools>(ToolResultJsonOptions);
      builder.Services.AddSingleton<CorpusCheckout>();
      builder.Services.AddSingleton<OpenReservoirs>();

      if(headlessUI != null)
      {
         mcpServer.WithTools<WindowTools>(ToolResultJsonOptions)
                  .WithTools<ScreenTools>(ToolResultJsonOptions)
                  .WithTools<CardTools>(ToolResultJsonOptions);
         builder.Services.AddSingleton(headlessUI);
      }

      jastudio.Resolve<IChildContainerHostIntegration>().UseChildContainerAsServiceProviderFor(builder.Host);

      var webApplication = builder.Build();
      webApplication.MapMcp();
      return webApplication;
   }
}
