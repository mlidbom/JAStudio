using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.Tessaging.TessageBus;
using Compze.Tessaging.TessageTypes;
using Compze.Teventive.MemoryImageStateStore;
using JAStudio.Core.Configuration;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Storage;
using JAStudio.Core.Studying;
using JAStudio.Core.Studying.StudyNotes;
using JAStudio.Core.TaskRunners;
using JAStudio.Core.TestUtils;

namespace JAStudio.Core;

/// <summary>The specifications' environment, with its notes kept as <see cref="NotesKept"/> says.</summary>
class TestEnvironmentSpecificDependenciesRegistrar(NotesKept notesKept) : IEnvironmentSpecificDependenciesRegistrar
{
   readonly TeventsObserved _teventsObserved = new();
   readonly NotesKept _notesKept = notesKept;

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
         Singleton.For<PretendAnki>().Instance(pretendAnki),
         Singleton.For<ISRSStudyNotes>().Instance(pretendAnki),
         Singleton.For<ISRSTagOperations>().Instance(pretendAnki),
         Singleton.For<ISRSMedia>().Instance(pretendAnki),
         Singleton.For<RethrowingFatalErrorHandler>().Instance(fatalErrorHandler),
         Singleton.For<IFatalErrorHandler>().Instance(fatalErrorHandler),
         Singleton.For<SRSAvailability>().Instance(pretendAnki.Availability),
         Singleton.For<ISRSWindows>().Instance(new NoSRSWindows()),
         Singleton.For<IUIThreadDispatcher>().Instance(new NoUIThreadDispatcher()),
         Singleton.For<IConfigDictSource>().CreatedBy((IEnvironmentPaths environmentPaths) => new ConfigurationFile(environmentPaths)),
         Singleton.For<IReadingsMappingsSource>().Instance(new TestReadingsMappingsSource()),
         Singleton.For<TeventsObserved>().Instance(_teventsObserved)
      );
   }

   public void DeclareWhereTheNotesAreKept(MemoryImageStateStoreBuilder store)
   {
      if(_notesKept == NotesKept.InMemory) CorpusNoteFiles.DeclareOnAFileSystemInMemoryIn(store);
      else CorpusNoteFiles.DeclareIn(store);
   }

   public void ObserveTevents(TeventObservationRegistrar observe) => observe.ForTevent<ITevent>((tevent, _) => _teventsObserved.Add(tevent));
}
