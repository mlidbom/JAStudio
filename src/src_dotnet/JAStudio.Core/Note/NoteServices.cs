using Compze.Utilities.DependencyInjection.Abstractions;
using JAStudio.Core.Configuration;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.Note;

/// <summary>
/// Bundles all external services that notes and their composed objects need.
/// Services are resolved lazily from the container, so NoteServices can be
/// created before all its dependencies are fully wired — breaking circular
/// dependency chains.
/// </summary>
public class NoteServices
{
   readonly IServiceLocator _serviceLocator;

   internal NoteServices(IServiceLocator serviceLocator) => _serviceLocator = serviceLocator;

   public JPCollection Collection => _serviceLocator.Resolve<JPCollection>();
   public ICardOperations CardOperations => _serviceLocator.Resolve<ICardOperations>();
   public Settings Settings => _serviceLocator.Resolve<Settings>();
   public DictLookup DictLookup => _serviceLocator.Resolve<DictLookup>();
   public VocabNoteFactory VocabNoteFactory => _serviceLocator.Resolve<VocabNoteFactory>();
   public VocabNoteGeneratedData VocabNoteGeneratedData => _serviceLocator.Resolve<VocabNoteGeneratedData>();
   public KanjiNoteMnemonicMaker KanjiNoteMnemonicMaker => _serviceLocator.Resolve<KanjiNoteMnemonicMaker>();
   public JapaneseConfig Config => _serviceLocator.Resolve<JapaneseConfig>();
   public TaskRunner TaskRunner => _serviceLocator.Resolve<TaskRunner>();
   public ExternalNoteIdMap ExternalNoteIdMap => _serviceLocator.Resolve<ExternalNoteIdMap>();
   public MediaFileIndex MediaFileIndex => _serviceLocator.Resolve<MediaFileIndex>();
}
