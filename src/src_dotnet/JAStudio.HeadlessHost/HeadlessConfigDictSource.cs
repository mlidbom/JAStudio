using System;
using System.Collections.Generic;
using JAStudio.Core.Configuration;

namespace JAStudio.HeadlessHost;

/// <summary>
/// The headless JAStudio's configuration: the same at every start, in every source checkout. It's JAStudio's defaults, except that the<br/>
/// batches run on one thread per logical core of the machine (<see cref="JapaneseConfig.ReanalysisThreads"/>). A setting changed while it<br/>
/// runs, such as a sentence breakdown's setting toggled with its hotkey, is kept in memory only, until it stops.
/// </summary>
/// <remarks>
/// The headless JAStudio is for developing JAStudio, whose results must not depend on when it was started. Magnus's configuration, kept in<br/>
/// the add-on folder (<see cref="ConfigurationFile"/>), changes from second to second as he studies: he toggles the sentence breakdown's<br/>
/// settings to see more or less of the sentence he's on. Read at the start, it made two runs show the same sentence's breakdown two ways,<br/>
/// and a worktree's stale copy of it made the batches run on 4 threads instead of 16 (2026-11-12). What Magnus sees right now is the live<br/>
/// JAStudio's to show.
/// </remarks>
class HeadlessConfigDictSource : IConfigDictSource
{
   public Dictionary<string, object> Load() => new() { [JapaneseConfig.ReanalysisThreadsName] = (long)Environment.ProcessorCount };

   /// <summary>Does nothing: the settings changed are already in the configuration in memory, and the headless JAStudio keeps them nowhere else.</summary>
   public void Persist(IReadOnlyDictionary<string, object> configuration) {}
}
