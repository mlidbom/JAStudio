using System.Collections.Generic;
using System.IO;
using JAStudio.Core.Note;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Storage.Media;

/// <summary>
/// Scans all notes in a single pass, producing one <see cref="NoteMediaFieldImportState"/> per media reference.
/// Each scan entry carries everything known about the file: source note, field, index status, and Anki source path.
/// Rule matching is applied separately via <see cref="ImportRulesCE.TryResolve"/>.
/// </summary>
public class NoteMediaFieldScanner
{
   readonly string _ankiMediaDir;
   readonly MediaFileIndex _index;

   public NoteMediaFieldScanner(string ankiMediaDir, MediaFileIndex index)
   {
      _ankiMediaDir = ankiMediaDir;
      _index = index;
   }

   public List<NoteMediaFieldImportState> GetVocabMediaFieldsImportState(IReadOnlyList<VocabNote> notes)
   {
      var result = new List<NoteMediaFieldImportState>();
      foreach(var note in notes)
      {
         var sourceTag = note.SourceTag;
         var noteId = note.GetId();
         ScanField(note.Audio.First.GetMediaReferences(),  nameof(VocabMediaField.AudioFirst),  sourceTag, noteId, result);
         ScanField(note.Audio.Second.GetMediaReferences(), nameof(VocabMediaField.AudioSecond), sourceTag, noteId, result);
         ScanField(note.Audio.Tts.GetMediaReferences(),    nameof(VocabMediaField.AudioTts),    sourceTag, noteId, result);
         ScanField(note.Image.GetMediaReferences(),        nameof(VocabMediaField.Image),       sourceTag, noteId, result);
         ScanField(note.UserImage.GetMediaReferences(),    nameof(VocabMediaField.UserImage),   sourceTag, noteId, result);
      }
      return result;
   }

   public List<NoteMediaFieldImportState> GetSentenceMediaFieldsImportState(IReadOnlyList<SentenceNote> notes)
   {
      var result = new List<NoteMediaFieldImportState>();
      foreach(var note in notes)
      {
         var sourceTag = note.SourceTag;
         var noteId = note.GetId();
         ScanField(note.Audio.GetMediaReferences(),      nameof(SentenceMediaField.Audio),      sourceTag, noteId, result);
         ScanField(note.Screenshot.GetMediaReferences(), nameof(SentenceMediaField.Screenshot), sourceTag, noteId, result);
      }
      return result;
   }

   void ScanField(List<MediaReference> references, string fieldName, SourceTag sourceTag, NoteId noteId, List<NoteMediaFieldImportState> result)
   {
      foreach(var reference in references)
      {
         var indexed = _index.TryGetByOriginalFileName(reference.FileName);
         var ankiSourcePath = indexed == null ? ResolveAnkiSourcePath(reference.FileName) : null;
         result.Add(new NoteMediaFieldImportState(sourceTag, noteId, fieldName, reference.FileName, reference.Type, indexed, ankiSourcePath));
      }
   }

   string? ResolveAnkiSourcePath(string fileName)
   {
      var path = Path.Combine(_ankiMediaDir, fileName);
      return File.Exists(path) ? path : null;
   }
}
