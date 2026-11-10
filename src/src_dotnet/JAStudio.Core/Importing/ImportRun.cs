using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Importing.Checking;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;
using Mp3LoudnessNormalization;

namespace JAStudio.Core.Importing;

/// <summary>
/// One application of an <see cref="ImportProfile"/>, prepared by <see cref="Importer.Prepare"/>: every <see cref="RawNote"/> it selected in Anki,<br/>
/// each as a <see cref="CheckedRawNote"/> with its <see cref="RawNoteOutcome"/>, and the <see cref="ProblemsWithTheProfile"/>, all found before<br/>
/// <see cref="ExecuteUnlessOutOfDate"/> changes anything.
/// </summary>
/// <remarks>
/// Executing it turns each raw note that <see cref="RawNoteOutcome.WillBeImported"/> into a corpus sentence and stores its media in the media layer, the audio<br/>
/// normalized to ReplayGain's reference loudness as mp3gain normalizes it, with the same normalized file replacing Anki's copy. A file the check found<br/>
/// in the media layer, the same and used by no note, is reused. Then it deletes the raw notes it imported, or tags them <see cref="RawNote.ImportedTag"/>.<br/>
/// The others are left as they are.
/// </remarks>
public class ImportRun
{
   readonly ChecksAndExecutionsOfImportRuns _checksAndExecutions;
   readonly int _numberOfExecutedRunsAtItsCheck;
   readonly IAnkiRawNotes _ankiRawNotes;
   readonly NoteServices _noteServices;
   readonly MediaFileIndex _mediaFileIndex;
   readonly MediaStorageService _mediaStorage;
   readonly string _ankiMediaFolder;
   readonly TaskRunner _taskRunner;
   readonly List<SentenceNote> _importedSentences = [];

   /// <summary>
   /// Created by <see cref="Importer.Prepare"/> at the end of the run's check, so the <see cref="ChecksAndExecutionsOfImportRuns.NumberOfExecutedRuns"/> it notes<br/>
   /// is the one at its check.
   /// </summary>
   internal ImportRun(ImportProfile profile,
                      IReadOnlyList<ImportProfileProblem> problemsWithTheProfile,
                      IReadOnlyList<CheckedRawNote> checkedRawNotes,
                      ChecksAndExecutionsOfImportRuns checksAndExecutions,
                      IAnkiRawNotes ankiRawNotes,
                      NoteServices noteServices,
                      MediaFileIndex mediaFileIndex,
                      MediaStorageService mediaStorage,
                      string ankiMediaFolder,
                      TaskRunner taskRunner)
   {
      Profile = profile;
      ProblemsWithTheProfile = problemsWithTheProfile;
      CheckedRawNotes = checkedRawNotes;
      _checksAndExecutions = checksAndExecutions;
      _numberOfExecutedRunsAtItsCheck = checksAndExecutions.NumberOfExecutedRuns;
      _ankiRawNotes = ankiRawNotes;
      _noteServices = noteServices;
      _mediaFileIndex = mediaFileIndex;
      _mediaStorage = mediaStorage;
      _ankiMediaFolder = ankiMediaFolder;
      _taskRunner = taskRunner;
   }

   public ImportProfile Profile { get; }

   /// <summary>The problems with the profile itself. While there are any, no raw notes are selected.</summary>
   public IReadOnlyList<ImportProfileProblem> ProblemsWithTheProfile { get; }

   /// <summary>Every raw note the profile selected: the notes of its note type that its search finds, those an earlier run imported included.</summary>
   public IReadOnlyList<CheckedRawNote> CheckedRawNotes { get; }

   /// <summary>The sentences <see cref="ExecuteUnlessOutOfDate"/> imported, in the order of their raw notes. Empty before it's executed.</summary>
   public IReadOnlyList<SentenceNote> ImportedSentences => _importedSentences;

   public IReadOnlyList<CheckedRawNote> With(RawNoteOutcome outcome) => [.. CheckedRawNotes.Where(it => it.Outcome == outcome)];

   /// <summary>Whether nothing stops this run. Any one problem, with the profile or with a raw note, stops all of it: they're fixed, and the profile is run again.</summary>
   public bool CanExecute => ProblemsWithTheProfile.Count == 0 && With(RawNoteOutcome.Problem).Count == 0;

   /// <summary>Whether executing does anything: nothing stops it, and some raw note <see cref="RawNoteOutcome.WillBeImported"/>.</summary>
   public bool HasNotesToImportAndNoProblems => CanExecute && With(RawNoteOutcome.WillBeImported).Count > 0;

   /// <summary>
   /// Whether another run was executed after this one's check, this one included once it's executed. The other run may have imported what the check found<br/>
   /// missing from JAStudio, so executing this one could import the same raw notes twice: it's checked again instead.
   /// </summary>
   public bool IsOutOfDate => _checksAndExecutions.NumberOfExecutedRuns != _numberOfExecutedRunsAtItsCheck;

   /// <summary>
   /// Imports the raw notes that <see cref="RawNoteOutcome.WillBeImported"/>, unless the run <see cref="IsOutOfDate"/>, once a check or an execution under way<br/>
   /// has finished. Says whether it executed the run: an out-of-date run is checked again instead.
   /// </summary>
   public bool ExecuteUnlessOutOfDate()
   {
      Contract.State.Assert(CanExecute, () => $"The import run of {Profile.Name} has problems.");
      return _checksAndExecutions.ExecuteUnlessOutOfDate(this, Execute);
   }

   void Execute()
   {
      var notesToImport = With(RawNoteOutcome.WillBeImported).Select(it => it.RawNote).ToList();
      using var scope = _taskRunner.Current($"Importing with the import profile {Profile.Name}");
      scope.RunBatch(notesToImport, Import, "Importing raw notes");
      FinishWith(notesToImport);
   }

   void Import(RawNote rawNote)
   {
      StoreTheAudioOf(rawNote);
      StoreTheScreenshotsOf(rawNote);
      AddTheSentenceOf(rawNote);
   }

   void StoreTheAudioOf(RawNote rawNote)
   {
      foreach(var audio in Profile.FieldMapping.AudioOf(rawNote).Where(IsNotInTheMediaLayer))
      {
         var ankisCopy = InAnkisMediaFolder(audio);
         var normalized = new Mp3File(File.ReadAllBytes(ankisCopy)).NormalizedToTheReplayGainReference().Bytes();
         _mediaStorage.StoreNewAudioFile(normalized, Profile.AudioFolder, audio.FileName);
         File.WriteAllBytes(ankisCopy, normalized);
      }
   }

   void StoreTheScreenshotsOf(RawNote rawNote)
   {
      foreach(var screenshot in Profile.FieldMapping.ScreenshotsOf(rawNote).Where(IsNotInTheMediaLayer))
         _mediaStorage.StoreFile(InAnkisMediaFolder(screenshot), Profile.ScreenshotFolder, screenshot.FileName, MediaType.Image);
   }

   void AddTheSentenceOf(RawNote rawNote)
   {
      var sentence = Profile.FieldMapping.SentenceFrom(rawNote, Profile.TagsToAdd, _noteServices);
      sentence.UpdateGeneratedData();

      if(Profile.CreatesAnkiNotesToStudy)
         _noteServices.Collection.Sentences.Add(sentence);
      else
         _noteServices.Collection.Sentences.AddWithoutAnAnkiNote(sentence);

      _importedSentences.Add(sentence);
   }

   void FinishWith(IReadOnlyList<RawNote> importedRawNotes)
   {
      if(Profile.DeletesTheRawNotesAfterwards)
         _ankiRawNotes.Delete(importedRawNotes);
      else
         _ankiRawNotes.TagAsImported(importedRawNotes);
   }

   /// <summary>A file of a note to import that's in the media layer already is one no note uses, and the same file: the check made it a problem otherwise. It's reused.</summary>
   bool IsNotInTheMediaLayer(MediaReference file) => !_mediaFileIndex.ContainsByOriginalFileName(file.FileName);

   string InAnkisMediaFolder(MediaReference file) => Path.Combine(_ankiMediaFolder, file.FileName);
}
