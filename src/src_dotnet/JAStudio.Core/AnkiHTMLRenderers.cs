using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using JAStudio.Core.UI.Web.Kanji;
using JAStudio.Core.UI.Web.Sentence;
using JAStudio.Core.UI.Web.Vocab;

namespace JAStudio.Core;

public class AnkiHTMLRenderers
{
   readonly IRootResolver _resolver;
   internal AnkiHTMLRenderers(IRootResolver resolver) => _resolver = resolver;

   // ReSharper disable once UnusedMember.Global used from python
   public VocabNoteRenderer VocabNoteRenderer => _resolver.Resolve<VocabNoteRenderer>();
   // ReSharper disable once UnusedMember.Global used from python
   public SentenceNoteRenderer SentenceNoteRenderer => _resolver.Resolve<SentenceNoteRenderer>();
   // ReSharper disable once UnusedMember.Global used from python
   public KanjiNoteRenderer KanjiNoteRenderer => _resolver.Resolve<KanjiNoteRenderer>();
}
