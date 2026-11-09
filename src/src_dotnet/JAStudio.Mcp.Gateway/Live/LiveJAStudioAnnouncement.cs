using System;
using System.IO;
using System.Text.Json;

namespace JAStudio.Mcp.Gateway.Live;

/// <summary>
/// What the live JAStudio, the one inside Anki, announces about its MCP server while the server runs, in a file of its own: the server's address,<br/>
/// the process Anki runs in, and the corpus checkout it works on. The gateway connects to the live JAStudio by it (<see cref="LiveJAStudio"/>).
/// </summary>
/// <remarks>
/// The live JAStudio writes it with <c>JAStudio.Mcp.Live.LiveJAStudioAnnouncement</c>, of which this is a copy that only reads: the gateway<br/>
/// references no JAStudio assembly, so that what changes with JAStudio needs no new gateway.
/// </remarks>
sealed record LiveJAStudioAnnouncement(Uri McpServerUrl, int ProcessId, string CorpusCheckout)
{
   static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

   /// <summary>The file the announcement is in: <c>live-jastudio.json</c> in JAStudio's folder of the local application data, <c>%LOCALAPPDATA%\JAStudio</c> on Windows.</summary>
   static FileInfo File => new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JAStudio", "live-jastudio.json"));

   /// <summary>The announcement of the live JAStudio, which fails when there's none: when JAStudio inside Anki doesn't run, or hasn't loaded its collection yet.</summary>
   public static LiveJAStudioAnnouncement Read() =>
      File.Exists
         ? JsonSerializer.Deserialize<LiveJAStudioAnnouncement>(System.IO.File.ReadAllText(File.FullName), JsonOptions) ?? throw new InvalidOperationException($"{File.FullName} announces nothing.")
         : throw new InvalidOperationException($"The live JAStudio doesn't run: there's no {File.FullName}, which JAStudio inside Anki writes once it has loaded its collection.");
}
