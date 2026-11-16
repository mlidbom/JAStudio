using System;
using System.ComponentModel;
using System.Diagnostics;
using JAStudio.Core.Studying;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Studying;

/// <summary>The tool that runs JAStudio's sync with Anki (<see cref="SRSSync"/>), as a person starts one from the Studying window.</summary>
[McpServerToolType]
public class SyncTools
{
   /// <summary>How many notes of each type the tool lists unless asked for another number.</summary>
   const int NotesListedPerTypeByDefault = 10;

   readonly SRSSync _sync;

   public SyncTools(SRSSync sync) => _sync = sync;

   [McpServerTool(Name = "sync_with_anki", Destructive = false, Idempotent = true)]
   [Description("""
                Runs JAStudio's sync with Anki, as a person starts one: reads every study-note from Anki, the Anki notes holding the notes' cards,
                and gives each note what Anki holds of its study-note, which is whether it has one, the studying statuses of its cards and its
                Anki tags; writes the study-notes whose fields differ from what JAStudio would write now, as write_the_study_notes_that_differ
                does; and starts the import profiles that run automatically, which import in the background and aren't in the report. Only in
                a live JAStudio, inside Anki or the standalone one, and only while Anki can be used: it refuses, saying why, while Anki isn't
                running or has no profile open, while Anki's own sync runs, and, in the standalone JAStudio, while Anki has another collection
                open than the one it works with. The standalone JAStudio's sync writes the study-notes from the corpus checkout it works on, and
                imports into it. The headless JAStudio has no Anki and refuses. Reports how long it took, how many study-notes it read, the notes whose study-note
                had changed in Anki since JAStudio last read it, the notes whose study-note is gone from Anki, the notes whose study-notes it
                wrote, each by type, and the study-notes that belong to no note: those that hold no corpus note's id, which is corruption, and
                those whose note the collection hasn't, such as a note deleted with its study-note kept.
                """)]
   public WhatTheSyncDid SyncWithAnki(
      [Description("The most notes of each type to list. Every note is counted either way.")] int maxNotesListedPerType = NotesListedPerTypeByDefault)
   {
      if(_sync.WhyItCannotRunNow is {} why) throw new InvalidOperationException($"The sync with Anki can't run now: {why}");

      var stopwatch = Stopwatch.StartNew();
      var report = _sync.Run();
      return WhatTheSyncDid.Of(stopwatch.Elapsed.TotalSeconds, report, maxNotesListedPerType);
   }
}
