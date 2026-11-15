using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using JAStudio.Core.Configuration;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.TaskRunners;
using JAStudio.Core.Storage;
using JAStudio.Core.Studying;
using JAStudio.Core.Studying.StudyNotes;
using JAStudio.Core.TestUtils;

namespace JAStudio.Core;

class TestEnvironmentSpecificDependenciesRegistrar : IEnvironmentSpecificDependenciesRegistrar
{
   public void WireEnvironmentSpecificServices(IComponentRegistrar registrar)
   {
      var paths = new TestEnvironmentPaths();
      var ankiRawNotes = new InMemoryAnkiRawNotes();
      var pretendAnki = new PretendAnki();
      var fatalErrorHandler = new RethrowingFatalErrorHandler();

      registrar.Register(
         Singleton.For<InMemoryAnkiRawNotes>().Instance(ankiRawNotes),
         Singleton.For<IAnkiRawNotes>().Instance(ankiRawNotes),
         Singleton.For<IEnvironmentPaths>().Instance(paths),
         Singleton.For<INoteRepository>().Instance(new InMemoryNoteRepository()),
         Singleton.For<PretendAnki>().Instance(pretendAnki),
         Singleton.For<ISpacedRepetitionSystemDataLoader>().Instance(pretendAnki),
         Singleton.For<ISpacedRepetitionSystemTagOperations>().Instance(pretendAnki),
         Singleton.For<RethrowingFatalErrorHandler>().Instance(fatalErrorHandler),
         Singleton.For<IFatalErrorHandler>().Instance(fatalErrorHandler),
         Singleton.For<SpacedRepetitionSystemAvailability>().Instance(SpacedRepetitionSystemAvailability.Available),
         Singleton.For<ISpacedRepetitionSystemWindows>().Instance(new NoSpacedRepetitionSystemWindows()),
         Singleton.For<IUIThreadDispatcher>().Instance(new NoUIThreadDispatcher()),
         Singleton.For<IConfigDictSource>().CreatedBy((IEnvironmentPaths environmentPaths) => new ConfigurationFile(environmentPaths)),
         Singleton.For<IReadingsMappingsSource>().Instance(new TestReadingsMappingsSource()),
         Singleton.For<ISpacedRepetitionSystemCardOperations>().Instance(new NoOpCardOperations()),
         Singleton.For<InMemoryAnkiStudyNotes>().CreatedBy((StudyNoteIds ankiNoteIds) => new InMemoryAnkiStudyNotes(ankiNoteIds)),
         Singleton.For<ISpacedRepetitionSystemStudyNotes>().CreatedBy((InMemoryAnkiStudyNotes studyNotes) => (ISpacedRepetitionSystemStudyNotes)studyNotes)
      );
   }
}
