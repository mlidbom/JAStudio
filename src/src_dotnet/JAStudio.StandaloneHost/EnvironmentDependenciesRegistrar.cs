using System.IO;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.Teventive.StateStore.Storage;
using JAStudio.Anki;
using JAStudio.Anki.AnkisFunctions.OverHttp;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core;
using JAStudio.Core.Configuration;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Storage;
using JAStudio.Core.Studying;
using JAStudio.Core.Studying.StudyNotes;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Utils;

namespace JAStudio.StandaloneHost;

/// <summary>
/// Wires the standalone JAStudio: JAStudio as a program of its own, working on the add-on folder it was started on, with Anki as a program beside<br/>
/// it. What is Anki's, such as the study-notes, it reaches by calling the Anki that runs over HTTP (<see cref="AnkiOverHttp"/>), with the classes<br/>
/// JAStudio inside Anki reaches it with. While no Anki runs, or Anki can't let JAStudio use its collection, it has no study-notes, and says why<br/>
/// what is Anki's can't be used (<see cref="AnkisAvailabilityOverHttp"/>).
/// </summary>
sealed class EnvironmentDependenciesRegistrar : IEnvironmentSpecificDependenciesRegistrar
{
   readonly DirectoryInfo _addOnFolder;

   public EnvironmentDependenciesRegistrar(DirectoryInfo addOnFolder) => _addOnFolder = addOnFolder;

   public void WireEnvironmentSpecificServices(IComponentRegistrar registrar) =>
      registrar.Register(
         Singleton.For<AnkiOverHttp>().CreatedBy(() => new AnkiOverHttp(AnkisHttpServerAccessFile.OfThisUser)),
         Singleton.For<IEnvironmentPaths>().CreatedBy(IEnvironmentPaths (AnkiOverHttp anki) => new EnvironmentPaths(_addOnFolder, anki.Media)),
         Singleton.For<ITaggregateStorage>().CreatedBy(ITaggregateStorage (IEnvironmentPaths paths) => new CorpusNoteFiles(paths.DatabaseDir)),
         Singleton.For<IFatalErrorHandler>().Instance(new AvaloniaFatalErrorHandler()),
         Singleton.For<IUIThreadDispatcher>().Instance(new AvaloniaUIThreadDispatcher()),
         Singleton.For<IConfigDictSource>().CreatedBy((IEnvironmentPaths paths) => new ConfigurationFile(paths)),
         Singleton.For<IReadingsMappingsSource>().CreatedBy((IEnvironmentPaths paths) => new FileReadingsMappingsSource(paths)),
         Singleton.For<SRSAvailability>().CreatedBy(SRSAvailability (AnkiOverHttp anki) => new AnkisAvailabilityOverHttp(anki)),
         Singleton.For<AnkiCollectionDatabase>().CreatedBy(AnkiCollectionDatabase (AnkiOverHttp anki) => new AnkisOwnConnection(anki.Database)),
         Singleton.For<ISRSWindows>().CreatedBy(ISRSWindows (AnkiOverHttp anki) => new AnkisOwnWindows(anki.Windows)),
         Singleton.For<ISRSMedia>().CreatedBy(ISRSMedia (AnkiOverHttp anki) => new AnkisMediaFolder(anki.Media)),
         Singleton.For<ISRSTagOperations>().CreatedBy(ISRSTagOperations (StudyNoteIds studyNoteIds, AnkiOverHttp anki) => new AnkiTagOperations(studyNoteIds, anki.Tags)),
         Singleton.For<ISRSStudyNotes>().CreatedBy(ISRSStudyNotes (AnkiCollectionDatabase database, StudyNoteIds studyNoteIds, AnkiOverHttp anki, SRSAvailability ankisAvailability) =>
                                                      new AnkisStudyNotesWhileAnkiCanBeUsed(new AnkiStudyNotes(database, studyNoteIds, anki.Notes, anki.Windows), ankisAvailability)),
         Singleton.For<IAnkiRawNotes>().CreatedBy(IAnkiRawNotes (AnkiCollectionDatabase database, AnkiOverHttp anki) => new AnkiRawNotes(database, anki.Notes))
      );
}
