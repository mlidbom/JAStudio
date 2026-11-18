using System.Collections.Generic;
using Compze.Tessaging.TessageTypes;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// The collection was loaded again (<see cref="JPCollection.ReloadFromBackend"/>): at startup, and when asked to, as after a change to the corpus<br/>
/// checkout outside JAStudio. It holds, by their ids, the notes the reload added, those whose files changed on disk, which it replaced, and those<br/>
/// it removed, each in no order. Anything that holds on to a note, rather than its reference, must look the changed ones up again.
/// </summary>
public interface ICollectionReloaded : ITevent
{
   IReadOnlyCollection<NoteId> Added { get; }
   IReadOnlyCollection<NoteId> Changed { get; }
   IReadOnlyCollection<NoteId> Removed { get; }
}
