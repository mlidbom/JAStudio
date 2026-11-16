using System.IO;
using Compze.Teventive.StateStore.Storage;

namespace JAStudio.Core.Storage;

public partial class JsonFilesystemObjectRepository<TData>
{
   /// <summary>
   /// An object's file, to delete when the write commits, if the object has one: one that was never saved has none. Deleting has nothing to<br/>
   /// prepare: the file stays where it is until then, so a crash before the commit leaves it as it was.
   /// </summary>
   sealed class PreparedDeleting : IPreparedWrite
   {
      readonly string _path;

      internal PreparedDeleting(string path) => _path = path;

      public void Commit()
      {
         if(File.Exists(_path)) File.Delete(_path);
      }

      public void Discard() {}
   }
}
