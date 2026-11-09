using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Extensions.Hosting;
using JAStudio.Core.SysUtils.Json;
using JAStudio.Mcp.Tools;
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
/// computer only. The same server runs in each JAStudio that starts it: the headless JAStudio (<c>JAStudio.HeadlessHost</c>) now, and the one<br/>
/// inside Anki later.
/// </summary>
/// <remarks>
/// Claude Code doesn't connect to it directly: the MCP gateway (<c>JAStudio.Mcp.Gateway</c>) does, and forwards its tools to Claude Code,<br/>
/// so the JAStudio behind it can be restarted without Claude Code losing them. The design is in <c>dev_docs/jastudio-mcp.md</c>.
/// </remarks>
public sealed class JAStudioMcpServer
{
   /// <summary>How the tools' results are written as JSON: the SDK's own options, but with Japanese written as is, not escaped.</summary>
   static readonly JsonSerializerOptions ToolResultJsonOptions = new(McpJsonUtilities.DefaultOptions) { Encoder = FullUnicodeJsonEncoder.Instance };

   readonly WebApplication _webApplication;

   JAStudioMcpServer(WebApplication startedWebApplication)
   {
      _webApplication = startedWebApplication;
      Url = new Uri(startedWebApplication.Urls.First());
   }

   /// <summary>The address the MCP endpoint listens at.</summary>
   public Uri Url { get; }

   /// <summary>
   /// Starts serving the tools, which use the services of <paramref name="jastudio"/>, such as its collection, on <paramref name="port"/> of this<br/>
   /// computer's loopback address, or on a free port the system picks when it's 0.
   /// </summary>
   public static JAStudioMcpServer Start(IRootResolver jastudio, int port)
   {
      var webApplication = WebApplicationServingTheTools(jastudio, port);
      webApplication.StartAsync().GetAwaiter().GetResult();
      return new JAStudioMcpServer(webApplication);
   }

   public async Task StopAsync()
   {
      await _webApplication.StopAsync(CancellationToken.None);
      await _webApplication.DisposeAsync();
   }

   static WebApplication WebApplicationServingTheTools(IRootResolver jastudio, int port)
   {
      var builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
         EnvironmentName = "Production",
         ApplicationName = typeof(JAStudioMcpServer).Assembly.GetName().Name
      });
      builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, port));
      builder.Logging.SetMinimumLevel(LogLevel.Warning);

      builder.Services.AddMcpServer(options => options.ServerInfo = new Implementation { Name = "JAStudio", Version = "1" })
             .WithHttpTransport(options => options.SessionMode = HttpServerSessionMode.Stateless)
             .WithRequestFilters(filters => filters.AddCallToolFilter(ToolExceptionReporting.ReportsTheWholeException))
             .WithTools<NoteTools>(ToolResultJsonOptions)
             .WithTools<CollectionTools>(ToolResultJsonOptions);

      jastudio.Resolve<IChildContainerHostIntegration>().UseChildContainerAsServiceProviderFor(builder.Host);

      var webApplication = builder.Build();
      webApplication.MapMcp();
      return webApplication;
   }
}
