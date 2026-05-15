using JAStudio.Core.Note;
using JAStudio.Core.Note.NoteFields;

namespace JAStudio.Core.Storage.Media;

/// <summary>
/// Everything known about a single media file referenced by a specific field on a specific note.
/// Produced by <see cref="NoteMediaFieldScanner"/> in a single pass over all notes.
/// </summary>
public class NoteMediaFieldImportState
{
   public NoteMediaFieldImportState(SourceTag sourceTag, NoteId noteId, string fieldName, string fileName, MediaType mediaType, MediaAttachment? indexedAttachment, string? ankiSourcePath)
   {
      SourceTag = sourceTag;
      NoteId = noteId;
      FieldName = fieldName;
      FileName = fileName;
      MediaType = mediaType;
      IndexedAttachment = indexedAttachment;
      AnkiSourcePath = ankiSourcePath;
   }

   public SourceTag SourceTag { get; }
   public NoteId NoteId { get; }
   public string FieldName { get; }
   public string FileName { get; }
   public MediaType MediaType { get; }

   /// <summary>Non-null if the file is already in JAStudio's media index.</summary>
   public MediaAttachment? IndexedAttachment { get; }

   /// <summary>Non-null if the file exists in Anki's media folder (and is not yet indexed).</summary>
   public string? AnkiSourcePath { get; }

   /// <summary>Set by rule application — non-null if a rule covers this field+sourceTag combination.</summary>
   public ImportRule? MatchingRule { get; set; }
}
