using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
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
using JAStudio.UI.Utils;

namespace JAStudio.Anki.PythonInterop;

class AnkiAddonEnvironmentDependenciesRegistrar : IEnvironmentSpecificDependenciesRegistrar
{
   public void WireEnvironmentSpecificServices(IComponentRegistrar registrar)
   {
      registrar.Register(
         Singleton.For<IEnvironmentPaths>().Instance(new AnkiEnvironmentPaths()),
         Singleton.For<INoteRepository>().CreatedBy(INoteRepository (NoteSerializer serializer, IEnvironmentPaths paths) => new FileSystemNoteRepository(serializer, paths)),
         Singleton.For<IBackendNoteCreator>().Instance(new AnkiBackendNoteCreator()),
         Singleton.For<AnkiCollectionDatabase>().Instance(new AnkisOwnConnection()),
         Singleton.For<IBackendDataLoader>().CreatedBy(IBackendDataLoader (AnkiCollectionDatabase database) => new AnkiBackendDataLoader(database)),
         Singleton.For<IAnkiRawNotes>().CreatedBy(IAnkiRawNotes (AnkiCollectionDatabase database) => new AnkiRawNotes(database)),
         Singleton.For<IFatalErrorHandler>().Instance(new AvaloniaFatalErrorHandler()),
         Singleton.For<AnkiAvailability>().Instance(AnkiAvailability.Available),
         Singleton.For<IAnkiWindows>().Instance(new AnkisOwnWindows()),
         Singleton.For<IUIThreadDispatcher>().Instance(new AvaloniaUIThreadDispatcher()),
         Singleton.For<IConfigDictSource>().CreatedBy((IEnvironmentPaths paths) => new ConfigurationFile(paths)),
         Singleton.For<IReadingsMappingsSource>().CreatedBy((IEnvironmentPaths paths) => new FileReadingsMappingsSource(paths)),
         Singleton.For<ICardOperations>().CreatedBy(ICardOperations (ExternalNoteIdMap idMap) => new AnkiCardOperationsImpl(idMap)),
         Singleton.For<IAnkiTagOperations>().CreatedBy(IAnkiTagOperations (ExternalNoteIdMap ankiNoteIds) => new AnkiTagOperations(ankiNoteIds)),
         Singleton.For<IAnkiStudyNotes>().CreatedBy(IAnkiStudyNotes (AnkiCollectionDatabase database, ExternalNoteIdMap ankiNoteIds) => new AnkiStudyNotes(database, ankiNoteIds))
      );
   }
}
