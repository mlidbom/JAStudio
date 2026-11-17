using JAStudio.Core.Specifications.Fixtures;
using JAStudio.Core.TestUtils;

namespace JAStudio.Core.Specifications;

public abstract class SpecificationStartingWithAnEmptyCollection(NotesKept notesKept = NotesKept.InMemory) : SpecificationUsingACollection(DataNeeded.None, notesKept);
