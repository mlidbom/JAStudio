using System.Collections.Generic;

namespace JAStudio.Core.Note.Collection;

public sealed class CollectionReloaded(IReadOnlyCollection<NoteId> added, IReadOnlyCollection<NoteId> changed, IReadOnlyCollection<NoteId> removed) : ICollectionReloaded
{
   public IReadOnlyCollection<NoteId> Added { get; } = added;
   public IReadOnlyCollection<NoteId> Changed { get; } = changed;
   public IReadOnlyCollection<NoteId> Removed { get; } = removed;
}
