using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Internals.Logging;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core.SysUtils.Threading;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.Importing;

/// <summary>
/// Runs the <see cref="ImportProfile"/>s that <see cref="ImportProfile.RunsAutomatically"/> when Anki signals that raw notes may have arrived:<br/>
/// when Anki finished importing a file, such as Immersion Kit's .apkg sentence cards, and when a note was added, as AnkiConnect adds them.<br/>
/// A run that has problems isn't executed: <see cref="StoppedByProblems"/> notifies its profile, for its report to be shown.
/// </summary>
/// <remarks>
/// Anki's .apkg importer runs in Anki's Rust backend, which adds the notes without announcing each one, so a finished import is a signal of its own.<br/>
/// The runs happen in the background, one profile at a time. A profile that's signalled again while it waits runs once, so a burst of added notes<br/>
/// doesn't check it once per note. A profile signalled while it runs runs again afterwards, to import what arrived meanwhile.
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
   /// Anki finished importing a file, such as an .apkg, which may have brought raw notes of any note type. Runs every automatic profile whose note type<br/>
   /// the open Anki profile has: a profile whose note type it hasn't has no raw notes to import there.
   /// </summary>
   public void AfterAnkiImportedAFile() => _profilesWaitingToRun.Add(AutomaticProfiles());

   /// <summary>A note of the note type <paramref name="ankiNoteType"/> was added in Anki. Runs the automatic profiles that take notes of that type.</summary>
   public void AfterANoteWasAddedInAnki(string ankiNoteType) => _profilesWaitingToRun.Add(AutomaticProfiles().Where(it => it.AnkiNoteType == ankiNoteType));

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
