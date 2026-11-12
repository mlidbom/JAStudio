using System;
using System.Collections.Concurrent;
using System.IO;
using MeCab;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

/// <summary>
/// The <see cref="MeCabTagger"/>s that <see cref="JNTokenizer"/> parses text with. Each parse gets a tagger no other parse uses while it runs,<br/>
/// so parses on many threads at once never wait for each other.
/// </summary>
/// <remarks>
/// MeCab.DotNet doesn't say that a <see cref="MeCabTagger"/> may parse on several threads at once. One tagger shared under a lock made parsing<br/>
/// on many threads spend most of its time waiting for the lock.<br/>
/// A pool, not a tagger per thread: the thread pool adds threads while its threads block and retires idle ones, so taggers per thread would<br/>
/// follow the thread pool's behavior. The pool holds as many taggers as the most parses that have run at the same moment.
/// </remarks>
/// <remarks>
/// A tagger is cheap to hold: it memory-maps its dictionary's files, and every mapping of a file shares the operating system's pages of it,<br/>
/// so each tagger adds about 256 KB, its table of character categories.
/// </remarks>
sealed class MeCabTaggerPool
{
   /// <summary>
   /// The folder of MeCab's dictionary, which is copied next to this assembly. MeCab looks for it in <c>dic</c> under the current directory by<br/>
   /// default, which isn't JAStudio's folder when Anki hosts it.
   /// </summary>
   static readonly string DictionaryFolder = Path.Combine(Path.GetDirectoryName(typeof(MeCabTaggerPool).Assembly.Location)
                                                          ?? throw new InvalidOperationException("Cannot determine the assembly's folder"),
                                                          "dic");

   readonly ConcurrentBag<MeCabTagger> _idleTaggers = [];

   /// <summary>
   /// Lends <paramref name="parse"/> a tagger that no other parse uses until <paramref name="parse"/> returns: an idle one if there is one,<br/>
   /// otherwise a new one.
   /// </summary>
   public TResult LendATaggerTo<TResult>(Func<MeCabTagger, TResult> parse)
   {
      var tagger = _idleTaggers.TryTake(out var idleTagger) ? idleTagger : CreateTagger();
      try
      {
         return parse(tagger);
      }
      finally
      {
         _idleTaggers.Add(tagger);
      }
   }

   static MeCabTagger CreateTagger() => MeCabTagger.Create(new MeCabParam { DicDir = DictionaryFolder });
}
