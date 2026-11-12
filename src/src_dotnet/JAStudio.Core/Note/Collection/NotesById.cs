using System.Collections.Generic;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// A note cache's notes by id. Any number of threads look notes up in it at once, without locking, while the cache changes it one change at a<br/>
/// time, under its lock.
/// </summary>
/// <remarks>
/// As in a <see cref="NoteIndex{TKey,TNote,TSnapshot}"/>, a change never touches what lookups read: it changes a copy of the notes, which<br/>
/// <see cref="PublishChanges"/> puts in place. Copying keeps the notes' order, so <see cref="All"/> lists them in the order they were cached.
/// </remarks>
sealed class NotesById<TNote> where TNote : JPNote
{
   /// <summary>What lookups read: a dictionary no change touches once it's here.</summary>
   volatile Dictionary<NoteId, TNote> _published = new();

   /// <summary>The copy of <see cref="_published"/> the change under way has changed, if it has changed any, which <see cref="PublishChanges"/> puts in place.</summary>
   Dictionary<NoteId, TNote>? _changedCopy;

   public TNote? WithIdOrNone(NoteId noteId) => _published.GetValueOrDefault(noteId);

   public List<TNote> All() => [.. _published.Values];

   /// <summary>Holds <paramref name="note"/> under its id. When it holds this very note already, as it does when a cached note was changed, that changes nothing.</summary>
   public void Set(TNote note)
   {
      if(ReferenceEquals((_changedCopy ?? _published).GetValueOrDefault(note.GetId()), note)) return;

      ChangedCopy()[note.GetId()] = note;
   }

   public void Remove(NoteId noteId) => ChangedCopy().Remove(noteId);

   /// <summary>Holds no notes any more, for lookups too, at once.</summary>
   public void Clear()
   {
      _changedCopy = null;
      _published = new Dictionary<NoteId, TNote>();
   }

   /// <summary>Makes the changes since the last call what lookups see.</summary>
   public void PublishChanges()
   {
      if(_changedCopy == null) return;

      _published = _changedCopy;
      _changedCopy = null;
   }

   Dictionary<NoteId, TNote> ChangedCopy() => _changedCopy ??= new Dictionary<NoteId, TNote>(_published);
}
