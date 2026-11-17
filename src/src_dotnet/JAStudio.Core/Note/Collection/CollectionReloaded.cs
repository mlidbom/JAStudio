using System.Collections.Generic;

namespace JAStudio.Core.Note.Collection;

public sealed class CollectionReloaded(IReadOnlyList<NoteId> added, IReadOnlyList<NoteId> changed, IReadOnlyList<NoteId> removed) : ICollectionReloaded
{
   public IReadOnlyList<NoteId> Added { get; } = added;
   public IReadOnlyList<NoteId> Changed { get; } = changed;
   public IReadOnlyList<NoteId> Removed { get; } = removed;
}
