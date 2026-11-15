using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core;
using JAStudio.Core.Configuration;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Storage;
using JAStudio.Core.Studying;
using JAStudio.Core.Studying.StudyNotes;
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
         Singleton.For<AnkiCollectionDatabase>().Instance(new AnkisOwnConnection()),
         Singleton.For<ISRSDataLoader>().CreatedBy(ISRSDataLoader (AnkiCollectionDatabase database) => new AnkiDataLoader(database)),
         Singleton.For<IAnkiRawNotes>().CreatedBy(IAnkiRawNotes (AnkiCollectionDatabase database) => new AnkiRawNotes(database)),
         Singleton.For<IFatalErrorHandler>().Instance(new AvaloniaFatalErrorHandler()),
         Singleton.For<SRSAvailability>().Instance(SRSAvailability.Available),
         Singleton.For<ISRSWindows>().Instance(new AnkisOwnWindows()),
         Singleton.For<IUIThreadDispatcher>().Instance(new AvaloniaUIThreadDispatcher()),
         Singleton.For<IConfigDictSource>().CreatedBy((IEnvironmentPaths paths) => new ConfigurationFile(paths)),
         Singleton.For<IReadingsMappingsSource>().CreatedBy((IEnvironmentPaths paths) => new FileReadingsMappingsSource(paths)),
         Singleton.For<ISRSCardOperations>().CreatedBy(ISRSCardOperations (StudyNoteIds idMap) => new AnkiCardOperations(idMap)),
         Singleton.For<ISRSTagOperations>().CreatedBy(ISRSTagOperations (StudyNoteIds ankiNoteIds) => new AnkiTagOperations(ankiNoteIds)),
         Singleton.For<ISRSStudyNotes>().CreatedBy(ISRSStudyNotes (AnkiCollectionDatabase database, StudyNoteIds ankiNoteIds) => new AnkiStudyNotes(database, ankiNoteIds))
      );
   }
}
