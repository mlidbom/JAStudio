using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JAStudio.Mcp.Discovery;
using JAStudio.Mcp.Gateway.Connections;

namespace JAStudio.Mcp.Gateway.Headless;

/// <summary>
/// The headless JAStudio this gateway runs, when it runs one. The gateway's tools (<see cref="HeadlessJAStudioTools"/>) start, restart and stop it,<br/>
/// and while it runs, the gateway is connected to it (<see cref="ConnectedJAStudios"/>), offering its MCP server's tools to Claude Code as its own.<br/>
/// Starting always builds it from the source checkout first, so it runs the checkout's code as it is.
/// </summary>
sealed class HeadlessJAStudio : IAsyncDisposable
{
   readonly SourceCheckout _sourceCheckout;
   readonly RunningJAStudios _runningJAStudios;
   readonly ConnectedJAStudios _connectedJAStudios;
   readonly OneAtATime _startsAndStops = new();
   RunOfTheHeadlessJAStudio? _run;

   public HeadlessJAStudio(SourceCheckout sourceCheckout, RunningJAStudios runningJAStudios, ConnectedJAStudios connectedJAStudios)
   {
      _sourceCheckout = sourceCheckout;
      _runningJAStudios = runningJAStudios;
      _connectedJAStudios = connectedJAStudios;
   }

   /// <summary>
   /// Builds the headless JAStudio and starts it on <paramref name="corpusFolder"/>, stopping the one running first, and connects to it. Returns a<br/>
   /// report of what runs, and of the connection.
   /// </summary>
   /// <param name="corpusFolder">The corpus checkout to work on, or none for <see cref="SourceCheckout.DefaultHeadlessCorpusFolderAsync"/>.</param>
   /// <param name="ankiProfile">The Anki profile to read studying statuses from, or none for the one used last.</param>
   /// <param name="cancellationToken">Cancels the build and the wait for the headless JAStudio to start.</param>
   public Task<string> StartAsync(string? corpusFolder, string? ankiProfile, CancellationToken cancellationToken) => _startsAndStops.RunAsync(async () =>
   {
      var stopwatch = Stopwatch.StartNew();
      await EndTheRunAsync();
      await HeadlessJAStudioFolders.DeleteTheRuntimeFoldersOfGatewaysThatExitedAsync();

      var setup = await HeadlessJAStudioSetup.ChosenAsync(_sourceCheckout, corpusFolder, ankiProfile, cancellationToken);
      var buildOutput = await HeadlessJAStudioBuild.BuildAsync(_sourceCheckout, cancellationToken);
      var runtimeFolder = await HeadlessJAStudioFolders.RuntimeFolderWithACopyOfAsync(buildOutput);
      var run = await RunOfTheHeadlessJAStudio.StartAsync(runtimeFolder, setup, _runningJAStudios, cancellationToken);
      _run = run;
      EndTheRunWhenItExitsByItself(run);
      var reportOfTheConnection = await _connectedJAStudios.ConnectAsync(run.JAStudio, cancellationToken);

      return $"{run.ReportOfItsStart(_sourceCheckout, stopwatch.Elapsed)}{Environment.NewLine}{reportOfTheConnection}";
   });

   /// <summary>Stops the headless JAStudio, if it runs, and ends the connection to it, withdrawing its tools.</summary>
   public Task<string> StopAsync() => _startsAndStops.RunAsync(async () =>
   {
      if(_run == null) return "The headless JAStudio doesn't run.";

      var logFile = _run.Process.Log.File;
      await EndTheRunAsync();
      return $"The headless JAStudio is stopped. Its log: {logFile.FullName}";
   });

   /// <summary>
   /// Ends <paramref name="run"/> when its headless JAStudio exits without the gateway stopping it, such as when it crashed, and the connection to it:<br/>
   /// nothing starts it again but Claude. When the gateway ended the run itself, it's no longer <see cref="_run"/>, and there's nothing left to end.
   /// </summary>
   void EndTheRunWhenItExitsByItself(RunOfTheHeadlessJAStudio run)
   {
      _ = run.Process.Exited.ContinueWith(_ => _startsAndStops.RunAsync(EndItUnlessTheGatewayEndedItAlreadyAsync), TaskScheduler.Default);
      return;

      async Task EndItUnlessTheGatewayEndedItAlreadyAsync()
      {
         if(_run == run) await EndTheRunAsync();
      }
   }

   async Task EndTheRunAsync()
   {
      if(_run == null) return;

      await _connectedJAStudios.DisconnectFromAsync(_run.JAStudio.Identity);
      await _run.DisposeAsync();
      _run = null;
   }

   public async ValueTask DisposeAsync()
   {
      await _startsAndStops.RunAsync(EndTheRunAsync);
      await HeadlessJAStudioFolders.DeleteTheRuntimeFolderAsync();
   }
}
