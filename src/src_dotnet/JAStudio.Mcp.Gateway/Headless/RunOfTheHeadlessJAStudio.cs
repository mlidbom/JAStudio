using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JAStudio.Mcp.Discovery;

namespace JAStudio.Mcp.Gateway.Headless;

/// <summary>A run of the headless JAStudio while it runs: what it was started with, its process, and how it announced itself among the running JAStudios.</summary>
sealed class RunOfTheHeadlessJAStudio : IAsyncDisposable
{
   RunOfTheHeadlessJAStudio(HeadlessJAStudioSetup setup, HeadlessJAStudioProcess process, RunningJAStudio jastudio)
   {
      Setup = setup;
      Process = process;
      JAStudio = jastudio;
   }

   public HeadlessJAStudioSetup Setup { get; }
   public HeadlessJAStudioProcess Process { get; }

   /// <summary>The headless JAStudio as it announced itself among the <see cref="RunningJAStudios"/>, once its MCP server listened.</summary>
   public RunningJAStudio JAStudio { get; }

   /// <summary>
   /// Starts the headless JAStudio from <paramref name="runtimeFolder"/>, the copy of its build output, and waits until its MCP server listens, which it<br/>
   /// announces among <paramref name="runningJAStudios"/> first.
   /// </summary>
   public static async Task<RunOfTheHeadlessJAStudio> StartAsync(DirectoryInfo runtimeFolder, HeadlessJAStudioSetup setup, RunningJAStudios runningJAStudios, CancellationToken cancellationToken)
   {
      var process = await HeadlessJAStudioProcess.StartAsync(runtimeFolder, setup, cancellationToken);
      return new RunOfTheHeadlessJAStudio(setup, process, runningJAStudios.All.Single(it => it.ProcessId == process.Id));
   }

   /// <summary>What <see cref="HeadlessJAStudioTools.StartHeadlessJAStudio"/> answers once the run has started: what it runs, and on what.</summary>
   public string ReportOfItsStart(SourceCheckout builtFrom, TimeSpan startedIn) =>
      $"""
       The headless JAStudio runs, built from {builtFrom.Folder.FullName}, started in {startedIn.TotalSeconds:F1} seconds.
       Corpus checkout: {Setup.CorpusFolder.FullName}
       Anki collection: {Setup.AnkiCollectionFile.FullName}
       Log: {Process.Log.File.FullName}
       """;

   /// <summary>Ends the run: stops the headless JAStudio if it's still running.</summary>
   public async ValueTask DisposeAsync() => await Process.DisposeAsync();
}
