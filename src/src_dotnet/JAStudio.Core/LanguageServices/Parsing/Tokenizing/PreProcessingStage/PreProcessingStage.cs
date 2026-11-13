using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.LanguageServices.MeCab;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.LanguageServices.Parsing.Tokenizing.PreProcessingStage;

/// <summary>
/// What JAStudio makes of the tokenizer's tokens before analyzing a text: it splits them into the pieces JAStudio's analysis and vocab see, such as<br/>
/// a dictionary-form verb's stem and ending, the tokens of an ichidan verb the tokenizer's dictionary lacks included<br/>
/// (<see cref="IchidanVerbUnknownToTheTokenizerSplitter"/>). The tokenizer's tokens stay as the tokenizer gave them, and each piece is of a type of<br/>
/// its own, recording the one tokenizer's token it was split off (<see cref="AnalysisToken.SourceToken"/>), so that what the tokenizer gave and<br/>
/// what the stage made of it can be compared.
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
   public List<AnalysisToken> PreProcess(List<MeCabToken> tokens) => tokens.SelectMany(PreProcessToken).ToList();

   List<AnalysisToken> PreProcessToken(MeCabToken token)
   {
      // Note: The order here matters, it's not random. Any change will break things even should the tests be incomplete and not show it.

      if(token.Surface == MeCabToken.SplitterTokenText)
      {
         return [];
      }

      var splitIchidanVerbUnknownToTheTokenizer = IchidanVerbUnknownToTheTokenizerSplitter.TrySplit(token, _vocabs, _dictLookup);
      if(splitIchidanVerbUnknownToTheTokenizer != null)
      {
         return splitIchidanVerbUnknownToTheTokenizer;
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
