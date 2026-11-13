using System;
using Compze.Contracts;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Microsoft;
using Compze.DependencyInjection.Microsoft.Extensions.Hosting;
using JAStudio.Core.Batches;
using JAStudio.Core.Configuration;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.LanguageServices.Parsing;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
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
   /// <summary>An unbuilt Compze container builder pre-populated with all JAStudio service registrations.
   /// Feed it to <c>new MicrosoftServiceProviderFactory(plan.Builder)</c> and plug into <c>IHostBuilder.UseServiceProviderFactory(...)</c>;
   /// the host's Build() call drives Compze's Build() exactly once.</summary>
   public readonly record struct BootstrapPlan(MicrosoftContainerBuilder Builder);

   public static BootstrapPlan PrepareProduction(IEnvironmentSpecificDependenciesRegistrar deps) => Prepare(deps);

   public static BootstrapPlan PrepareForTests()
   {
      Contract.State.Assert(CoreApp.IsTesting);
      return Prepare(new TestEnvironmentSpecificDependenciesRegistrar());
   }

   static BootstrapPlan Prepare(IEnvironmentSpecificDependenciesRegistrar environmentSpecificDependenciesRegistrar)
   {
      var builder = new MicrosoftContainerBuilder();
      var registrar = builder.Registrar;

      environmentSpecificDependenciesRegistrar.WireEnvironmentSpecificServices(registrar);

      registrar.Register(
         Singleton.For<AnkiHTMLRenderers>().CreatedBy((IRootResolver resolver) => new AnkiHTMLRenderers(resolver)),
         Singleton.For<CoreApp>().CreatedBy((TemporaryServiceCollection services, IEnvironmentPaths paths) => new CoreApp(services, paths)),
         Singleton.For<ConfigurationStore>().CreatedBy((IReadingsMappingsSource readingsMappingsSource, IConfigDictSource configDictSource) => new ConfigurationStore(readingsMappingsSource, configDictSource)),
         Singleton.For<TemporaryServiceCollection>().CreatedBy((IRootResolver resolver) => new TemporaryServiceCollection(resolver)),
         Singleton.For<JapaneseConfig>().CreatedBy((ConfigurationStore store) => store.Config()),
         Singleton.For<JPCollection>().CreatedBy((IBackendNoteCreator backendNoteCreator, NoteServices noteServices, INoteRepository noteRepository, MediaFileIndex mediaFileIndex, MediaStorageService mediaStorage, IBackendDataLoader backendDataLoader) =>
                                                    new JPCollection(backendNoteCreator, noteServices, noteRepository, mediaFileIndex, mediaStorage, backendDataLoader)),
         Singleton.For<AnkiTagChanges>().CreatedBy((JPCollection collection, IAnkiTagOperations anki) => new AnkiTagChanges(collection, anki)),
         Singleton.For<VocabCollection>().CreatedBy((JPCollection col) => col.Vocab),
         Singleton.For<KanjiCollection>().CreatedBy((JPCollection col) => col.Kanji),
         Singleton.For<SentenceCollection>().CreatedBy((JPCollection col) => col.Sentences),

         // Media storage
         Singleton.For<MediaFileIndex>().CreatedBy((IEnvironmentPaths paths, TaskRunner taskRunner) => new MediaFileIndex(paths, taskRunner)),
         Singleton.For<MediaStorageService>().CreatedBy((IEnvironmentPaths paths, MediaFileIndex index) => new MediaStorageService(paths, index)),

         // Importing
         Singleton.For<SavedImportProfiles>().CreatedBy((IEnvironmentPaths paths) => new SavedImportProfiles(paths)),
         Singleton.For<Importer>().CreatedBy((IAnkiRawNotes ankiRawNotes, NoteServices noteServices, MediaFileIndex mediaFileIndex, MediaStorageService mediaStorage, IEnvironmentPaths paths, TaskRunner taskRunner) =>
                                                new Importer(ankiRawNotes, noteServices, mediaFileIndex, mediaStorage, paths, taskRunner)),
         Singleton.For<AutomaticImportRuns>().CreatedBy((SavedImportProfiles savedProfiles, Importer importer, BackgroundTaskManager backgroundTaskManager) =>
                                                           new AutomaticImportRuns(savedProfiles, importer, backgroundTaskManager)),

         // Matching
         Singleton.For<SharedConditions>().CreatedBy((IEnvironmentPaths paths, IRootResolver resolver) => new SharedConditions(paths, new Lazy<VocabCollection>(resolver.Resolve<VocabCollection>))),

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
         Singleton.For<NoteServices>().CreatedBy((IRootResolver resolver) => new NoteServices(resolver)),
         Singleton.For<DictLookup>().CreatedBy((JPCollection col) => col.DictLookup),
         Singleton.For<VocabNoteFactory>().CreatedBy((JPCollection col) => col.VocabNoteFactory),
         Singleton.For<VocabNoteGeneratedData>().CreatedBy((JPCollection col) => col.VocabNoteGeneratedData),
         Singleton.For<NoteSerializer>().CreatedBy((NoteServices noteServices) => new NoteSerializer(noteServices)),
         Singleton.For<FileSystemNoteRepository>().CreatedBy((NoteSerializer serializer, IEnvironmentPaths paths) =>
                                                                new FileSystemNoteRepository(serializer, paths)),
         Singleton.For<KanjiNoteMnemonicMaker>().CreatedBy((JapaneseConfig config) => new KanjiNoteMnemonicMaker(config)),

         // ViewModels
         Singleton.For<SentenceKanjiListViewModel>().CreatedBy((KanjiCollection kanji) => new SentenceKanjiListViewModel(kanji)),

         // Note renderers
         Singleton.For<VocabNoteRenderer>().CreatedBy(() => new VocabNoteRenderer()),
         Singleton.For<SentenceNoteRenderer>().CreatedBy(() => new SentenceNoteRenderer()),
         Singleton.For<KanjiNoteRenderer>().CreatedBy(() => new KanjiNoteRenderer())
      );

      // Allow ASP.NET Core / Blazor hosts inside this process (CardServer) to spin up a
      // child container whose singletons delegate back to the parent.
      MicrosoftChildContainerHostIntegration.RegisterWith(registrar);

      return new BootstrapPlan(builder);
   }
}
