using System.Linq;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing.PreProcessingStage;

static class WordInfo
{
   public static WordInfoEntry? Lookup(VocabCollection vocab, DictLookup dictLookup, string word)
   {
      var vocabEntries = vocab.WithForm(word);
      if(vocabEntries.Any())
      {
         // Try to find exact question match first
         foreach(var v in vocabEntries)
         {
            if(v.GetQuestion() == word)
            {
               return new VocabWordInfoEntry(word, v);
            }
         }

         //review: with no vocab whose question is the word, the first of those with it as a form decides, in the order of their ids: an
         //   arbitrary one. 唱う is a form of 歌う, 唄う and 詠う; 歌う comes first and is intransitive, 唄う isn't, so whether 呪文を唱える splits
         //   into 唱う and える depends on which comes first (IchidanGodanPotentialOrImperativeHybridSplitter.IsPotentialGodan). A rule over all
         //   of them, such as any of them being intransitive, would decide the same however they are ordered. (taggregates-opus, 2026-11-16)
         return new VocabWordInfoEntry(word, vocabEntries.First());
      }

      var dictLookupResult = dictLookup.LookupWord(word);
      if(dictLookupResult.FoundWords())
      {
         return new DictWordInfoEntry(word, dictLookupResult);
      }

      return null;
   }

   public static WordInfoEntry? LookupGodan(VocabCollection vocab, DictLookup dictLookup, string word)
   {
      var wordInfo = Lookup(vocab, dictLookup, word);
      return wordInfo != null && wordInfo.IsGodan ? wordInfo : null;
   }

   public static WordInfoEntry? LookupIchidan(VocabCollection vocab, DictLookup dictLookup, string word)
   {
      var wordInfo = Lookup(vocab, dictLookup, word);
      return wordInfo != null && wordInfo.IsIchidan ? wordInfo : null;
   }

   public static bool IsGodan(VocabCollection vocab, DictLookup dictLookup, string word) => LookupGodan(vocab, dictLookup, word) != null;

   public static bool IsIchidan(VocabCollection vocab, DictLookup dictLookup, string word) => LookupIchidan(vocab, dictLookup, word) != null;
}
