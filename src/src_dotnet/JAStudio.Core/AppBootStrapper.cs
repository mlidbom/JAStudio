using System;
using Compze.Contracts;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Microsoft;
using Compze.DependencyInjection.Microsoft.Extensions.Hosting;
using Compze.Tessaging.Engine.Wiring;
using Compze.Tessaging.TessageBus;
using Compze.Teventive.StateStore;
using Compze.Teventive.StateStore.Storage;
using JAStudio.Core.Batches;
using JAStudio.Core.Configuration;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.RelatedVocab;
using JAStudio.Core.Storage;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.Studying;
using JAStudio.Core.Studying.StudyNotes;
using JAStudio.Core.TaskRunners;
using JAStudio.Core.UI.Shortcuts;
using JAStudio.Core.UI.Web;
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
         Singleton.For<JPCollection>().CreatedBy((NoteServices noteServices, IRootResolver resolver, MediaFileIndex mediaFileIndex, MediaStorageService mediaStorage, ISRSStudyNotes studyNotesInTheSRS) =>
                                                    new JPCollection(noteServices, resolver, mediaFileIndex, mediaStorage, studyNotesInTheSRS)),
         Singleton.For<AnkiTagChanges>().CreatedBy((JPCollection collection, ISRSTagOperations anki) => new AnkiTagChanges(collection, anki)),
         Singleton.For<SRSSync>().CreatedBy((JPCollection collection, ISRSStudyNotes studyNotes, SRSAvailability srsAvailability, AutomaticImportRuns automaticImportRuns, TaskRunner taskRunner) =>
                                               new SRSSync(collection, studyNotes, srsAvailability, automaticImportRuns, taskRunner)),
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
         Singleton.For<StudyNoteIds>().CreatedBy(() => new StudyNoteIds()),
         Singleton.For<WhatTheSRSHoldsOfTheStudyNotes>().CreatedBy(() => new WhatTheSRSHoldsOfTheStudyNotes()),
         Singleton.For<LocalNoteUpdater>().CreatedBy((TaskRunner taskRunner, JPCollection collection, JapaneseConfig config, DictLookup dictLookup, VocabNoteFactory vocabNoteFactory) =>
                                                        new LocalNoteUpdater(taskRunner, collection, config, dictLookup, vocabNoteFactory)),
         Singleton.For<TaskRunner>().CreatedBy((DialogProgressPresenter dialogPresenter) => new TaskRunner(dialogPresenter)),
         Singleton.For<DialogProgressPresenter>().CreatedBy((IUIThreadDispatcher dispatcher) => new DialogProgressPresenter(dispatcher)),
         Singleton.For<BackgroundTaskManager>().CreatedBy((IFatalErrorHandler fatalErrorHandler) => new BackgroundTaskManager(fatalErrorHandler)),

         // Services owned by JPCollection — registered as property accessors
         Singleton.For<NoteServices>().CreatedBy((IRootResolver resolver) => new NoteServices(resolver)),
         Singleton.For<DictLookup>().CreatedBy((JPCollection col) => col.DictLookup),
         Singleton.For<VocabNoteFactory>().CreatedBy((JPCollection col) => col.VocabNoteFactory),
         Singleton.For<VocabNoteGeneratedData>().CreatedBy((JPCollection col) => col.VocabNoteGeneratedData),
         Singleton.For<NoteSerializer>().CreatedBy((NoteServices noteServices) => new NoteSerializer(noteServices)),
         Singleton.For<KanjiNoteMnemonicMaker>().CreatedBy((JapaneseConfig config) => new KanjiNoteMnemonicMaker(config)),

         // What the spaced repetition system asks of JAStudio
         Singleton.For<NoteViewShortcuts>().CreatedBy((JapaneseConfig config) => new NoteViewShortcuts(config)),
         Singleton.For<HowLongToShowTheQuestion>().CreatedBy((JapaneseConfig config) => new HowLongToShowTheQuestion(config)),
         Singleton.For<TimeboxLengths>().CreatedBy((JapaneseConfig config) => new TimeboxLengths(config)),

         // ViewModels
         Singleton.For<SentenceKanjiListViewModel>().CreatedBy((KanjiCollection kanji) => new SentenceKanjiListViewModel(kanji)),

         // Note renderers
         Singleton.For<ChangesThatOutdateNotePages>().CreatedBy((JPCollection collection, JapaneseConfig config) => new ChangesThatOutdateNotePages(collection, config)),
         Singleton.For<VocabNoteRenderer>().CreatedBy(() => new VocabNoteRenderer()),
         Singleton.For<SentenceNoteRenderer>().CreatedBy(() => new SentenceNoteRenderer()),
         Singleton.For<KanjiNoteRenderer>().CreatedBy(() => new KanjiNoteRenderer())
      );

      ComposeTheStateStoreOfTheNotes(registrar);

      // Allow ASP.NET Core / Blazor hosts inside this process (CardServer) to spin up a
      // child container whose singletons delegate back to the parent.
      MicrosoftChildContainerHostIntegration.RegisterWith(registrar);

      return new BootstrapPlan(builder);
   }

   /// <summary>
   /// Composes the state store that keeps the notes: the three classes of note, which it builds empty with <see cref="NoteServices"/> to restore
   /// from their snapshots, their DTOs; the indexes the collections look notes up by; and its storage, which the environment registers, such as
   /// <see cref="CorpusNoteFiles"/>. And the engine that dispatches the notes' tevents, to the participation handlers through which a change to<br/>
   /// one note reaches others (<see cref="DeclareWhatReachesOtherNotes"/>).
   /// </summary>
   static void ComposeTheStateStoreOfTheNotes(IComponentRegistrar registrar)
   {
      registrar.StateStore(store =>
      {
         store.StoredIn((ITaggregateStorage storage) => storage)
              .Taggregate((NoteServices services) => new VocabNote(services), CorpusNoteDto.HoldTheSameState)
              .Taggregate((NoteServices services) => new KanjiNote(services), CorpusNoteDto.HoldTheSameState)
              .Taggregate((NoteServices services) => new SentenceNote(services), CorpusNoteDto.HoldTheSameState);
         VocabIndexes.DeclareIn(store);
         KanjiIndexes.DeclareIn(store);
         SentenceIndexes.DeclareIn(store);
      });
      registrar.LocalTessagingEngine(engine => engine.RegisterTessageBusHandlers(DeclareWhatReachesOtherNotes));
   }

   /// <summary>
   /// Declares the participation handlers through which a change to one note reaches other notes. Compze runs each inside the unit of work that<br/>
   /// made the change, synchronously, as the note that changed ends publishing the tevent the handler handles: what a handler changes is committed,<br/>
   /// or rolled back, with the change. A handler gets the notes it changes through the unit of work running (<see cref="NoteUnitOfWork"/>), so<br/>
   /// that they are notified once it has committed.
   /// </summary>
   static void DeclareWhatReachesOtherNotes(TessageBusHandlerRegistrar handle)
   {
      PerfectSynonyms.ShareTheAnswerThrough(handle);
      VocabNoteQuestion.RenameTheWordReferencesThrough(handle);
      VocabNoteForms.MirrorTheFormsThrough(handle);
   }
}
