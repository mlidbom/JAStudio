using System.IO;
using JAStudio.Core.Note;
using JAStudio.Core.Note.NoteFields;

namespace JAStudio.Core.Storage.Media;

public class MediaStorageService
{
   readonly string _mediaRoot;
   readonly MediaFileIndex _index;

   public MediaStorageService(IEnvironmentPaths paths, MediaFileIndex index)
      : this(paths.MediaDir, index) {}

   public MediaStorageService(string mediaRoot, MediaFileIndex index)
   {
      _mediaRoot = mediaRoot;
      _index = index;
   }

   public MediaFileId StoreFile(string sourceFilePath, string targetDirectory, SourceTag sourceTag, string originalFileName, NoteId noteId, MediaType mediaType, CopyrightStatus copyright, TtsInfo? tts = null)
   {
      var id = MediaFileId.New();
      var destPath = BuildStoragePath(id, targetDirectory, originalFileName);

      Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
      File.Copy(sourceFilePath, destPath, overwrite: false);

      MediaAttachment attachment;
      if(mediaType == MediaType.Audio)
      {
         var audio = new AudioAttachment
                     {
                        Id = id,
                        NoteIds = [noteId],
                        NoteSourceTag = sourceTag,
                        OriginalFileName = originalFileName,
                        Copyright = copyright,
                        Tts = tts,
                        FilePath = destPath
                     };

         var sidecarPath = SidecarSerializer.BuildAudioSidecarPath(destPath);
         SidecarSerializer.WriteAudioSidecar(sidecarPath, audio);
         attachment = audio;
      } else
      {
         var image = new ImageAttachment
                     {
                        Id = id,
                        NoteIds = [noteId],
                        NoteSourceTag = sourceTag,
                        OriginalFileName = originalFileName,
                        Copyright = copyright,
                        FilePath = destPath
                     };

         var sidecarPath = SidecarSerializer.BuildImageSidecarPath(destPath);
         SidecarSerializer.WriteImageSidecar(sidecarPath, image);
         attachment = image;
      }

      _index.Register(attachment);
      return id;
   }

   public void AddNoteIdToExisting(MediaAttachment existing, NoteId noteId)
   {
      if(existing.NoteIds.Contains(noteId)) return;
      existing.NoteIds.Add(noteId);

      if(string.IsNullOrEmpty(existing.FilePath)) return;

      if(existing is AudioAttachment audio)
      {
         var sidecarPath = SidecarSerializer.BuildAudioSidecarPath(existing.FilePath);
         SidecarSerializer.WriteAudioSidecar(sidecarPath, audio);
      } else if(existing is ImageAttachment image)
      {
         var sidecarPath = SidecarSerializer.BuildImageSidecarPath(existing.FilePath);
         SidecarSerializer.WriteImageSidecar(sidecarPath, image);
      }
   }

   string BuildStoragePath(MediaFileId id, string targetDirectory, string originalFileName)
   {
      var bucket = id.ToString()[..2];
      var extension = Path.GetExtension(originalFileName);

      // {mediaRoot}/{targetDirectory}/{bucket}/{id}.{ext}
      return Path.Combine(_mediaRoot, targetDirectory, bucket, $"{id}{extension}");
   }
}
