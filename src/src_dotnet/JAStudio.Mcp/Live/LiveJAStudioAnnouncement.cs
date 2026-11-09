using System;
using System.IO;
using System.Text.Json;

namespace JAStudio.Mcp.Live;

/// <summary>
/// What the live JAStudio, the one inside Anki, announces about its MCP server while the server runs, in a file of its own: the server's address,<br/>
/// the process Anki runs in, and the corpus checkout it works on. The MCP gateway that serves the live JAStudio's tools to Claude Code,<br/>
/// <c>jastudio-live</c> in <c>.mcp.json</c>, connects by it.
/// </summary>
/// <remarks>The gateway references no JAStudio assembly, so it reads the file with a copy of this type: <c>JAStudio.Mcp.Gateway.Live.LiveJAStudioAnnouncement</c>.</remarks>
public record LiveJAStudioAnnouncement(Uri McpServerUrl, int ProcessId, string CorpusCheckout)
{
   static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

   /// <summary>The file the announcement is in: <c>live-jastudio.json</c> in JAStudio's folder of the local application data, <c>%LOCALAPPDATA%\JAStudio</c> on Windows.</summary>
   public static FileInfo File => new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JAStudio", "live-jastudio.json"));

   public void Write()
   {
      Directory.CreateDirectory(File.DirectoryName ?? throw new InvalidOperationException($"{File.FullName} is in no folder."));
      System.IO.File.WriteAllText(File.FullName, JsonSerializer.Serialize(this, JsonOptions));
   }

   /// <summary>Deletes the announcement's file, once the server it announces has stopped.</summary>
   public static void Withdraw() => File.Delete();
}
