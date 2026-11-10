using JAStudio.Anki;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.Specifications.Anki;

/// <summary>
/// A specification that reads an Anki collection from its file (<see cref="AnkiCollectionFile"/>), as the headless JAStudio does. The test wiring<br/>
/// has a pretend Anki load the study-notes (<see cref="TestUtils.PretendAnki"/>), so the real load is made here, with the wiring's other parts.
/// </summary>
public abstract class SpecificationReadingAnAnkiCollectionFile : SpecificationStartingWithAnEmptyCollection
{
   /// <summary>What JAStudio loads of the study-notes in <paramref name="file"/> (<see cref="AnkiBackendDataLoader"/>).</summary>
   protected BackendData WhatTheCollectionLoadsOfTheStudyNotesIn(AnkiCollectionFile file) => new AnkiBackendDataLoader(file).Load(GetService<TaskRunner>());
}
