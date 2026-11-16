using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Internals.Logging;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core.SysUtils.Threading;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.Importing;

/// <summary>
/// Runs the <see cref="ImportProfile"/>s that <see cref="ImportProfile.RunsAutomatically"/> at each sync with the spaced repetition system<br/>
/// (<see cref="Studying.SRSSync"/>): raw notes may have arrived in Anki since the sync before, such as Immersion Kit's .apkg sentence cards, or<br/>
/// notes that AnkiConnect added.<br/>
/// A run that has problems isn't executed: <see cref="StoppedByProblems"/> notifies its profile, for its report to be shown.
/// </summary>
/// <remarks>
/// The runs happen in the background, one profile at a time. A profile that's asked for again while it waits runs once, and one asked for while it<br/>
/// runs runs again afterwards, to import what arrived meanwhile.
/// </remarks>
/// <remarks>
/// Until 2026-11-15 Anki signalled when it had imported a file and when a note was added, and the profiles ran then: see<br/>
/// dev_docs/moving-jastudio-out-of-anki.md.
/// </remarks>
public class AutomaticImportRuns
{
   readonly SavedImportProfiles _savedProfiles;
   readonly Importer _importer;
   readonly BackgroundQueueWithoutDuplicates<ImportProfile> _profilesWaitingToRun;
   readonly SimpleObservable<ImportProfile> _stoppedByProblems = new();
   readonly SimpleObservable<ImportRun> _executed = new();

   public AutomaticImportRuns(SavedImportProfiles savedProfiles, Importer importer, BackgroundTaskManager backgroundTaskManager)
   {
      _savedProfiles = savedProfiles;
      _importer = importer;
      _profilesWaitingToRun = new BackgroundQueueWithoutDuplicates<ImportProfile>(Run, backgroundTaskManager);
   }

   /// <summary>Notifies each profile whose automatic run wasn't executed because its check found problems, on the thread that checked it.</summary>
   public IObservable<ImportProfile> StoppedByProblems => _stoppedByProblems;

   /// <summary>Notifies each automatic run that imported raw notes, once it's executed, on the thread that executed it.</summary>
   public IObservable<ImportRun> Executed => _executed;

   /// <summary>
   /// A sync with the spaced repetition system was made (<see cref="Studying.SRSSync"/>), and raw notes of any note type may have arrived in Anki since<br/>
   /// the one before. Runs every automatic profile whose note type the open Anki profile has: a profile whose note type it hasn't has no raw notes<br/>
   /// to import there.
   /// </summary>
   public void AfterASyncWithTheSRS() => _profilesWaitingToRun.Add(AutomaticProfiles());

   IEnumerable<ImportProfile> AutomaticProfiles() => _savedProfiles.All.Where(it => it.RunsAutomatically);

   void Run(ImportProfile profile)
   {
      if(!StillRunsAutomaticallyAndTheOpenAnkiProfileHasItsNoteType()) return;

      var run = _importer.Prepare(profile);
      if(!run.CanExecute)
      {
         this.Log().Info($"The automatic run of the import profile {profile.Name} has problems, so it wasn't executed.");
         _stoppedByProblems.OnNext(profile);
      }
      else if(!run.HasNotesToImportAndNoProblems)
      {
         this.Log().Info($"The automatic run of the import profile {profile.Name} found nothing to import.");
      }
      else if(run.ExecuteUnlessOutOfDate())
      {
         this.Log().Info($"The automatic run of the import profile {profile.Name} imported {run.ImportedSentences.Count} sentences.");
         _executed.OnNext(run);
      }
      else
      {
         this.Log().Info($"Another import run was executed after the check of the automatic run of the import profile {profile.Name}, so it's checked again.");
         _profilesWaitingToRun.Add([profile]);
      }

      return;

      bool StillRunsAutomaticallyAndTheOpenAnkiProfileHasItsNoteType() =>
         _savedProfiles.Contains(profile) && profile.RunsAutomatically && _importer.NoteTypesForRawNotes().Any(it => it.Name == profile.AnkiNoteType);
   }
}
