using System.IO;
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
