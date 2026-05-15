using System.Collections.Generic;
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
}
