using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Storage.Media;

namespace JAStudio.Core.Importing;

/// <summary>
/// Checks an <see cref="ImportProfile"/> and the <see cref="RawNote"/>s it selects before an <see cref="ImportRun"/> changes anything:<br/>
/// finds the <see cref="ImportProblem"/>s that stop the run, and the <see cref="AlreadyImportedNote"/>s it skips.
/// </summary>
class ImportRunValidation(ImportProfile profile, IReadOnlyList<AnkiNoteType> noteTypesForRawNotes, IReadOnlySet<string> externalIdsInTheCorpus, MediaFileIndex mediaFileIndex, string ankiMediaFolder)
{
   readonly ImportProfile _profile = profile;
   readonly SentenceFieldMapping _mapping = profile.FieldMapping;
   readonly IReadOnlyList<AnkiNoteType> _noteTypesForRawNotes = noteTypesForRawNotes;
   readonly IReadOnlySet<string> _externalIdsInTheCorpus = externalIdsInTheCorpus;
   readonly MediaFileIndex _mediaFileIndex = mediaFileIndex;
   readonly string _ankiMediaFolder = ankiMediaFolder;

   /// <summary>The problems with the import profile itself. While there are any, its raw notes can't even be selected.</summary>
   public List<ImportProblem> ProblemsWithTheProfile()
   {
      var problems = new List<string>();
      var noteType = _noteTypesForRawNotes.SingleOrDefault(it => it.Name == _profile.AnkiNoteType);

      if(_profile.AnkiNoteType == "") problems.Add("Choose the Anki note type of the raw notes.");
      else if(noteType == null) problems.Add($"Anki has no note type named {_profile.AnkiNoteType} that raw notes can have. JAStudio's own note types can't be imported from.");

      if(_mapping.SourceQuestion == "") problems.Add("Map an Anki field to the sentence's source question.");

      if(noteType != null)
         problems.AddRange(_mapping.MappedAnkiFields().Where(it => !noteType.FieldNames.Contains(it)).Distinct().Select(it => $"The note type {noteType.Name} has no field named {it}."));

      if(_mapping.Audio != "" && !MediaLayerFolders.IsInsideFreeOrACommercialRepository(_profile.AudioFolder))
         problems.Add($"The audio folder, '{_profile.AudioFolder}', must be free/ or a commercial repository, such as commercial-01/, or a folder inside one.");

      if(_mapping.Screenshot != "" && !MediaLayerFolders.IsInsideFreeOrACommercialRepository(_profile.ScreenshotFolder))
         problems.Add($"The screenshot folder, '{_profile.ScreenshotFolder}', must be free/ or a commercial repository, such as commercial-01/, or a folder inside one.");

      return [.. problems.Select(it => new ImportProblem(null, it))];
   }

   /// <summary>The notes of <paramref name="rawNotes"/> that look imported already, and the problems of the rest.</summary>
   public (List<AlreadyImportedNote> alreadyImported, List<ImportProblem> problems) Validate(IReadOnlyList<RawNote> rawNotes)
   {
      var alreadyImported = rawNotes.Select(WhyItLooksImportedAlready).OfType<AlreadyImportedNote>().ToList();
      var notesToImport = rawNotes.Except(alreadyImported.Select(it => it.RawNote)).ToList();
      var fileNamesOfMoreThanOneNote = FileNamesOfMoreThanOneNoteIn(notesToImport);

      var problems = notesToImport.SelectMany(it => ProblemsOf(it, fileNamesOfMoreThanOneNote)).ToList();
      return (alreadyImported, problems);
   }

   AlreadyImportedNote? WhyItLooksImportedAlready(RawNote rawNote)
   {
      var matches = new List<string>();

      var externalId = _mapping.ExternalIdOf(rawNote);
      if(externalId != "" && _externalIdsInTheCorpus.Contains(externalId)) matches.Add($"its external id {externalId} is in the corpus");

      matches.AddRange(_mapping.MediaOf(rawNote).Where(it => _mediaFileIndex.ContainsByOriginalFileName(it.FileName)).Select(it => $"its file {it.FileName} is in the media layer"));

      return matches.Count == 0 ? null : new AlreadyImportedNote(rawNote, _mapping.QuestionOf(rawNote), string.Join(", and ", matches));
   }

   IEnumerable<ImportProblem> ProblemsOf(RawNote rawNote, HashSet<string> fileNamesOfMoreThanOneNote)
   {
      if(_mapping.QuestionOf(rawNote).Trim() == "") yield return new ImportProblem(rawNote, $"Its field {_mapping.SourceQuestion}, the sentence's source question, is empty.");

      foreach(var file in _mapping.MediaOf(rawNote))
      {
         if(!File.Exists(Path.Combine(_ankiMediaFolder, file.FileName))) yield return new ImportProblem(rawNote, $"Its file {file.FileName} isn't in Anki's media folder.");
         if(fileNamesOfMoreThanOneNote.Contains(file.FileName)) yield return new ImportProblem(rawNote, $"Its file {file.FileName} is referenced by another raw note of this run too.");
      }

      foreach(var audio in _mapping.AudioOf(rawNote).Where(IsNotAnMp3File))
         yield return new ImportProblem(rawNote, $"Its audio {audio.FileName} isn't an MP3 file, and the corpus holds only MP3.");
   }

   HashSet<string> FileNamesOfMoreThanOneNoteIn(IEnumerable<RawNote> rawNotes) =>
      rawNotes.SelectMany(it => _mapping.MediaOf(it).Select(file => file.FileName).Distinct(StringComparer.OrdinalIgnoreCase))
              .GroupBy(it => it, StringComparer.OrdinalIgnoreCase)
              .Where(it => it.Count() > 1)
              .Select(it => it.Key)
              .ToHashSet(StringComparer.OrdinalIgnoreCase);

   static bool IsNotAnMp3File(MediaReference audio) => !audio.FileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase);
}
