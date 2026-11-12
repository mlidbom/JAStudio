using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using JAStudio.Core.Anki.StudyNotes;
using JAStudio.Core.Batches;
using JAStudio.Core.Configuration;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.TaskRunners;
using JAStudio.Core.Storage;
using JAStudio.Core.TestUtils;

namespace JAStudio.Core;

class TestEnvironmentSpecificDependenciesRegistrar : IEnvironmentSpecificDependenciesRegistrar
{
   public void WireEnvironmentSpecificServices(IComponentRegistrar registrar)
   {
      var paths = new TestEnvironmentPaths();
      var ankiRawNotes = new InMemoryAnkiRawNotes();
      var pretendAnki = new PretendAnki();

      registrar.Register(
         Singleton.For<InMemoryAnkiRawNotes>().Instance(ankiRawNotes),
         Singleton.For<IAnkiRawNotes>().Instance(ankiRawNotes),
         Singleton.For<IEnvironmentPaths>().Instance(paths),
         Singleton.For<INoteRepository>().Instance(new InMemoryNoteRepository()),
         Singleton.For<PretendAnki>().Instance(pretendAnki),
         Singleton.For<IBackendNoteCreator>().Instance(pretendAnki),
         Singleton.For<IBackendDataLoader>().Instance(pretendAnki),
         Singleton.For<IAnkiTagOperations>().Instance(pretendAnki),
         Singleton.For<IFatalErrorHandler>().Instance(new RethrowingFatalErrorHandler()),
         Singleton.For<IUIThreadDispatcher>().Instance(new NoUIThreadDispatcher()),
         Singleton.For<IConfigDictSource>().Instance(new TestConfigDictSource()),
         Singleton.For<IReadingsMappingsSource>().Instance(new TestReadingsMappingsSource()),
         Singleton.For<ICardOperations>().Instance(new NoOpCardOperations()),
         Singleton.For<InMemoryAnkiStudyNotes>().CreatedBy((ExternalNoteIdMap ankiNoteIds) => new InMemoryAnkiStudyNotes(ankiNoteIds)),
         Singleton.For<IAnkiStudyNotes>().CreatedBy((InMemoryAnkiStudyNotes studyNotes) => (IAnkiStudyNotes)studyNotes)
      );
   }
}
