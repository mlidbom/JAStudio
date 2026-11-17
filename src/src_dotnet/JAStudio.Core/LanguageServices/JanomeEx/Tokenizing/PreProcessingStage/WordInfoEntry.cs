using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing.PreProcessingStage;

abstract class WordInfoEntry
{
   public string Word { get; }
   public FrozenSet<PartOfSpeech> PartsOfSpeech { get; }

   protected WordInfoEntry(string word, FrozenSet<PartOfSpeech> partsOfSpeech)
   {
      Word = word;
      PartsOfSpeech = partsOfSpeech;
   }

   public bool IsIchidan => PartsOfSpeech.Contains(PartOfSpeech.IchidanVerb);
   public bool IsGodan => PartsOfSpeech.Contains(PartOfSpeech.GodanVerb);

   /// <summary>
   /// Whether the word can be intransitive: one of the words it may be is. Then a <c>を</c> before it is unlikely to mark its object, so the<br/>
   /// splitting of an ichidan verb into a godan verb's potential or imperative takes the <c>を</c> as a sign of the ichidan verb<br/>
   /// (<see cref="IchidanGodanPotentialOrImperativeHybridSplitter"/>).
   /// </summary>
   public abstract bool CanBeIntransitive { get; }

   public abstract string Answer { get; }
}

/// <summary>
/// A word looked up among the vocab: those whose question it is, or when none is, those it is a form of (<see cref="WordInfo.Lookup"/>).<br/>
/// Its parts of speech and its answer are the first one's, in the order of their ids. Whether it <see cref="CanBeIntransitive"/> is any one's, as the<br/>
/// dictionary's is any entry's, so that it doesn't depend on their order.
/// </summary>
class VocabWordInfoEntry : WordInfoEntry
{
   readonly IReadOnlyList<VocabNote> _lookedUpAmong;

   public VocabWordInfoEntry(string word, IReadOnlyList<VocabNote> lookedUpAmong)
      : base(word, lookedUpAmong[0].PartsOfSpeech.All().ToFrozenSet()) =>
      _lookedUpAmong = lookedUpAmong;

   public override bool CanBeIntransitive => _lookedUpAmong.Any(it => it.PartsOfSpeech.IsIntransitive());

   public override string Answer => _lookedUpAmong[0].GetAnswer();
}

/// <summary>A word looked up in the dictionary, with the parts of speech of all its entries.</summary>
class DictWordInfoEntry : WordInfoEntry
{
   readonly DictLookupResult _dictResult;

   public DictWordInfoEntry(string word, DictLookupResult dictResult)
      : base(word, dictResult.PartsOfSpeech().ToFrozenSet()) =>
      _dictResult = dictResult;

   public override bool CanBeIntransitive => PartsOfSpeech.Contains(PartOfSpeech.Intransitive);

   public override string Answer => _dictResult.FormatAnswer();
}
