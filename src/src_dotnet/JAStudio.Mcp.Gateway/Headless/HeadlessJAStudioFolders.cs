using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JAStudio.Mcp.Gateway.SysUtils.IO;

namespace JAStudio.Mcp.Gateway.Headless;

/// <summary>
/// The folders the gateway keeps the headless JAStudio's files in, under <c>%LOCALAPPDATA%\JAStudio\headless</c>: the copy of its build output<br/>
/// it runs from, and the logs of its runs.
/// </summary>
static class HeadlessJAStudioFolders
{
   const string RuntimeFolderPrefix = "runtime-of-gateway-";

   static DirectoryInfo HeadlessFolder => new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JAStudio", "headless"));

   /// <summary>
   /// The folder this gateway runs the headless JAStudio from: a copy of its build output, so that building JAStudio while it runs doesn't fail<br/>
   /// on the files it has loaded, which Windows locks. Each gateway, one per Claude Code session, has its own, named by its process id.
   /// </summary>
   static DirectoryInfo RuntimeFolder => new(Path.Combine(HeadlessFolder.FullName, $"{RuntimeFolderPrefix}{Environment.ProcessId}"));

   /// <summary>A new log file for a run of the headless JAStudio, named by when it started.</summary>
   public static FileInfo NewLogFile()
   {
      var logsFolder = Directory.CreateDirectory(Path.Combine(HeadlessFolder.FullName, "logs"));
      return new FileInfo(Path.Combine(logsFolder.FullName, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.log"));
   }

   /// <summary>Replaces the contents of <see cref="RuntimeFolder"/> with a copy of <paramref name="buildOutput"/>, and returns it. The headless JAStudio must not be running from it.</summary>
   public static async Task<DirectoryInfo> RuntimeFolderWithACopyOfAsync(DirectoryInfo buildOutput)
   {
      var runtimeFolder = RuntimeFolder;
      await runtimeFolder.DeleteOnceItsFilesAreReleasedAsync();
      buildOutput.CopyEverythingTo(runtimeFolder);
      return runtimeFolder;
   }

   /// <summary>Deletes this gateway's <see cref="RuntimeFolder"/>, once the headless JAStudio no longer runs from it.</summary>
   public static Task DeleteTheRuntimeFolderAsync() => RuntimeFolder.DeleteOnceItsFilesAreReleasedAsync();

   /// <summary>Deletes the runtime folders of gateways that are no longer running, which a gateway that was killed leaves behind.</summary>
   public static async Task DeleteTheRuntimeFoldersOfGatewaysThatExitedAsync()
   {
      var headlessFolder = HeadlessFolder;
      if(!headlessFolder.Exists) return;

      var runningProcessIds = Process.GetProcesses().Select(it => it.Id).ToHashSet();
      foreach(var runtimeFolderOfAGatewayThatExited in headlessFolder.EnumerateDirectories($"{RuntimeFolderPrefix}*")
                                                                     .Where(it => !runningProcessIds.Contains(ProcessIdOfTheGatewayOf(it))))
      {
         await runtimeFolderOfAGatewayThatExited.DeleteOnceItsFilesAreReleasedAsync();
      }

      return;

      static int ProcessIdOfTheGatewayOf(DirectoryInfo runtimeFolder) => int.Parse(runtimeFolder.Name[RuntimeFolderPrefix.Length..]);
   }
}
