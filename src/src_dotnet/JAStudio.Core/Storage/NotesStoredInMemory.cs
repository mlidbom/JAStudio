using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Compze.Abstractions;
using Compze.Teventive.StateStore.Storage;
using JAStudio.Core.Note;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Storage;

/// <summary>
/// The notes the specifications store, held in memory instead of the corpus's files, as the storage of the state store that keeps the notes: each<br/>
/// note's snapshot, its DTO, as the unit of work that last changed it committed it. Units of work commit on many threads at once, as the reparse does.
/// </summary>
public sealed partial class NotesStoredInMemory : ITaggregateStorage
{
   readonly ConcurrentDictionary<TaggregateId, TaggregateSnapshot> _stored = new();

   public IReadOnlyList<TaggregateSnapshot> LoadAll() => [.. _stored.Values];

   public IPreparedWrite Prepare(IReadOnlyList<TaggregateSnapshot> toSave, IReadOnlyList<TaggregateToDelete> toDelete) => new PreparedWrite(this, toSave, toDelete);

   /// <summary>The snapshot of each stored note whose snapshot is a <typeparamref name="TSnapshot"/>, such as every vocab's.</summary>
   public IReadOnlyList<TSnapshot> Snapshots<TSnapshot>() where TSnapshot : CorpusNoteDto => [.. _stored.Values.Select(it => it.Snapshot).OfType<TSnapshot>()];

   /// <summary>The snapshot stored of the note <paramref name="noteId"/>.</summary>
   public TSnapshot SnapshotOf<TSnapshot>(NoteId noteId) where TSnapshot : CorpusNoteDto => (TSnapshot)_stored[noteId].Snapshot;

   /// <summary>Stores <paramref name="note"/> as it is now, in place of what is stored of it, as a change to its file on disk would.</summary>
   public void Store(JPNote note) => _stored[note.Id] = new TaggregateSnapshot(note.GetType(), note.Id, note.ToDto());
}
