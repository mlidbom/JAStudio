using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.LanguageServices.TokenizerDictionaries;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing.PreProcessingStage;

/// <summary>
/// What JAStudio makes of the tokenizer's tokens before analyzing a text. First it corrects the tokenizer's mistakes, putting tokens of its own in<br/>
/// place of the tokenizer's (<see cref="MisTokenizedIchidanVerbUnknownToTheTokenizer"/>). Then it splits tokens into the pieces JAStudio's analysis<br/>
/// and vocab see, such as a dictionary-form verb's stem and ending. The tokenizer's tokens stay as the tokenizer gave them, and each token the stage<br/>
/// makes is of a type of its own, recording the tokenizer's token it came from (<see cref="IAnalysisToken.SourceToken"/>,<br/>
/// <see cref="CorrectedToken.TokenizedAs"/>), so that what the tokenizer gave and what the stage made of it can be compared.
/// </summary>
/// <remarks>
/// It's for what the parsing rules, the vocab's match-conditions, can't do sanely: the first tool for a word parsed wrong is the rules.
/// </remarks>
class PreProcessingStage
{
   readonly VocabCollection _vocabs;
   readonly DictLookup _dictLookup;
   readonly TokenizerDictionary _tokenizerDictionary;

   public PreProcessingStage(VocabCollection vocabs, DictLookup dictLookup, TokenizerDictionary tokenizerDictionary)
   {
      _vocabs = vocabs;
      _dictLookup = dictLookup;
      _tokenizerDictionary = tokenizerDictionary;
   }

   /// <summary>The tokens JAStudio's analysis works with, made from the tokenizer's <paramref name="tokens"/>, which stay as they are.</summary>
   public List<IAnalysisToken> PreProcess(List<JNToken> tokens) =>
      MisTokenizedIchidanVerbUnknownToTheTokenizer.CorrectEachIn(tokens).SelectMany(PreProcessToken).ToList();

   List<IAnalysisToken> PreProcessToken(JNToken token)
   {
      // Note: The order here matters, it's not random. Any change will break things even should the tests be incomplete and not show it.

      if(token.Surface == JNToken.SplitterTokenText)
      {
         return [];
      }

      var splitVolitional = VolitionalSplitter.TrySplit(token);
      if(splitVolitional != null)
      {
         return splitVolitional;
      }

      var splitRaNukiPotential = RaNukiPotentialSplitter.TrySplit(token, _tokenizerDictionary);
      if(splitRaNukiPotential != null)
      {
         return splitRaNukiPotential;
      }

      var splitGodanImperative = GodanImperativeSplitter.TrySplit(token);
      if(splitGodanImperative != null)
      {
         return splitGodanImperative;
      }

      var splitHybrid = IchidanGodanPotentialOrImperativeHybridSplitter.TrySplit(token, _vocabs, _dictLookup);
      if(splitHybrid != null)
      {
         return splitHybrid;
      }

      var splitIchidanImperative = IchidanImperativeSplitter.TrySplit(token);
      if(splitIchidanImperative != null)
      {
         return splitIchidanImperative;
      }

      var splitDictionaryFormVerb = DictionaryFormVerbSplitter.TrySplit(token);
      if(splitDictionaryFormVerb != null)
      {
         return splitDictionaryFormVerb;
      }

      return [token];
   }
}
