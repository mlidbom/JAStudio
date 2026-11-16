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
/// It's the only way what changed in the spaced repetition system reaches JAStudio, and the only time JAStudio writes the study-notes. Until<br/>
/// 2026-11-15 Anki told JAStudio of each change as it was made, and JAStudio wrote a note's study-note each time it updated the note (Magnus,<br/>
/// 2026-11-14: "we don't need instant notifications on changes from anki. A sync button in a Studying view would be perfectly sufficient I<br/>
/// think."): see dev_docs/moving-jastudio-out-of-anki.md.
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
   readonly SimpleObservable<bool> _isRunningChanged = new();
   SRSSyncReport? _lastReport;
   bool _isRunning;

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

   /// <summary>
   /// Whether a sync can't run right now because the spaced repetition system has another collection open than the one JAStudio works with, such<br/>
   /// as another Anki profile's: JAStudio can then work with that one instead (<see cref="WorkWithTheCollectionTheSRSHasOpen"/>).
   /// </summary>
   public bool TheSRSHasAnotherCollectionOpenThanJAStudioWorksWith => _srsAvailability.HasAnotherCollectionOpenThanJAStudioWorksWith;

   /// <summary>
   /// Has JAStudio work with the collection the spaced repetition system has open, in place of the one it worked with, and reads that<br/>
   /// collection's study-notes, so that each note shows what that collection holds of its study-note. It writes nothing: the next sync does.<br/>
   /// It blocks until the study-notes are read, so it's for the background.
   /// </summary>
   /// <remarks>
   /// Magnus, 2026-11-15, of JAStudio finding that Anki has another collection open: "you should be able to accept using this one instead with a<br/>
   /// single click in jastudio to reload the anki state".
   /// </remarks>
   public void WorkWithTheCollectionTheSRSHasOpen() => _oneSyncAtATime.Do(() =>
   {
      _srsAvailability.WorkWithTheCollectionItHasOpen();
      _collection.ReadTheStudyNotesFromTheSRSAgain();
   });

   /// <summary>What the last sync did, or null when none has run since JAStudio started.</summary>
   public SRSSyncReport? LastReport => _monitor.Locked(() => _lastReport);

   /// <summary>Notifies what each sync did, once it's done, on the thread that ran it, whoever started it.</summary>
   public IObservable<SRSSyncReport> Completed => _completed;

   /// <summary>Whether a sync is under way, whoever started it.</summary>
   public bool IsRunning => _monitor.Locked(() => _isRunning);

   /// <summary>
   /// Notifies <see cref="IsRunning"/> each time it changes, on the thread that runs the sync: as a sync starts, and once it's over, whether it<br/>
   /// finished, after <see cref="Completed"/> notified what it did, or failed.
   /// </summary>
   public IObservable<bool> IsRunningChanged => _isRunningChanged;

   /// <summary>
   /// Runs a sync, once the sync under way, if any, has finished. It blocks until the study-notes are read and written, so it's for the<br/>
   /// background. It may only be asked for while a sync can run (<see cref="WhyItCannotRunNow"/>).
   /// </summary>
   /// <remarks>
   /// It opens no progress window, however many study-notes it writes: whoever started the sync shows that it's under way, as the Sync button<br/>
   /// does (Magnus, 2026-11-15: "Click it and it spins and done", and of a progress window for a long write: "I probably still don't want it<br/>
   /// popping up automatically. Just spinning is fine."). JAStudio writes only a study-note's question and audio, which seldom change.
   /// </remarks>
   public SRSSyncReport Run() => _oneSyncAtATime.Do(() =>
   {
      if(WhyItCannotRunNow is {} why) throw new InvalidOperationException($"A sync with the spaced repetition system can't run now: {why}");

      SetWhetherItIsRunning(true);
      try
      {
         using var runner = _taskRunner.Current("Syncing with Anki", forceHide: true);
         var (read, comparison) = runner.RunIndeterminate("Reading the study-notes from Anki", _collection.ReadTheStudyNotesFromTheSRSAgainAndCompareThem);

         List<JPNote> notesWhoseStudyNoteDiffers = [.. comparison.NotesWhoseStudyNoteDiffers.Select(it => it.Note)];
         runner.RunBatch(notesWhoseStudyNoteDiffers, _studyNotes.Write, "Writing the study-notes that differ");

         _automaticImportRuns.AfterASyncWithTheSRS();

         var report = new SRSSyncReport(DateTime.Now, read, comparison, notesWhoseStudyNoteDiffers);
         _monitor.Locked(() => _lastReport = report);
         _completed.OnNext(report);
         return report;
      }
      finally
      {
         SetWhetherItIsRunning(false);
      }
   });

   void SetWhetherItIsRunning(bool isRunning)
   {
      _monitor.Locked(() => _isRunning = isRunning);
      _isRunningChanged.OnNext(isRunning);
   }
}
