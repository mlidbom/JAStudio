using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing.PreProcessingStage;

static class WordInfo
{
   public static WordInfoEntry? Lookup(VocabCollection vocab, DictLookup dictLookup, string word)
   {
      var withTheForm = vocab.WithForm(word);
      if(withTheForm.Count > 0)
      {
         List<VocabNote> withItAsTheirQuestion = [.. withTheForm.Where(it => it.GetQuestion() == word)];
         return new VocabWordInfoEntry(word, withItAsTheirQuestion.Count > 0 ? withItAsTheirQuestion : withTheForm);
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
