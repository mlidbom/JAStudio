using System.IO;
using Compze.Teventive.StateStore.Storage;

namespace JAStudio.Core.Storage;

public partial class JsonFilesystemObjectRepository<TData>
{
   /// <summary>An object's file, written under a temporary name beside its own: committing gives it its name, replacing the file it had.</summary>
   sealed class PreparedSaving : IPreparedWrite
   {
      readonly string _temporaryPath;
      readonly string _path;

      internal PreparedSaving(string temporaryPath, string path)
      {
         _temporaryPath = temporaryPath;
         _path = path;
      }

      public void Commit() => File.Move(_temporaryPath, _path, overwrite: true);

      public void Discard() => File.Delete(_temporaryPath);
   }
}
