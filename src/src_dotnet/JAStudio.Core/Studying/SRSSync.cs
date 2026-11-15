using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Threading;
using JAStudio.Core.Importing;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Studying.StudyNotes;
using JAStudio.Core.SysUtils.Threading;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.Studying;

/// <summary>
/// JAStudio's sync with the SRS, the spaced repetition system, which a person starts when they want what the two hold brought together. A sync:<br/>
/// • reads the study-notes from the spaced repetition system, and gives each note what it holds of its study-note: whether the note has one, the<br/>
///   studying statuses of its cards, and its tags;<br/>
/// • writes the study-notes that differ from what JAStudio would write for their notes now (<see cref="StudyNoteComparison"/>);<br/>
/// • runs the import profiles that run automatically (<see cref="AutomaticImportRuns"/>), which import in the background, after the sync.<br/>
/// It answers with what it did (<see cref="SRSSyncReport"/>).
/// </summary>
/// <remarks>
/// It's to replace Anki telling JAStudio of each change as it's made (Magnus, 2026-11-14: "we don't need instant notifications on changes from<br/>
/// anki. A sync button in a Studying view would be perfectly sufficient I think."): see dev_docs/moving-jastudio-out-of-anki.md.
/// </remarks>
/// <remarks>It isn't Anki's own sync, which syncs Anki's collection with AnkiWeb, and during which this sync can't run.</remarks>
/// <remarks>review-naming:studying: SRSSync and SRSSyncReport are new (Claude, 2026-11-15).</remarks>
public sealed class SRSSync
{
   readonly JPCollection _collection;
   readonly ISRSStudyNotes _studyNotes;
   readonly SRSAvailability _srsAvailability;
   readonly AutomaticImportRuns _automaticImportRuns;
   readonly TaskRunner _taskRunner;
   readonly OneAtATime _oneSyncAtATime = new();
   readonly IMonitor _monitor = IMonitor.New();
   readonly SimpleObservable<SRSSyncReport> _completed = new();
   SRSSyncReport? _lastReport;

   internal SRSSync(JPCollection collection, ISRSStudyNotes studyNotes, SRSAvailability srsAvailability, AutomaticImportRuns automaticImportRuns, TaskRunner taskRunner)
   {
      _collection = collection;
      _studyNotes = studyNotes;
      _srsAvailability = srsAvailability;
      _automaticImportRuns = automaticImportRuns;
      _taskRunner = taskRunner;
   }

   /// <summary>
   /// Why a sync can't run right now, in words for a person, or null when it can. It can't while what the spaced repetition system holds can't<br/>
   /// be read or changed (<see cref="SRSAvailability.WhyItCannotBeUsedNow"/>), or before the collection has loaded.
   /// </summary>
   public string? WhyItCannotRunNow => _srsAvailability.WhyItCannotBeUsedNow ?? (_collection.IsInitialized ? null : "JAStudio hasn't loaded the collection yet.");

   /// <summary>What the last sync did, or null when none has run since JAStudio started.</summary>
   public SRSSyncReport? LastReport => _monitor.Locked(() => _lastReport);

   /// <summary>Notifies what each sync did, once it's done, on the thread that ran it, whoever started it.</summary>
   public IObservable<SRSSyncReport> Completed => _completed;

   /// <summary>
   /// Runs a sync, once the sync under way, if any, has finished, showing its progress. It blocks until the study-notes are read and written, so<br/>
   /// it's for the background. It may only be asked for while a sync can run (<see cref="WhyItCannotRunNow"/>).
   /// </summary>
   public SRSSyncReport Run() => _oneSyncAtATime.Do(() =>
   {
      if(WhyItCannotRunNow is {} why) throw new InvalidOperationException($"A sync with the spaced repetition system can't run now: {why}");

      using var runner = _taskRunner.Current("Syncing with Anki");
      var (read, comparison) = runner.RunIndeterminate("Reading the study-notes from Anki", _collection.ReadTheStudyNotesFromTheSRSAgainAndCompareThem);

      List<JPNote> notesWhoseStudyNoteDiffers = [.. comparison.NotesWhoseStudyNoteDiffers.Select(it => it.Note)];
      runner.RunBatch(notesWhoseStudyNoteDiffers, _studyNotes.Write, "Writing the study-notes that differ");

      _automaticImportRuns.AfterASyncWithTheSRS();

      var report = new SRSSyncReport(DateTime.Now, read, comparison, notesWhoseStudyNoteDiffers);
      _monitor.Locked(() => _lastReport = report);
      _completed.OnNext(report);
      return report;
   });
}
