using System.Linq;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.LanguageServices.Parsing.Tokenizing.PreProcessingStage;

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
