using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;
using Mp3LoudnessNormalization;

namespace JAStudio.Core.Importing;

/// <summary>
/// One application of an <see cref="ImportProfile"/>, prepared by <see cref="Importer.Prepare"/>: the <see cref="RawNote"/>s it selected in Anki,<br/>
/// the <see cref="Problems"/> that stop it and the <see cref="AlreadyImported"/> notes it skips, all found before <see cref="Execute"/> changes anything.
/// </summary>
/// <remarks>
/// Executing it turns each raw note into a corpus sentence and stores the sentence's media in the media layer, the audio normalized to ReplayGain's<br/>
/// reference loudness as mp3gain normalizes it, with the same normalized file replacing Anki's copy. Then it deletes every raw note it selected,<br/>
/// or tags it <see cref="RawNote.ImportedTag"/>, the skipped ones too, so that later runs leave them out.
/// </remarks>
public class ImportRun
{
   readonly IAnkiRawNotes _ankiRawNotes;
   readonly NoteServices _noteServices;
   readonly MediaStorageService _mediaStorage;
   readonly string _ankiMediaFolder;
   readonly TaskRunner _taskRunner;

   internal ImportRun(ImportProfile profile,
                      IReadOnlyList<RawNote> rawNotes,
                      IReadOnlyList<ImportProblem> problems,
                      IReadOnlyList<AlreadyImportedNote> alreadyImported,
                      IAnkiRawNotes ankiRawNotes,
                      NoteServices noteServices,
                      MediaStorageService mediaStorage,
                      string ankiMediaFolder,
                      TaskRunner taskRunner)
   {
      Profile = profile;
      RawNotes = rawNotes;
      Problems = problems;
      AlreadyImported = alreadyImported;
      _ankiRawNotes = ankiRawNotes;
      _noteServices = noteServices;
      _mediaStorage = mediaStorage;
      _ankiMediaFolder = ankiMediaFolder;
      _taskRunner = taskRunner;
   }

   public ImportProfile Profile { get; }

   /// <summary>The raw notes the profile selected: the notes of its note type that its search finds and that aren't tagged <see cref="RawNote.ImportedTag"/>.</summary>
   public IReadOnlyList<RawNote> RawNotes { get; }

   /// <summary>What stops this run. Any one problem stops all of it: the raw notes are fixed in Anki, and the profile is run again.</summary>
   public IReadOnlyList<ImportProblem> Problems { get; }

   /// <summary>The raw notes that look imported already, which this run skips.</summary>
   public IReadOnlyList<AlreadyImportedNote> AlreadyImported { get; }

   public IReadOnlyList<RawNote> NotesToImport => [.. RawNotes.Except(AlreadyImported.Select(it => it.RawNote))];

   public bool CanExecute => Problems.Count == 0;

   /// <summary>Whether executing does anything: it found raw notes, and no problems. A run whose raw notes are all imported already still tags or deletes them.</summary>
   public bool HasRawNotesAndNoProblems => CanExecute && RawNotes.Count > 0;

   public void Execute()
   {
      Contract.State.Assert(CanExecute, () => $"The import run of {Profile.Name} has problems: {string.Join(" ", Problems)}");

      using var scope = _taskRunner.Current($"Importing with the import profile {Profile.Name}");
      scope.RunBatch([.. NotesToImport], Import, "Importing raw notes");
      FinishWithTheRawNotes();
   }

   void Import(RawNote rawNote)
   {
      StoreTheAudioOf(rawNote);
      StoreTheScreenshotsOf(rawNote);
      AddTheSentenceOf(rawNote);
   }

   void StoreTheAudioOf(RawNote rawNote)
   {
      foreach(var audio in Profile.FieldMapping.AudioOf(rawNote))
      {
         var ankisCopy = InAnkisMediaFolder(audio);
         var normalized = new Mp3File(File.ReadAllBytes(ankisCopy)).NormalizedToTheReplayGainReference().Bytes();
         _mediaStorage.StoreNewAudioFile(normalized, Profile.AudioFolder, audio.FileName);
         File.WriteAllBytes(ankisCopy, normalized);
      }
   }

   void StoreTheScreenshotsOf(RawNote rawNote)
   {
      foreach(var screenshot in Profile.FieldMapping.ScreenshotsOf(rawNote))
         _mediaStorage.StoreFile(InAnkisMediaFolder(screenshot), Profile.ScreenshotFolder, screenshot.FileName, MediaType.Image);
   }

   void AddTheSentenceOf(RawNote rawNote)
   {
      var sentence = new SentenceNote(_noteServices, Profile.FieldMapping.SentenceDataFrom(rawNote, Profile.TagsToAdd));
      sentence.UpdateGeneratedData();

      if(Profile.CreatesAnkiNotesToStudy)
         _noteServices.Collection.Sentences.Add(sentence);
      else
         _noteServices.Collection.Sentences.AddWithoutAnAnkiNote(sentence);
   }

   void FinishWithTheRawNotes()
   {
      if(Profile.DeletesTheRawNotesAfterwards)
         _ankiRawNotes.Delete(RawNotes);
      else
         _ankiRawNotes.TagAsImported(RawNotes);
   }

   string InAnkisMediaFolder(MediaReference file) => Path.Combine(_ankiMediaFolder, file.FileName);
}
