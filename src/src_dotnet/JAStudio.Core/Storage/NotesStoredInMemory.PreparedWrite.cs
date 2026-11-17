using System.Collections.Generic;
using Compze.Teventive.StateStore.Storage;

namespace JAStudio.Core.Storage;

public sealed partial class NotesStoredInMemory
{
   /// <summary>
   /// What a unit of work stores, made when it commits: nothing is held until then, so discarding it does nothing. It keeps of each note only what<br/>
   /// a file would hold, its snapshot, and not the note the state store hands over with it: the store would take that note back at a reload,<br/>
   /// instead of restoring the note from what is stored, as it does from the corpus's files.
   /// </summary>
   sealed class PreparedWrite : IPreparedWrite
   {
      readonly NotesStoredInMemory _storage;
      readonly IReadOnlyList<TaggregateSnapshot> _toSave;
      readonly IReadOnlyList<TaggregateToDelete> _toDelete;

      public PreparedWrite(NotesStoredInMemory storage, IReadOnlyList<TaggregateSnapshot> toSave, IReadOnlyList<TaggregateToDelete> toDelete)
      {
         _storage = storage;
         _toSave = toSave;
         _toDelete = toDelete;
      }

      public void Commit()
      {
         foreach(var note in _toSave) _storage._stored[note.Id] = new TaggregateSnapshot(note.TaggregateClass, note.Id, note.Snapshot);
         foreach(var note in _toDelete) _storage._stored.TryRemove(note.Id, out _);
      }

      public void Discard() {}
   }
}
