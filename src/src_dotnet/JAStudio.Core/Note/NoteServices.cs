using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
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
   readonly IRootResolver _resolver;

   internal NoteServices(IRootResolver resolver) => _resolver = resolver;

   public JPCollection Collection => _resolver.Resolve<JPCollection>();
   public ICardOperations CardOperations => _resolver.Resolve<ICardOperations>();
   public Settings Settings => _resolver.Resolve<Settings>();
   public DictLookup DictLookup => _resolver.Resolve<DictLookup>();
   public VocabNoteFactory VocabNoteFactory => _resolver.Resolve<VocabNoteFactory>();
   public VocabNoteGeneratedData VocabNoteGeneratedData => _resolver.Resolve<VocabNoteGeneratedData>();
   public KanjiNoteMnemonicMaker KanjiNoteMnemonicMaker => _resolver.Resolve<KanjiNoteMnemonicMaker>();
   public JapaneseConfig Config => _resolver.Resolve<JapaneseConfig>();
   public TaskRunner TaskRunner => _resolver.Resolve<TaskRunner>();
   public ExternalNoteIdMap ExternalNoteIdMap => _resolver.Resolve<ExternalNoteIdMap>();
   public MediaFileIndex MediaFileIndex => _resolver.Resolve<MediaFileIndex>();
}
