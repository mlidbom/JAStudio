using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol;

namespace JAStudio.Mcp.Gateway;

/// <summary>
/// Starts the processes the gateway runs: the commands it runs to completion, such as <c>dotnet build</c> and <c>git</c>, and the headless JAStudio.<br/>
/// Each has every standard stream redirected and its standard input closed. A child must never inherit the gateway's own standard streams: they<br/>
/// are the stdio connection to Claude Code, which a child reading or writing them would corrupt.
/// </summary>
static class ChildProcesses
{
   /// <summary>Starts <paramref name="commandLine"/> in <paramref name="workingFolder"/>, and leaves reading what it writes to the caller.</summary>
   public static Process Start(CommandLine commandLine, DirectoryInfo workingFolder)
   {
      var startInfo = new ProcessStartInfo(commandLine.Command)
      {
         WorkingDirectory = workingFolder.FullName,
         UseShellExecute = false,
         RedirectStandardInput = true,
         RedirectStandardOutput = true,
         RedirectStandardError = true,
         StandardOutputEncoding = Encoding.UTF8,
         StandardErrorEncoding = Encoding.UTF8
      };
      foreach(var argument in commandLine.Arguments) startInfo.ArgumentList.Add(argument);

      var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"`{commandLine}` started no process.");
      process.StandardInput.Close();
      return process;
   }

   /// <summary>Runs <paramref name="commandLine"/> in <paramref name="workingFolder"/> to completion and returns what it wrote, or throws with that output when it exits with an error code.</summary>
   public static async Task<string> RunToCompletionAsync(CommandLine commandLine, DirectoryInfo workingFolder, CancellationToken cancellationToken)
   {
      using var process = Start(commandLine, workingFolder);
      var standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
      var standardError = process.StandardError.ReadToEndAsync(cancellationToken);
      await process.WaitForExitAsync(cancellationToken);
      var output = await standardOutput + await standardError;

      return process.ExitCode == 0
                ? output
                : throw new McpException($"`{commandLine}` failed with exit code {process.ExitCode}:{Environment.NewLine}{output}");
   }
}
