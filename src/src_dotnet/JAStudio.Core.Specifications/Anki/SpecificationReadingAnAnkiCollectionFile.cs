using System.Collections.Generic;
using JAStudio.Anki;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.Core.Specifications.Anki;

/// <summary>
/// A specification that reads an Anki collection from its file (<see cref="AnkiCollectionFile"/>), as the headless JAStudio does. The test wiring<br/>
/// has JAStudio read the study-notes from a pretend Anki (<see cref="TestUtils.PretendAnki"/>), so the real read is made here, with the wiring's<br/>
/// other parts.
/// </summary>
public abstract class SpecificationReadingAnAnkiCollectionFile : SpecificationStartingWithAnEmptyCollection
{
   /// <summary>The study-notes in <paramref name="file"/>, as JAStudio reads them (<see cref="StudyNotesInAnkisDatabase"/>).</summary>
   protected static IReadOnlyList<StudyNoteInTheSRS> TheStudyNotesIn(AnkiCollectionFile file) => StudyNotesInAnkisDatabase.ReadAll(file);
}
