using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol;

namespace JAStudio.Mcp.Gateway.Headless;

/// <summary>
/// The process of a run of the headless JAStudio, started from a copy of its build output, with everything it writes kept in its <see cref="Log"/>,<br/>
/// and the address its MCP server listens at once it has loaded the collection.
/// </summary>
sealed class HeadlessJAStudioProcess : IAsyncDisposable
{
   /// <summary>What the headless JAStudio writes, followed by its MCP server's address, once its MCP server listens. See <c>JAStudio.HeadlessHost.Program</c>.</summary>
   const string McpServerListensAt = "JAStudio's MCP server listens at ";

   /// <summary>How long the headless JAStudio may take to load the collection and start its MCP server: a first load, which builds the corpus's snapshots, takes about 20 seconds.</summary>
   static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(3);

   readonly Process _process;
   readonly TaskCompletionSource<Uri> _mcpServerUrl = new(TaskCreationOptions.RunContinuationsAsynchronously);

   HeadlessJAStudioProcess(Process process, HeadlessJAStudioLog log)
   {
      _process = process;
      Exited = process.WaitForExitAsync();
      Log = log;
   }

   /// <summary>The id of the headless JAStudio's process.</summary>
   public int Id => _process.Id;

   /// <summary>Everything the headless JAStudio wrote, to standard output and standard error.</summary>
   public HeadlessJAStudioLog Log { get; }

   /// <summary>Completes when the headless JAStudio exits, whether stopped or not.</summary>
   public Task Exited { get; }

   /// <summary>Starts the headless JAStudio from <paramref name="runtimeFolder"/>, and waits until its MCP server listens, which is once it has loaded the collection.</summary>
   public static async Task<HeadlessJAStudioProcess> StartAsync(DirectoryInfo runtimeFolder, HeadlessJAStudioSetup setup, CancellationToken cancellationToken)
   {
      var process = ChildProcesses.Start(new CommandLine("dotnet", [Path.Combine(runtimeFolder.FullName, "JAStudio.HeadlessHost.dll"), .. setup.HeadlessHostArguments()]), runtimeFolder);
      var headlessJAStudio = new HeadlessJAStudioProcess(process, HeadlessJAStudioLog.New());
      headlessJAStudio.StartReadingWhatItWrites();
      await headlessJAStudio.WaitUntilTheMcpServerListensAsync(cancellationToken);
      return headlessJAStudio;
   }

   /// <summary>The address of the headless JAStudio's MCP server.</summary>
   public Uri McpServerUrl => _mcpServerUrl.Task.IsCompletedSuccessfully ? _mcpServerUrl.Task.Result : throw new InvalidOperationException("The MCP server doesn't listen yet.");

   void StartReadingWhatItWrites()
   {
      _process.OutputDataReceived += (_, written) => OnWritten(written);
      _process.ErrorDataReceived += (_, written) => OnWritten(written);
      _process.BeginOutputReadLine();
      _process.BeginErrorReadLine();
      return;

      void OnWritten(DataReceivedEventArgs written)
      {
         if(written.Data is not {} line) return; // The stream it wrote to closed.

         Log.Write(line);
         if(line.StartsWith(McpServerListensAt, StringComparison.Ordinal)) _mcpServerUrl.TrySetResult(new Uri(line[McpServerListensAt.Length..]));
      }
   }

   async Task WaitUntilTheMcpServerListensAsync(CancellationToken cancellationToken)
   {
      var theMcpServerListensOrTheProcessExited = Task.WhenAny(_mcpServerUrl.Task, Exited);
      await Task.WhenAny(theMcpServerListensOrTheProcessExited, Task.Delay(StartupTimeout, cancellationToken));
      if(_mcpServerUrl.Task.IsCompletedSuccessfully) return;

      var whyItFailed = Exited.IsCompleted
                           ? $"The headless JAStudio exited with code {_process.ExitCode} before its MCP server started."
                           : $"The headless JAStudio's MCP server didn't start within {StartupTimeout}.";
      var report = $"{whyItFailed} The end of its log, {Log.File.FullName}:{Environment.NewLine}{Log.LastLines()}";
      await DisposeAsync();
      cancellationToken.ThrowIfCancellationRequested();
      throw new McpException(report);
   }

   /// <summary>Stops the headless JAStudio, if it's still running, and waits until it has exited.</summary>
   public async ValueTask DisposeAsync()
   {
      if(!_process.HasExited) _process.Kill(entireProcessTree: true);
      await _process.WaitForExitAsync();
      _process.Dispose();
      await Log.DisposeAsync();
   }
}
