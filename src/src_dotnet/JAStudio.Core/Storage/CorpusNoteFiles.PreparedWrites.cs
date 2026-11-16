using System.Collections.Generic;
using Compze.Teventive.StateStore.Storage;

namespace JAStudio.Core.Storage;

public partial class CorpusNoteFiles
{
   /// <summary>The writes of the files of the notes one unit of work changed, made or abandoned together.</summary>
   sealed class PreparedWrites : IPreparedWrite
   {
      readonly IReadOnlyList<IPreparedWrite> _writes;

      internal PreparedWrites(IReadOnlyList<IPreparedWrite> writes) => _writes = writes;

      public void Commit()
      {
         foreach(var it in _writes) it.Commit();
      }

      public void Discard()
      {
         foreach(var it in _writes) it.Discard();
      }
   }
}
