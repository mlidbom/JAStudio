using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using JAStudio.Anki;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core;
using JAStudio.Core.Anki;
using JAStudio.Core.Anki.StudyNotes;
using JAStudio.Core.Configuration;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Storage;
using JAStudio.Core.TaskRunners;

namespace JAStudio.HeadlessHost;

/// <summary>
/// Wires JAStudio for running without Anki or a UI: the notes come from a corpus checkout, as inside Anki, and the studying statuses from an<br/>
/// Anki profile's collection file, read while Anki may be running. Whatever needs Anki itself, such as creating a note's Anki note, refuses.
/// </summary>
class HeadlessEnvironmentDependenciesRegistrar : IEnvironmentSpecificDependenciesRegistrar
{
   readonly HeadlessJAStudioSetup _setup;
   readonly HeadlessJAStudioUIThreadDispatcher _uiThreadDispatcher;

   public HeadlessEnvironmentDependenciesRegistrar(HeadlessJAStudioSetup setup, HeadlessJAStudioUIThreadDispatcher uiThreadDispatcher)
   {
      _setup = setup;
      _uiThreadDispatcher = uiThreadDispatcher;
   }

   public void WireEnvironmentSpecificServices(IComponentRegistrar registrar) =>
      registrar.Register(
         Singleton.For<IEnvironmentPaths>().Instance(new HeadlessEnvironmentPaths(_setup)),
         Singleton.For<INoteRepository>().CreatedBy(INoteRepository (NoteSerializer serializer, IEnvironmentPaths paths) => new FileSystemNoteRepository(serializer, paths)),
         Singleton.For<IBackendNoteCreator>().Instance(new HeadlessBackendNoteCreator()),
         Singleton.For<AnkiCollectionDatabase>().Instance(new AnkiCollectionFile(_setup.AnkiCollectionFile)),
         Singleton.For<IAnkiRawNotes>().CreatedBy(IAnkiRawNotes (AnkiCollectionDatabase database) => new HeadlessAnkiRawNotes(database)),
         Singleton.For<IBackendDataLoader>().CreatedBy(IBackendDataLoader (AnkiCollectionDatabase database) => new AnkiBackendDataLoader(database)),
         Singleton.For<IFatalErrorHandler>().Instance(new HeadlessFatalErrorHandler()),
         Singleton.For<IAnkiWindows>().Instance(new NoAnkiWindows()),
         Singleton.For<IUIThreadDispatcher>().Instance(_uiThreadDispatcher),
         Singleton.For<IConfigDictSource>().Instance(new HeadlessConfigDictSource()),
         Singleton.For<IReadingsMappingsSource>().CreatedBy((IEnvironmentPaths paths) => new FileReadingsMappingsSource(paths)),
         Singleton.For<ICardOperations>().Instance(new HeadlessCardOperations()),
         Singleton.For<IAnkiTagOperations>().Instance(new HeadlessAnkiTagOperations()),
         Singleton.For<IAnkiStudyNotes>().CreatedBy(IAnkiStudyNotes (AnkiCollectionDatabase database) => new HeadlessAnkiStudyNotes(database))
      );
}
