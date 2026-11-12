namespace JAStudio.Core.Note.Collection;

/// <summary>
/// What a <see cref="NoteCache{TNote,TSnapshot}"/> does to each of its indexes, <see cref="NoteIndex{TKey,TNote,TSnapshot}"/>, whatever its key, as its<br/>
/// notes change. The changes are made under the cache's lock, and lookups see none of them until <see cref="PublishChanges"/>.
/// </summary>
interface INoteIndex<in TNote, in TSnapshot>
{
   /// <summary>Files <paramref name="note"/> under the keys of <paramref name="snapshot"/>.</summary>
   void Add(TNote note, TSnapshot snapshot);

   /// <summary>Takes <paramref name="note"/> out from under the keys of <paramref name="snapshot"/>, the snapshot it was filed by.</summary>
   void Remove(TNote note, TSnapshot snapshot);

   /// <summary>
   /// Refiles <paramref name="note"/>, filed by <paramref name="before"/>, by <paramref name="after"/>: under only the keys that changed, so a note<br/>
   /// whose keys didn't change costs nothing.
   /// </summary>
   void Update(TNote note, TSnapshot before, TSnapshot after);

   /// <summary>Empties the index, for lookups too, at once.</summary>
   void Clear();

   /// <summary>Makes the changes since the last call what lookups see.</summary>
   void PublishChanges();
}
