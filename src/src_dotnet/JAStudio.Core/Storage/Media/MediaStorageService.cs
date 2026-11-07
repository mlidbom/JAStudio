using System.IO;
using Compze.Contracts;
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

   /// <summary>
   /// Stores <paramref name="audioFile"/>, an audio file's bytes, as <paramref name="fileName"/> in <paramref name="targetDirectory"/> of the media layer,<br/>
   /// and registers it in the <see cref="MediaFileIndex"/>, so that a note referencing the name finds it at once. The name must be new to the index.
   /// </summary>
   public AudioAttachment StoreNewAudioFile(byte[] audioFile, string targetDirectory, string fileName)
   {
      Contract.Argument.Assert(!_index.ContainsByOriginalFileName(fileName), () => $"The media layer already has a file named {fileName}.");

      var filePath = Path.Combine(_mediaRoot, targetDirectory, fileName);
      Directory.CreateDirectory(Path.Combine(_mediaRoot, targetDirectory));
      File.WriteAllBytes(filePath, audioFile);
      var attachment = new AudioAttachment { OriginalFileName = fileName, FilePath = filePath };
      _index.Register(attachment);
      return attachment;
   }

   public void StoreFile(string sourceFilePath, string targetDirectory, string originalFileName, MediaType mediaType)
   {
      var destPath = Path.Combine(_mediaRoot, targetDirectory, originalFileName);
      Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);

      if(!File.Exists(destPath))
         File.Copy(sourceFilePath, destPath);

      MediaAttachment attachment = mediaType == MediaType.Audio
         ? new AudioAttachment { OriginalFileName = originalFileName, FilePath = destPath }
         : new ImageAttachment { OriginalFileName = originalFileName, FilePath = destPath };

      _index.Register(attachment);
   }
}
