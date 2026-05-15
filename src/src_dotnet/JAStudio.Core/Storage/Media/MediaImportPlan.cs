using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.NoteFields;

namespace JAStudio.Core.Storage.Media;

public class PlannedFileImport(
   string sourcePath,
   string targetDirectory,
   SourceTag sourceTag,
   string originalFileName,
   NoteId noteId,
   MediaType mediaType)
{
   public string SourcePath { get; } = sourcePath;
   public string TargetDirectory { get; } = targetDirectory;
   public SourceTag SourceTag { get; } = sourceTag;
   public string OriginalFileName { get; } = originalFileName;
   public NoteId NoteId { get; } = noteId;
   public MediaType MediaType { get; } = mediaType;
}

public class AlreadyStoredFile(MediaAttachment existing, NoteId noteId)
{
   public MediaAttachment Existing { get; } = existing;
   public NoteId NoteId { get; } = noteId;
}

public class MissingFile(string fileName, NoteId noteId, string fieldName)
{
   public string FileName { get; } = fileName;
   public NoteId NoteId { get; } = noteId;
   public string FieldName { get; } = fieldName;
}

public class MediaImportPlan
{
   public List<PlannedFileImport> FilesToImport { get; } = [];
   public List<AlreadyStoredFile> AlreadyStored { get; } = [];
   public List<MissingFile> Missing { get; } = [];

   public static MediaImportPlan From(IEnumerable<NoteMediaFieldScan> scans)
   {
      var plan = new MediaImportPlan();
      foreach(var scan in scans)
      {
         if(scan.IndexedAttachment != null && scan.MatchingRule != null)
            plan.AlreadyStored.Add(new AlreadyStoredFile(scan.IndexedAttachment, scan.NoteId));
         else if(scan.MatchingRule != null && scan.AnkiSourcePath != null)
            plan.FilesToImport.Add(new PlannedFileImport(scan.AnkiSourcePath, scan.MatchingRule.TargetDirectory, scan.SourceTag, scan.FileName, scan.NoteId, scan.MediaType));
         else if(scan.MatchingRule != null)
            plan.Missing.Add(new MissingFile(scan.FileName, scan.NoteId, scan.FieldName));
      }
      return plan;
   }
}
