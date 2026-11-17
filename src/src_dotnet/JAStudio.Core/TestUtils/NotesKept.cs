using JAStudio.Core.Storage;

namespace JAStudio.Core.TestUtils;

/// <summary>Where the specifications' environment has the state store keep the notes.</summary>
/// <remarks>
/// In memory unless a specification is about the notes' files: writing a file for each note, as each unit of work commits, made the whole suite<br/>
/// take about three times as long, mostly in the 74 specifications that start with the specifications' collection, 1,035 notes, each added<br/>
/// in a unit of work of its own (taggregates-opus, 2026-11-17).
/// </remarks>
public enum NotesKept
{
   /// <summary>In <see cref="NotesStoredInMemory"/>.</summary>
   InMemory,

   /// <summary>In their files in the specification's own corpus checkout (<see cref="CorpusNoteFiles"/>), as the application keeps them.</summary>
   InTheirFiles
}
