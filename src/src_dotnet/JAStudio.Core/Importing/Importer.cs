using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.Importing;

/// <summary>Prepares <see cref="ImportRun"/>s: selects an <see cref="ImportProfile"/>'s raw notes in Anki and validates them, before anything changes.</summary>
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
   /// Selects the raw notes of <paramref name="profileBeingEdited"/> and validates the profile and them. While the profile itself has problems,<br/>
   /// such as an unmapped question, no raw notes are selected. The run gets a <see cref="ImportProfile.Copy"/> of the profile, which later edits don't reach.
   /// </summary>
   public ImportRun Prepare(ImportProfile profileBeingEdited)
   {
      var profile = profileBeingEdited.Copy();
      var ankiMediaFolder = _paths.AnkiMediaDir;
      var validation = new ImportRunValidation(profile, NoteTypesForRawNotes(), ExternalIdsInTheCorpus(), _mediaFileIndex, ankiMediaFolder);

      var problemsWithTheProfile = validation.ProblemsWithTheProfile();
      if(problemsWithTheProfile.Count > 0)
         return Run([], problemsWithTheProfile, []);

      var rawNotes = _ankiRawNotes.NotYetImported(profile);
      var (alreadyImported, problems) = validation.Validate(rawNotes);
      return Run(rawNotes, problems, alreadyImported);

      ImportRun Run(IReadOnlyList<RawNote> selected, IReadOnlyList<ImportProblem> foundProblems, IReadOnlyList<AlreadyImportedNote> skipped) =>
         new(profile, selected, foundProblems, skipped, _ankiRawNotes, _noteServices, _mediaStorage, ankiMediaFolder, _taskRunner);
   }

   HashSet<string> ExternalIdsInTheCorpus() =>
      [.. _noteServices.Collection.Sentences.All().Select(it => it.ExternalId.Value).Where(it => it != "")];
}
