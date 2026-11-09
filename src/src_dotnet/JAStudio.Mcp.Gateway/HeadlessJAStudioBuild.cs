using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JAStudio.Mcp.Gateway;

/// <summary>Builds the headless JAStudio from the source checkout, so that each start runs the checkout's code as it is.</summary>
static class HeadlessJAStudioBuild
{
   /// <summary>Builds the headless JAStudio's project and returns its build output folder. A build that fails throws, with what the build wrote.</summary>
   /// <remarks>
   /// It builds without the build servers, the compiler server and MSBuild's reused nodes: they outlive the build, and would keep the build's<br/>
   /// redirected output open, so that reading it never ended.
   /// </remarks>
   public static async Task<DirectoryInfo> BuildAsync(SourceCheckout sourceCheckout, CancellationToken cancellationToken)
   {
      await ChildProcesses.RunToCompletionAsync(
         new CommandLine("dotnet", ["build", sourceCheckout.HeadlessHostProject.FullName, "-c", "Debug", "-v", "quiet", "-nologo", "--disable-build-servers", "-nodeReuse:false"]),
         sourceCheckout.Folder,
         cancellationToken);

      return BuildOutputFolder(sourceCheckout);
   }

   /// <summary>
   /// The folder the build wrote <c>JAStudio.HeadlessHost.dll</c> to last, under the project's <c>bin\Debug</c>: the project builds for this<br/>
   /// computer's runtime, which puts the build output in a folder named for it.
   /// </summary>
   static DirectoryInfo BuildOutputFolder(SourceCheckout sourceCheckout) =>
      new DirectoryInfo(Path.Combine(sourceCheckout.HeadlessHostProjectFolder.FullName, "bin", "Debug"))
        .EnumerateFiles("JAStudio.HeadlessHost.dll", SearchOption.AllDirectories)
        .MaxBy(it => it.LastWriteTimeUtc)?.Directory
   ?? throw new InvalidOperationException($"The build of {sourceCheckout.HeadlessHostProject.FullName} wrote no JAStudio.HeadlessHost.dll");
}
