using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Storage.Media;
using Mp3LoudnessNormalization;

namespace JAStudio.Core.Importing.Checking;

/// <summary>
/// Checks an <see cref="ImportProfile"/> and the <see cref="RawNote"/>s it selects before an <see cref="ImportRun"/> changes anything:<br/>
/// finds the <see cref="ImportProfileProblem"/>s, and decides each raw note's <see cref="RawNoteOutcome"/>, stating every <see cref="RawNoteFinding"/> behind it.
/// </summary>
/// <remarks>
/// Notes and media files live apart: deleting a sentence leaves its files in the media layer. So the check states what it finds about a raw note's<br/>
/// external id and about each of its files on its own. A file in the media layer that no note uses is reused, when it's the same file.
/// </remarks>
class ImportCheck(ImportProfile profile, IReadOnlyList<AnkiNoteType> noteTypesForRawNotes, JPCollection collection, MediaFileIndex mediaFileIndex, string ankiMediaFolder)
{
   readonly ImportProfile _profile = profile;
   readonly SentenceFieldMapping _mapping = profile.FieldMapping;
   readonly IReadOnlyList<AnkiNoteType> _noteTypesForRawNotes = noteTypesForRawNotes;
   readonly JPCollection _collection = collection;
   readonly MediaFileIndex _mediaFileIndex = mediaFileIndex;
   readonly string _ankiMediaFolder = ankiMediaFolder;

   /// <summary>The problems with the import profile itself. While there are any, its raw notes aren't even selected.</summary>
   public List<ImportProfileProblem> ProblemsWithTheProfile()
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

      return [.. problems.Select(it => new ImportProfileProblem(it))];
   }

   /// <summary>Each of <paramref name="rawNotes"/> with its outcome and findings.</summary>
   public List<CheckedRawNote> Check(IReadOnlyList<RawNote> rawNotes)
   {
      var whatIsInJAStudio = new WhatIsInJAStudio(_collection, _mediaFileIndex);
      var notesInJAStudioAlready = rawNotes.Where(it => !it.IsTaggedAsImported && whatIsInJAStudio.Has(it, _mapping)).ToHashSet();
      var candidates = rawNotes.Where(it => !it.IsTaggedAsImported && !notesInJAStudioAlready.Contains(it)).ToList();
      var fileNamesOfMoreThanOneCandidate = FileNamesOfMoreThanOneNoteIn(candidates);

      return [.. rawNotes.Select(CheckedRawNoteFor)];

      CheckedRawNote CheckedRawNoteFor(RawNote rawNote)
      {
         if(rawNote.IsTaggedAsImported)
            return Checked(rawNote, RawNoteOutcome.ImportedByAnEarlierRun, whatIsInJAStudio.FindingsAboutAnImportedNote(rawNote, _mapping));

         if(notesInJAStudioAlready.Contains(rawNote))
            return Checked(rawNote, RawNoteOutcome.AlreadyInJAStudio, whatIsInJAStudio.FindingsAboutANoteThatIsInJAStudio(rawNote, _mapping));

         var findings = FindingsAboutACandidate(rawNote, fileNamesOfMoreThanOneCandidate);
         return Checked(rawNote, findings.Any(it => it.Kind == RawNoteFindingKind.Problem) ? RawNoteOutcome.Problem : RawNoteOutcome.WillBeImported, findings);
      }
   }

   CheckedRawNote Checked(RawNote rawNote, RawNoteOutcome outcome, List<RawNoteFinding> findings) =>
      new(rawNote, _mapping.QuestionOf(rawNote), _mapping.ExternalIdOf(rawNote), outcome, findings);

   List<RawNoteFinding> FindingsAboutACandidate(RawNote rawNote, HashSet<string> fileNamesOfMoreThanOneCandidate)
   {
      var findings = new List<RawNoteFinding>();
      if(_mapping.QuestionOf(rawNote).Trim() == "") findings.Add(RawNoteFinding.Problem($"The question, its field {_mapping.SourceQuestion}, is empty."));

      foreach(var file in _mapping.MediaOf(rawNote))
      {
         if(!File.Exists(InAnkisMediaFolder(file)))
         {
            findings.Add(RawNoteFinding.Problem($"The file {file.FileName} isn't in Anki's media folder."));
            continue;
         }

         if(fileNamesOfMoreThanOneCandidate.Contains(file.FileName)) findings.Add(RawNoteFinding.Problem($"Another raw note of this run uses the file {file.FileName} too."));
         if(_mediaFileIndex.TryGetByOriginalFileName(file.FileName) is {} inTheMediaLayer) findings.Add(FindingAboutAnUnusedFileInTheMediaLayer(file, inTheMediaLayer));
      }

      findings.AddRange(_mapping.AudioOf(rawNote).Where(IsNotAnMp3File).Select(it => RawNoteFinding.Problem($"The audio {it.FileName} isn't an MP3 file, and the corpus holds only MP3.")));
      return findings;
   }

   /// <summary>
   /// No note uses <paramref name="inTheMediaLayer"/>, or the raw note would be in JAStudio already. When it's the same file as Anki's copy, the import reuses it:<br/>
   /// for audio, the same once Anki's copy is normalized, as the import normalizes it. A different file by the same name would give the sentence the wrong file.
   /// </summary>
   RawNoteFinding FindingAboutAnUnusedFileInTheMediaLayer(MediaReference file, MediaAttachment inTheMediaLayer)
   {
      return IsTheSameFile()
                ? RawNoteFinding.Information($"The file {file.FileName} is in the media layer, but no note uses it: the import reuses it.")
                : RawNoteFinding.Problem($"A different file named {file.FileName} is in the media layer, though no note uses it, in {inTheMediaLayer.FilePath}.");

      bool IsTheSameFile()
      {
         var ankisCopy = File.ReadAllBytes(InAnkisMediaFolder(file));
         var asTheImportWouldStoreIt = file.Type == MediaType.Audio && !IsNotAnMp3File(file) ? new Mp3File(ankisCopy).NormalizedToTheReplayGainReference().Bytes() : ankisCopy;
         return File.ReadAllBytes(inTheMediaLayer.FilePath).AsSpan().SequenceEqual(asTheImportWouldStoreIt);
      }
   }

   HashSet<string> FileNamesOfMoreThanOneNoteIn(IEnumerable<RawNote> rawNotes) =>
      rawNotes.SelectMany(it => _mapping.MediaOf(it).Select(file => file.FileName).Distinct(StringComparer.OrdinalIgnoreCase))
              .GroupBy(it => it, StringComparer.OrdinalIgnoreCase)
              .Where(it => it.Count() > 1)
              .Select(it => it.Key)
              .ToHashSet(StringComparer.OrdinalIgnoreCase);

   string InAnkisMediaFolder(MediaReference file) => Path.Combine(_ankiMediaFolder, file.FileName);

   static bool IsNotAnMp3File(MediaReference audio) => !audio.FileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase);
}
