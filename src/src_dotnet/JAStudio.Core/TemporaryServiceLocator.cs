using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using JAStudio.Core.Batches;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core;

//TODO: We should redesign so that we have a sane dependency graph and just use normal dependency injection, but first we need to get rid of all the static classes and this will help us do that
public class TemporaryServiceCollection
{
   public static TemporaryServiceCollection Instance { get; set; } = null!;

   internal TemporaryServiceCollection(IRootResolver services) => Services = services;

   public IRootResolver Services { get; }

   public T Resolve<T>() where T : class => Services.Resolve<T>();

   public CoreApp CoreApp => Services.Resolve<CoreApp>();
   public ConfigurationStore ConfigurationStore => Services.Resolve<ConfigurationStore>();

   // Core services
   public LocalNoteUpdater LocalNoteUpdater => Services.Resolve<LocalNoteUpdater>();
   public TaskRunner TaskRunner => Services.Resolve<TaskRunner>();
   public BackgroundTaskManager BackgroundTaskManager => Services.Resolve<BackgroundTaskManager>();
   public ExternalNoteIdMap ExternalNoteIdMap => Services.Resolve<ExternalNoteIdMap>();

   // Note services
   public NoteServices NoteServices => Services.Resolve<NoteServices>();
   public VocabNoteFactory VocabNoteFactory => Services.Resolve<VocabNoteFactory>();

   // ReSharper disable once UnusedMember.Global used from python
   public AnkiHTMLRenderers AnkiHTMLRenderers => Services.Resolve<AnkiHTMLRenderers>();
}
