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
/// finds the <see cref="ImportProfileProblem"/>s, and checks each field of each raw note, deciding its <see cref="RawNoteOutcome"/>.
/// </summary>
/// <remarks>
/// A raw note is checked by what it is. A note an earlier run imported is checked for what's left of it in JAStudio; a note that isn't tagged is first<br/>
/// checked against what's in JAStudio, and skipped on a match; only a note to import is checked for problems.<br/>
/// Notes and media files live apart: deleting a sentence leaves its files in the media layer. A file there that no note uses is reused, when it's the same file.
/// </remarks>
class ImportCheck(ImportProfile profile, IReadOnlyList<AnkiNoteType> noteTypesForRawNotes, JPCollection collection, MediaFileIndex mediaFileIndex, string ankiMediaFolder)
{
   const string Audio = "Audio";
   const string Screenshot = "Screenshot";

   readonly ImportProfile _profile = profile;
   readonly SentenceFieldMapping _mapping = profile.FieldMapping;
   readonly IReadOnlyList<AnkiNoteType> _noteTypesForRawNotes = noteTypesForRawNotes;
   readonly WhatIsInJAStudio _whatIsInJAStudio = new(collection, mediaFileIndex);
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

   /// <summary>Each of <paramref name="rawNotes"/> with its outcome and the check of each of its fields.</summary>
   public List<CheckedRawNote> Check(IReadOnlyList<RawNote> rawNotes)
   {
      var notesToCheckForProblems = rawNotes.Where(it => !it.IsTaggedAsImported && !_whatIsInJAStudio.HasAnythingOf(it, _mapping)).ToList();
      var fileNamesOfMoreThanOneNoteToImport = FileNamesOfMoreThanOneNoteIn(notesToCheckForProblems);

      return [.. rawNotes.Select(it => it.IsTaggedAsImported ? CheckOfANoteImportedEarlier(it)
                                    : _whatIsInJAStudio.HasAnythingOf(it, _mapping) ? CheckOfANoteAlreadyInJAStudio(it)
                                    : CheckOfANoteToImport(it, fileNamesOfMoreThanOneNoteToImport))];
   }

   /// <summary>
   /// A note an earlier run imported is fine while anything of it is left in JAStudio. Its file that no note uses any more is normal, as after its audio was replaced.<br/>
   /// When nothing of it is left, it's a problem: only Magnus can decide whether to import it again or to delete it.
   /// </summary>
   CheckedRawNote CheckOfANoteImportedEarlier(RawNote rawNote)
   {
      var externalId = _mapping.ExternalIdOf(rawNote);
      var hasSomethingToFindItBy = externalId != "" || _mapping.MediaOf(rawNote).Count > 0;
      var isGoneFromJAStudio = hasSomethingToFindItBy && !_whatIsInJAStudio.HasAnythingOf(rawNote, _mapping);

      return isGoneFromJAStudio
                ? Checked(rawNote,
                          RawNoteOutcome.Problem,
                          ExternalIdCheck(externalId, () => FieldCheck.ProblemOfTheNoteAsAWhole(externalId, $"No sentence in the corpus has the external id {externalId} any more.")),
                          MediaFieldCheck(_mapping.Audio, _mapping.AudioOf(rawNote), FileOfANoteGoneFromJAStudio),
                          MediaFieldCheck(_mapping.Screenshot, _mapping.ScreenshotsOf(rawNote), FileOfANoteGoneFromJAStudio),
                          [$"Imported by an earlier run, but no longer in JAStudio: remove its {RawNote.ImportedTag} tag in Anki to import it again, or delete it."])
                : Checked(rawNote,
                          RawNoteOutcome.ImportedByAnEarlierRun,
                          ExternalIdCheck(externalId, () => _whatIsInJAStudio.SentencesWithTheExternalId(externalId) is { Count: > 0 } sentences
                                                   ? FieldCheck.Fine(externalId, $"A sentence in the corpus has the external id {externalId}: {WhatIsInJAStudio.Describe(sentences)}.")
                                                   : FieldCheck.Neutral(externalId, $"No sentence in the corpus has the external id {externalId} any more.")),
                          MediaFieldCheck(_mapping.Audio, _mapping.AudioOf(rawNote), FileOfANoteImportedEarlier),
                          MediaFieldCheck(_mapping.Screenshot, _mapping.ScreenshotsOf(rawNote), FileOfANoteImportedEarlier),
                          []);

      FieldCheck FileOfANoteGoneFromJAStudio(MediaReference file) =>
         FieldCheck.ProblemOfTheNoteAsAWhole(file.FileName, _whatIsInJAStudio.InTheMediaLayer(file.FileName) == null
                                                               ? $"The file {file.FileName} isn't in the media layer."
                                                               : $"The file {file.FileName} is in the media layer, but no note uses it.");

      FieldCheck FileOfANoteImportedEarlier(MediaReference file) =>
         _whatIsInJAStudio.NotesUsing(file.FileName) is { Count: > 0 } notes
            ? FieldCheck.Fine(file.FileName, $"The file {file.FileName} is in the media layer, used by {WhatIsInJAStudio.Describe(notes)}.")
            : FieldCheck.Neutral(file.FileName, _whatIsInJAStudio.InTheMediaLayer(file.FileName) == null
                                                   ? $"The file {file.FileName} isn't in the media layer."
                                                   : $"The file {file.FileName} is in the media layer, but no note uses it.");
   }

   /// <summary>A note that isn't tagged as imported, of which JAStudio has something: it's skipped, and not checked for problems.</summary>
   CheckedRawNote CheckOfANoteAlreadyInJAStudio(RawNote rawNote)
   {
      var externalId = _mapping.ExternalIdOf(rawNote);
      return Checked(rawNote,
                     RawNoteOutcome.AlreadyInJAStudio,
                     ExternalIdCheck(externalId, () => ExternalIdOfANoteThatIsNotTagged(externalId)),
                     MediaFieldCheck(_mapping.Audio, _mapping.AudioOf(rawNote), file => FileOfANoteAlreadyInJAStudio(file, Audio)),
                     MediaFieldCheck(_mapping.Screenshot, _mapping.ScreenshotsOf(rawNote), file => FileOfANoteAlreadyInJAStudio(file, Screenshot)),
                     []);

      FieldCheck FileOfANoteAlreadyInJAStudio(MediaReference file, string field) =>
         _whatIsInJAStudio.NotesUsing(file.FileName) is { Count: > 0 } notes
            ? FieldCheck.AlreadyInJAStudio(file.FileName, $"The file {file.FileName} is in the media layer, used by {WhatIsInJAStudio.Describe(notes)}.", $"{field} used by {WhatIsInJAStudio.Describe(notes)}")
            : FieldCheck.Neutral(file.FileName, "Not checked: the note is skipped.");
   }

   /// <summary>A note that isn't tagged as imported, of which JAStudio has nothing: it's checked for problems, any one of which stops the run.</summary>
   CheckedRawNote CheckOfANoteToImport(RawNote rawNote, HashSet<string> fileNamesOfMoreThanOneNoteToImport)
   {
      var question = _mapping.QuestionOf(rawNote);
      var externalId = _mapping.ExternalIdOf(rawNote);
      var questionCheck = question.Trim() == ""
                             ? FieldCheck.Problem(question, $"The question, its field {_mapping.SourceQuestion}, is empty.", "Question is empty")
                             : FieldCheck.Fine(question, "");
      var audio = MediaFieldCheck(_mapping.Audio, _mapping.AudioOf(rawNote), it => FileOfANoteToImport(it, Audio, _profile.AudioFolder));
      var screenshot = MediaFieldCheck(_mapping.Screenshot, _mapping.ScreenshotsOf(rawNote), it => FileOfANoteToImport(it, Screenshot, _profile.ScreenshotFolder));
      var hasAProblem = new[] { questionCheck, audio, screenshot }.Any(it => it.Mark == CheckMark.Problem);

      return new CheckedRawNote(rawNote,
                                hasAProblem ? RawNoteOutcome.Problem : RawNoteOutcome.WillBeImported,
                                questionCheck,
                                ExternalIdCheck(externalId, () => ExternalIdOfANoteThatIsNotTagged(externalId)),
                                audio,
                                screenshot,
                                []);

      FieldCheck FileOfANoteToImport(MediaReference file, string field, string folder)
      {
         var name = file.FileName;
         if(!File.Exists(InAnkisMediaFolder(file))) return FieldCheck.Problem(name, $"The file {name} isn't in Anki's media folder.", $"{field} {name} isn't in Anki's media folder");
         if(field == Audio && IsNotAnMp3File(file)) return FieldCheck.Problem(name, $"The audio {name} isn't an MP3 file, and the corpus holds only MP3.", $"Audio {name} isn't MP3");
         if(fileNamesOfMoreThanOneNoteToImport.Contains(name)) return FieldCheck.Problem(name, $"Another raw note of this run uses the file {name} too.", $"{field} {name} is used by another raw note of this run too");

         return _whatIsInJAStudio.InTheMediaLayer(name) switch
         {
            null => FieldCheck.Fine(name, $"The file {name} will be stored in {folder}."),
            var unused when IsTheSameFile(file, unused) => FieldCheck.Fine(name, $"The file {name} is in the media layer, but no note uses it: the import reuses it."),
            var unused => FieldCheck.Problem(name, $"A different file named {name} is in the media layer, though no note uses it, in {unused.FilePath}.", $"A different file named {name} is in the media layer")
         };
      }
   }

   FieldCheck ExternalIdOfANoteThatIsNotTagged(string externalId) =>
      _whatIsInJAStudio.SentencesWithTheExternalId(externalId) is { Count: > 0 } sentences
         ? FieldCheck.AlreadyInJAStudio(externalId, $"A sentence in the corpus has the external id {externalId}: {WhatIsInJAStudio.Describe(sentences)}.", $"External id taken by {WhatIsInJAStudio.Describe(sentences)}")
         : FieldCheck.Fine(externalId, $"No sentence in the corpus has the external id {externalId}.");

   /// <summary>The check of <paramref name="externalId"/>, or a grey mark when the profile maps none, or the raw note has none, which a deck doesn't always give.</summary>
   FieldCheck ExternalIdCheck(string externalId, Func<FieldCheck> checkOfTheExternalId) =>
      _mapping.ExternalId == "" ? FieldCheck.NotMapped()
      : externalId == "" ? FieldCheck.Neutral("", "No external id.")
      : checkOfTheExternalId();

   static FieldCheck MediaFieldCheck(string mappedAnkiField, List<MediaReference> files, Func<MediaReference, FieldCheck> checkOfAFile) =>
      mappedAnkiField == "" ? FieldCheck.NotMapped() : FieldCheck.OfAMediaField([.. files.Select(checkOfAFile)]);

   CheckedRawNote Checked(RawNote rawNote, RawNoteOutcome outcome, FieldCheck externalId, FieldCheck audio, FieldCheck screenshot, IReadOnlyList<string> whatIsWrongWithTheNoteAsAWhole) =>
      new(rawNote, outcome, FieldCheck.Neutral(_mapping.QuestionOf(rawNote), ""), externalId, audio, screenshot, whatIsWrongWithTheNoteAsAWhole);

   /// <summary>Whether <paramref name="inTheMediaLayer"/> is the same file as Anki's copy of <paramref name="file"/>: for audio, once Anki's copy is normalized, as the import would store it.</summary>
   bool IsTheSameFile(MediaReference file, MediaAttachment inTheMediaLayer)
   {
      var ankisCopy = File.ReadAllBytes(InAnkisMediaFolder(file));
      var asTheImportWouldStoreIt = file.Type == MediaType.Audio ? new Mp3File(ankisCopy).NormalizedToTheReplayGainReference().Bytes() : ankisCopy;
      return File.ReadAllBytes(inTheMediaLayer.FilePath).AsSpan().SequenceEqual(asTheImportWouldStoreIt);
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
