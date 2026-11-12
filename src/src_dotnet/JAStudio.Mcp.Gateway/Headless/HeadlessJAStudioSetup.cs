using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JAStudio.Mcp.Gateway.Headless;

/// <summary>What a run of the headless JAStudio works with: the add-on folder of the source checkout, a corpus checkout, and an Anki profile's collection.</summary>
/// <param name="AddonFolder">The source checkout's <c>src</c> folder, with JAStudio's configuration and the user files.</param>
/// <param name="CorpusFolder">The corpus checkout the notes are loaded from and saved to.</param>
/// <param name="AnkiCollectionFile">The Anki profile's <c>collection.anki2</c>, which the notes' studying statuses are read from.</param>
record HeadlessJAStudioSetup(DirectoryInfo AddonFolder, DirectoryInfo CorpusFolder, FileInfo AnkiCollectionFile)
{
   /// <summary>
   /// The setup Claude chose when starting the headless JAStudio from <paramref name="sourceCheckout"/>: on the corpus checkout <paramref name="corpusFolder"/>,<br/>
   /// or with none named on <see cref="SourceCheckout.DefaultHeadlessCorpusFolderAsync"/>, and with the collection of the Anki profile<br/>
   /// <paramref name="ankiProfile"/>, or with none named of the profile used last.
   /// </summary>
   public static async Task<HeadlessJAStudioSetup> ChosenAsync(SourceCheckout sourceCheckout, string? corpusFolder, string? ankiProfile, CancellationToken cancellationToken) =>
      //review: On a worktree, the configuration and the user files come from the worktree's src folder, where meta.json and user_files are
      //   untracked copies that go stale: one from May 29 made the headless JAStudio reparse on 4 threads while Anki's configuration says 16
      //   (2026-11-12). Should they come from the add-on folder Anki loads (addons21/jastudio_module, Magnus's main checkout) instead?
      new(sourceCheckout.AddonFolder,
          corpusFolder == null ? await sourceCheckout.DefaultHeadlessCorpusFolderAsync(cancellationToken) : new DirectoryInfo(corpusFolder),
          AnkiProfiles.CollectionFileOf(ankiProfile));

   /// <summary>
   /// The command line arguments the headless JAStudio takes this setup as: see <c>JAStudio.HeadlessHost.HeadlessHostArguments</c>. Its MCP server<br/>
   /// listens on a port the system picks, and it exits with the gateway.
   /// </summary>
   public IReadOnlyList<string> HeadlessHostArguments() =>
   [
      "--addon-folder", AddonFolder.FullName,
      "--corpus-folder", CorpusFolder.FullName,
      "--anki-collection-file", AnkiCollectionFile.FullName,
      "--mcp-server-port", "0",
      "--exit-with-process", Environment.ProcessId.ToString(CultureInfo.InvariantCulture)
   ];
}
