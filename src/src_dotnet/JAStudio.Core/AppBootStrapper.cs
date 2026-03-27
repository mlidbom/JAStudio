using Compze.Utilities.Contracts;
using Compze.Utilities.DependencyInjection;
using Compze.Utilities.DependencyInjection.Abstractions;
using Compze.Utilities.DependencyInjection.SimpleInjector;
using JAStudio.Core.Batches;
using JAStudio.Core.Configuration;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;
using JAStudio.Core.UI.Web.Kanji;
using JAStudio.Core.UI.Web.Sentence;
using JAStudio.Core.UI.Web.Vocab;
using JAStudio.Core.ViewModels.KanjiList;

namespace JAStudio.Core;

public static class AppBootstrapper
{
   public static CoreApp BootstrapProduction(IEnvironmentSpecificDependenciesRegistrar deps) => Bootstrap(deps);

   public static CoreApp BootstrapForTests()
   {
      Assert.State.Is(CoreApp.IsTesting);
      return Bootstrap(new TestEnvironmentSpecificDependenciesRegistrar());
   }

   static CoreApp Bootstrap(IEnvironmentSpecificDependenciesRegistrar environmentSpecificDependenciesRegistrar)
   {
      var container = new SimpleInjectorDependencyInjectionContainer();
      var registrar = container.Register();

      environmentSpecificDependenciesRegistrar.WireEnvironmentSpecificServices(registrar);

      registrar.Register(
         Singleton.For<IServiceLocator>().CreatedBy(() => container.ServiceLocator),
         Singleton.For<AnkiHTMLRenderers>().CreatedBy((IServiceLocator serviceLocator) => new AnkiHTMLRenderers(serviceLocator)),
         Singleton.For<CoreApp>().CreatedBy((TemporaryServiceCollection services, IEnvironmentPaths paths) => new CoreApp(services, paths)),
         Singleton.For<ConfigurationStore>().CreatedBy((IReadingsMappingsSource readingsMappingsSource, IConfigDictSource configDictSource) => new ConfigurationStore(readingsMappingsSource, configDictSource)),
         Singleton.For<TemporaryServiceCollection>().CreatedBy((IServiceLocator serviceLocator) => new TemporaryServiceCollection(serviceLocator)),
         Singleton.For<JapaneseConfig>().CreatedBy((ConfigurationStore store) => store.Config()),
         Singleton.For<JPCollection>().CreatedBy((IBackendNoteCreator backendNoteCreator, NoteServices noteServices, INoteRepository noteRepository, MediaFileIndex mediaFileIndex, IBackendDataLoader backendDataLoader) =>
                                                    new JPCollection(backendNoteCreator, noteServices, noteRepository, mediaFileIndex, backendDataLoader)),
         Singleton.For<VocabCollection>().CreatedBy((JPCollection col) => col.Vocab),
         Singleton.For<KanjiCollection>().CreatedBy((JPCollection col) => col.Kanji),
         Singleton.For<SentenceCollection>().CreatedBy((JPCollection col) => col.Sentences),

         // Media storage
         Singleton.For<MediaFileIndex>().CreatedBy((IEnvironmentPaths paths, TaskRunner taskRunner, BackgroundTaskManager bgTasks) => new MediaFileIndex(paths, taskRunner, bgTasks)),
         Singleton.For<MediaStorageService>().CreatedBy((IEnvironmentPaths paths, MediaFileIndex index) => new MediaStorageService(paths, index)),

         // Core services
         Singleton.For<Settings>().CreatedBy((JapaneseConfig config) => new Settings(config)),
         Singleton.For<AnalysisServices>().CreatedBy((VocabCollection vocab, DictLookup dictLookup, Settings settings) => new AnalysisServices(vocab, dictLookup, settings)),
         Singleton.For<ExternalNoteIdMap>().CreatedBy(() => new ExternalNoteIdMap()),
         Singleton.For<LocalNoteUpdater>().CreatedBy((TaskRunner taskRunner, VocabCollection vocab, KanjiCollection kanji, SentenceCollection sentences, JapaneseConfig config, DictLookup dictLookup, VocabNoteFactory vocabNoteFactory, FileSystemNoteRepository fileSystemNoteRepository) =>
                                                        new LocalNoteUpdater(taskRunner, vocab, kanji, sentences, config, dictLookup, vocabNoteFactory, fileSystemNoteRepository)),
         Singleton.For<TaskRunner>().CreatedBy((DialogProgressPresenter dialogPresenter) => new TaskRunner(dialogPresenter)),
         Singleton.For<DialogProgressPresenter>().CreatedBy((IUIThreadDispatcher dispatcher) => new DialogProgressPresenter(dispatcher)),
         Singleton.For<BackgroundTaskManager>().CreatedBy((IFatalErrorHandler fatalErrorHandler) => new BackgroundTaskManager(fatalErrorHandler)),

         // Services owned by JPCollection — registered as property accessors
         Singleton.For<NoteServices>().CreatedBy((IServiceLocator serviceLocator) => new NoteServices(serviceLocator)),
         Singleton.For<DictLookup>().CreatedBy((JPCollection col) => col.DictLookup),
         Singleton.For<VocabNoteFactory>().CreatedBy((JPCollection col) => col.VocabNoteFactory),
         Singleton.For<VocabNoteGeneratedData>().CreatedBy((JPCollection col) => col.VocabNoteGeneratedData),
         Singleton.For<NoteSerializer>().CreatedBy((NoteServices noteServices) => new NoteSerializer(noteServices)),
         Singleton.For<FileSystemNoteRepository>().CreatedBy((NoteSerializer serializer, TaskRunner taskRunner, BackgroundTaskManager bgTasks, IEnvironmentPaths paths) =>
                                                                new FileSystemNoteRepository(serializer, taskRunner, bgTasks, paths)),
         Singleton.For<KanjiNoteMnemonicMaker>().CreatedBy((JapaneseConfig config) => new KanjiNoteMnemonicMaker(config)),

         // ViewModels
         Singleton.For<SentenceKanjiListViewModel>().CreatedBy((KanjiCollection kanji) => new SentenceKanjiListViewModel(kanji)),

         // Note renderers
         Singleton.For<VocabNoteRenderer>().CreatedBy(() => new VocabNoteRenderer()),
         Singleton.For<SentenceNoteRenderer>().CreatedBy(() => new SentenceNoteRenderer()),
         Singleton.For<KanjiNoteRenderer>().CreatedBy(() => new KanjiNoteRenderer())
      );

      TemporaryServiceCollection.Instance = container.ServiceLocator.Resolve<TemporaryServiceCollection>();

      return container.ServiceLocator.Resolve<CoreApp>();
   }
}
