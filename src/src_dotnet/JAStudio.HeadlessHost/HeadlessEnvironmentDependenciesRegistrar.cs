using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using JAStudio.Anki;
using JAStudio.Core;
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
   readonly HeadlessHostArguments _arguments;

   public HeadlessEnvironmentDependenciesRegistrar(HeadlessHostArguments arguments) => _arguments = arguments;

   public void WireEnvironmentSpecificServices(IComponentRegistrar registrar) =>
      registrar.Register(
         Singleton.For<IEnvironmentPaths>().Instance(new HeadlessEnvironmentPaths(_arguments)),
         Singleton.For<INoteRepository>().CreatedBy(INoteRepository (NoteSerializer serializer, IEnvironmentPaths paths) => new FileSystemNoteRepository(serializer, paths)),
         Singleton.For<IBackendNoteCreator>().Instance(new HeadlessBackendNoteCreator()),
         Singleton.For<IAnkiRawNotes>().Instance(new HeadlessAnkiRawNotes(_arguments.AnkiCollectionFile)),
         Singleton.For<IBackendDataLoader>().Instance(AnkiBackendDataLoader.ForTheCollectionIn(_arguments.AnkiCollectionFile)),
         Singleton.For<IFatalErrorHandler>().Instance(new HeadlessFatalErrorHandler()),
         Singleton.For<IUIThreadDispatcher>().Instance(new HeadlessUIThreadDispatcher()),
         Singleton.For<IConfigDictSource>().Instance(new AddonConfigFile(_arguments.AddonFolder)),
         Singleton.For<IReadingsMappingsSource>().CreatedBy((IEnvironmentPaths paths) => new FileReadingsMappingsSource(paths)),
         Singleton.For<ICardOperations>().Instance(new HeadlessCardOperations()),
         Singleton.For<IAnkiTagOperations>().Instance(new HeadlessAnkiTagOperations()),
         Singleton.For<IAnkiStudyNotes>().Instance(new HeadlessAnkiStudyNotes(_arguments.AnkiCollectionFile))
      );
}
