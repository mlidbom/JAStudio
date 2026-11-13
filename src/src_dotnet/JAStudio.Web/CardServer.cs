using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Extensions.Hosting;
using JAStudio.Core.Note;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.UI.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace JAStudio.Web;

/// <summary>
/// Hosts a Blazor Server application on localhost.
/// Started in-process by the Anki addon via pythonnet.
/// The iframe in the Anki card template points at this server.
/// </summary>
public class CardServer
{
   readonly IRootResolver _parentResolver;
   readonly INoteLinkOpener _noteLinkOpener;
   WebApplication? _app;

   public CardServer(IRootResolver parentResolver, INoteLinkOpener noteLinkOpener)
   {
      _parentResolver = parentResolver;
      _noteLinkOpener = noteLinkOpener;
   }

   public int Port { get; private set; }

   /// <summary>
   /// The address the card pages are asked for at: the IPv4 loopback address, which the server listens on, not <c>localhost</c>.
   /// </summary>
   /// <remarks>
   /// On Windows, <c>localhost</c> resolves to the IPv6 loopback address first, which the server doesn't listen on, and a client such as<br/>
   /// <see cref="System.Net.Http.HttpClient"/> waits about 2 seconds for that connection to be refused before trying the IPv4 one: 2 seconds<br/>
   /// for every connection, which the CardServer's specifications spent in each of them.
   /// </remarks>
   public string BaseUrl => $"http://127.0.0.1:{Port}";

   /// <summary>
   /// Starts the Kestrel web server, reusing the previously saved port if available.
   /// Falls back to an OS-assigned port if the saved port is unavailable.
   /// Call from Python on addon startup.
   /// </summary>
   // ReSharper disable once UnusedMember.Global — called from Python
   public void Start()
   {
      // Prevent ASP.NET from scanning for hosting startup assemblies (we're hosted in Anki's process)
      Environment.SetEnvironmentVariable("ASPNETCORE_PREVENTHOSTINGSTARTUP", "true");

      StartOnThePortItHadLastOrAnother();
      SavePort(Port);
   }

   /// <summary>
   /// Starts the Kestrel web server on a port the system picks, leaving alone the port the server inside Anki remembers, which <see cref="Start"/><br/>
   /// reads and saves: how the headless JAStudio starts its own, while Anki's may be running.
   /// </summary>
   public void StartOnAPortTheSystemPicks() => StartOnPort(0);

   void StartOnThePortItHadLastOrAnother()
   {
      var preferredPort = LoadSavedPort();
      if(preferredPort > 0)
      {
         try
         {
            StartOnPort(preferredPort);
            return;
         }
         catch(IOException)
         {
            Console.WriteLine($"[CardServer] Port {preferredPort} unavailable, falling back to OS-assigned port");
         }
      }

      StartOnPort(0);
   }

   void StartOnPort(int port)
   {
      var assemblyDir = Path.GetDirectoryName(typeof(CardServer).Assembly.Location)!;
      var builder = WebApplication.CreateBuilder(new WebApplicationOptions
      {
         EnvironmentName = "Production",
         ApplicationName = typeof(CardServer).Assembly.GetName().Name!,
         ContentRootPath = assemblyDir,
         WebRootPath = Path.Combine(assemblyDir, "wwwroot"),
      });
      builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, port));
      builder.WebHost.UseStaticWebAssets();

      builder.Services.AddRazorComponents()
                      .AddInteractiveServerComponents();

      builder.Services.AddMudServices();
      builder.Services.AddSingleton<WebRootFiles>();

      builder.Services.AddCors(options =>
         options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod()));

      // Hook the parent Compze container in as the service provider for this child host.
      // Singletons delegate to the parent; ASP.NET Core / MudBlazor / Razor services
      // registered above are added on top.
      _parentResolver.Resolve<IChildContainerHostIntegration>()
                     .UseChildContainerAsServiceProviderFor(builder.Host);

      _app = builder.Build();

      _app.UseCors();
      _app.UseStaticFiles();
      _app.UseAntiforgery();

      // UseAntiforgery() adds 'frame-ancestors: self' which blocks cross-origin iframes.
      // Override it so Anki's reviewer webview (on a different port) can frame our pages.
      _app.Use(async (context, next) =>
      {
         context.Response.OnStarting(() =>
         {
            context.Response.Headers["Content-Security-Policy"] = "frame-ancestors *";
            return Task.CompletedTask;
         });
         await next();
      });

      _app.MapRazorComponents<Components.App>()
          .AddInteractiveServerRenderMode();

      ConfigureMediaEndpoint(_app);
      ConfigureOpenInBrowserEndpoint(_app);
      ConfigureOpenInWorkspaceEndpoint(_app);
      ConfigureOpenInPreviewerEndpoint(_app);

      try
      {
         _app.StartAsync().GetAwaiter().GetResult();
      }
      catch
      {
         _app.DisposeAsync().AsTask().GetAwaiter().GetResult();
         _app = null;
         throw;
      }

      // Resolve the actual port assigned by the OS
      var address = _app.Urls.First();
      Port = new Uri(address).Port;
      CardServerUrl.BaseUrl = BaseUrl;
      Console.WriteLine($"[CardServer] Listening on {BaseUrl}");
   }

   static void ConfigureMediaEndpoint(WebApplication app)
   {
      // Serve media files from the storage directory by original filename.
      // URL: /media?name={filename}
      app.MapGet("/media", (string name, MediaFileIndex index) =>
      {
         var attachment = index.TryGetByOriginalFileName(name)
                       ?? throw new InvalidOperationException($"No media file found for: {name}");

         var contentType = Path.GetExtension(attachment.FilePath).ToLowerInvariant() switch
         {
            ".mp3" => "audio/mpeg",
            ".ogg" => "audio/ogg",
            ".wav" => "audio/wav",
            ".m4a" => "audio/mp4",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
         };

         return Results.File(attachment.FilePath, contentType, enableRangeProcessing: true);
      });
   }

   void ConfigureOpenInBrowserEndpoint(WebApplication app)
   {
      // API endpoint for opening a note card in the system browser.
      // Called by card.js on Shift+Click on a NoteLink.
      app.MapGet("/api/open-in-browser/{noteType}/{noteId}", (string noteType, string noteId) =>
      {
         var url = $"{BaseUrl}/card/{noteType}/back?NoteId={noteId}";
         Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
         return Results.Ok();
      });
   }

   void ConfigureOpenInWorkspaceEndpoint(WebApplication app)
   {
      // API endpoint for opening a note in JAStudio's workspace.
      // Called by card.js on a plain click on a NoteLink inside Anki's reviewer.
      app.MapGet("/api/open-in-workspace/{noteId}", (string noteId) =>
      {
         _noteLinkOpener.OpenInWorkspace(NoteId.Parse(noteId));
         return Results.Ok();
      });
   }

   void ConfigureOpenInPreviewerEndpoint(WebApplication app)
   {
      // API endpoint for opening a note in the Anki previewer.
      // Called by card.js on Ctrl+Click on a NoteLink.
      app.MapGet("/api/open-in-previewer/{noteId}", (string noteId) =>
      {
         _noteLinkOpener.OpenInAnkiPreviewer(NoteId.Parse(noteId));
         return Results.Ok();
      });
   }

   /// <summary>
   /// Stops the web server. Call on addon shutdown.
   /// </summary>
   // ReSharper disable once UnusedMember.Global — called from Python
   public async Task StopAsync()
   {
      if(_app != null)
      {
         await _app.StopAsync(CancellationToken.None);
         await _app.DisposeAsync();
         _app = null;
         Console.WriteLine("[CardServer] Stopped");
      }
   }

   static string PortFilePath => Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
      "JAStudio",
      "cardserver-port.txt");

   /// <summary>
   /// Returns the previously saved port, or 0 (let OS pick) if none saved or the saved port is unavailable.
   /// </summary>
   static int LoadSavedPort()
   {
      try
      {
         if(!File.Exists(PortFilePath)) return 0;
         var text = File.ReadAllText(PortFilePath).Trim();
         return int.TryParse(text, out var port) && port > 0 ? port : 0;
      }
      catch
      {
         return 0;
      }
   }

   static void SavePort(int port)
   {
      try
      {
         var dir = Path.GetDirectoryName(PortFilePath)!;
         Directory.CreateDirectory(dir);
         File.WriteAllText(PortFilePath, port.ToString());
      }
      catch(Exception ex)
      {
         Console.WriteLine($"[CardServer] Failed to save port: {ex.Message}");
      }
   }
}
