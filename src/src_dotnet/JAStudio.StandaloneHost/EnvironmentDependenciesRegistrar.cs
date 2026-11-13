using System.IO;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
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

namespace JAStudio.StandaloneHost;

/// <summary>
/// Wires the standalone JAStudio: JAStudio as inside Anki, working on the add-on folder it was started on, with Anki entirely out of the loop. What<br/>
/// is Anki's, such as the study-notes and creating a note's Anki note, there is none of (<see cref="AnkiAvailability.Unavailable"/>).
/// </summary>
sealed class EnvironmentDependenciesRegistrar : IEnvironmentSpecificDependenciesRegistrar
{
   readonly DirectoryInfo _addOnFolder;

   public EnvironmentDependenciesRegistrar(DirectoryInfo addOnFolder) => _addOnFolder = addOnFolder;

   public void WireEnvironmentSpecificServices(IComponentRegistrar registrar) =>
      registrar.Register(
         Singleton.For<IEnvironmentPaths>().Instance(new EnvironmentPaths(_addOnFolder)),
         Singleton.For<INoteRepository>().CreatedBy(INoteRepository (NoteSerializer serializer, IEnvironmentPaths paths) => new FileSystemNoteRepository(serializer, paths)),
         Singleton.For<IFatalErrorHandler>().Instance(new AvaloniaFatalErrorHandler()),
         Singleton.For<IUIThreadDispatcher>().Instance(new AvaloniaUIThreadDispatcher()),
         Singleton.For<IConfigDictSource>().CreatedBy((IEnvironmentPaths paths) => new ConfigurationFile(paths)),
         Singleton.For<IReadingsMappingsSource>().CreatedBy((IEnvironmentPaths paths) => new FileReadingsMappingsSource(paths)),
         Singleton.For<AnkiAvailability>().Instance(AnkiAvailability.Unavailable),
         Singleton.For<IAnkiWindows>().Instance(new NoAnkiWindows()),
         Singleton.For<IBackendDataLoader>().Instance(new NoAnkiData()),
         Singleton.For<ICardOperations>().Instance(new NoAnkiCardOperations()),
         Singleton.For<IAnkiTagOperations>().Instance(new NoAnkiTagOperations()),
         Singleton.For<IAnkiStudyNotes>().Instance(new NoAnkiStudyNotes()),
         Singleton.For<IAnkiRawNotes>().Instance(new NoAnkiRawNotes())
      );
}
