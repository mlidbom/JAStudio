using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using JAStudio.Anki.AnkisFunctions.ThroughPythonnet;
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
      var anki = new AnkiThroughPythonnet();
      var ankisAvailability = new AnkisAvailability();
      registrar.Register(
         Singleton.For<IEnvironmentPaths>().Instance(new AnkiEnvironmentPaths(anki)),
         Singleton.For<INoteRepository>().CreatedBy(INoteRepository (NoteSerializer serializer, IEnvironmentPaths paths) => new FileSystemNoteRepository(serializer, paths)),
         Singleton.For<AnkiCollectionDatabase>().Instance(new AnkisOwnConnection(anki.Database)),
         Singleton.For<IAnkiRawNotes>().CreatedBy(IAnkiRawNotes (AnkiCollectionDatabase database) => new AnkiRawNotes(database, anki.Notes)),
         Singleton.For<IFatalErrorHandler>().Instance(new AvaloniaFatalErrorHandler()),
         Singleton.For<AnkisAvailability>().Instance(ankisAvailability),
         Singleton.For<SRSAvailability>().Instance(ankisAvailability),
         Singleton.For<ISRSWindows>().Instance(new AnkisOwnWindows(anki.Windows)),
         Singleton.For<ISRSMedia>().Instance(new AnkisMediaFolder(anki.Media)),
         Singleton.For<IUIThreadDispatcher>().Instance(new AvaloniaUIThreadDispatcher()),
         Singleton.For<IConfigDictSource>().CreatedBy((IEnvironmentPaths paths) => new ConfigurationFile(paths)),
         Singleton.For<IReadingsMappingsSource>().CreatedBy((IEnvironmentPaths paths) => new FileReadingsMappingsSource(paths)),
         Singleton.For<ISRSTagOperations>().CreatedBy(ISRSTagOperations (StudyNoteIds ankiNoteIds) => new AnkiTagOperations(ankiNoteIds, anki.Tags)),
         Singleton.For<ISRSStudyNotes>().CreatedBy(ISRSStudyNotes (AnkiCollectionDatabase database, StudyNoteIds studyNoteIds) => new AnkiStudyNotes(database, studyNoteIds, anki.Notes, anki.Windows))
      );
   }
}
