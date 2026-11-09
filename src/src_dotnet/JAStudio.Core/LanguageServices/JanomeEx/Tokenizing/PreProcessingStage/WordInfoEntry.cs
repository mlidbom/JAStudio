using System.Collections.Frozen;
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
   public bool IsIntransitive => PartsOfSpeech.Contains(PartOfSpeech.Intransitive);

   public abstract string Answer { get; }
}

class VocabWordInfoEntry : WordInfoEntry
{
   readonly VocabNote _vocab;

   public VocabWordInfoEntry(string word, VocabNote vocab)
      : base(word, vocab.PartsOfSpeech.All().ToFrozenSet()) =>
      _vocab = vocab;

   public override string Answer => _vocab.GetAnswer();
}

class DictWordInfoEntry : WordInfoEntry
{
   readonly DictLookupResult _dictResult;

   public DictWordInfoEntry(string word, DictLookupResult dictResult)
      : base(word, dictResult.PartsOfSpeech().ToFrozenSet()) =>
      _dictResult = dictResult;

   public override string Answer => _dictResult.FormatAnswer();
}
