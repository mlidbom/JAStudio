using Compze.Teventive.MemoryImageStateStore.Testing;
using JAStudio.Core.Storage;

namespace JAStudio.Core.TestUtils;

/// <summary>Where the specifications' environment has the memory image state store keep the notes' files.</summary>
/// <remarks>
/// In memory unless a specification needs them on the disk, such as one that runs git on them: writing a file on the disk for each note, as each<br/>
/// unit of work commits, made the whole suite take about three times as long (taggregates-opus, 2026-11-17).
/// </remarks>
public enum NotesKept
{
   /// <summary>
   /// In memory: as the application keeps them (<see cref="CorpusNoteFiles"/>), but with the files kept in memory, which Compze's<br/>
   /// <see cref="Compze.Teventive.MemoryImageStateStore.IFileSystemInMemory"/> reads and changes.
   /// </summary>
   InMemory,

   /// <summary>In their files on the disk, in the specification's own corpus checkout, as the application keeps them (<see cref="CorpusNoteFiles"/>).</summary>
   InTheirFiles
}
