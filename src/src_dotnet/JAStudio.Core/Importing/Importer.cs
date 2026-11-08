using System.Collections.Generic;
using JAStudio.Core.Importing.Checking;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.Importing;

/// <summary>Prepares <see cref="ImportRun"/>s: selects an <see cref="ImportProfile"/>'s raw notes in Anki and checks the profile and them, before anything changes.</summary>
public class Importer(IAnkiRawNotes ankiRawNotes, NoteServices noteServices, MediaFileIndex mediaFileIndex, MediaStorageService mediaStorage, IEnvironmentPaths paths, TaskRunner taskRunner)
{
   readonly IAnkiRawNotes _ankiRawNotes = ankiRawNotes;
   readonly NoteServices _noteServices = noteServices;
   readonly MediaFileIndex _mediaFileIndex = mediaFileIndex;
   readonly MediaStorageService _mediaStorage = mediaStorage;
   readonly IEnvironmentPaths _paths = paths;
   readonly TaskRunner _taskRunner = taskRunner;

   /// <summary>The note types in Anki that an <see cref="ImportProfile"/> can take raw notes of.</summary>
   public IReadOnlyList<AnkiNoteType> NoteTypesForRawNotes() => _ankiRawNotes.NoteTypesForRawNotes();

   /// <summary>
   /// Selects the raw notes of <paramref name="profileBeingEdited"/> and checks the profile and them. While the profile itself has problems,<br/>
   /// such as an unmapped question, no raw notes are selected. The run gets a <see cref="ImportProfile.Copy"/> of the profile, which later edits don't reach.
   /// </summary>
   public ImportRun Prepare(ImportProfile profileBeingEdited)
   {
      var profile = profileBeingEdited.Copy();
      var ankiMediaFolder = _paths.AnkiMediaDir;
      var check = new ImportCheck(profile, NoteTypesForRawNotes(), _noteServices.Collection, _mediaFileIndex, ankiMediaFolder);

      var problemsWithTheProfile = check.ProblemsWithTheProfile();
      List<CheckedRawNote> checkedRawNotes = problemsWithTheProfile.Count > 0 ? [] : check.Check(_ankiRawNotes.SelectedBy(profile));
      return new ImportRun(profile, problemsWithTheProfile, checkedRawNotes, _ankiRawNotes, _noteServices, _mediaFileIndex, _mediaStorage, ankiMediaFolder, _taskRunner);
   }
}
