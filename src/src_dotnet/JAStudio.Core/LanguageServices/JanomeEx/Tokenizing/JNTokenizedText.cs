using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.LanguageServices.TokenizerDictionaries;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

/// <summary>A text as the tokenizer cut it into tokens, with the tokenizer dictionary it did so with.</summary>
public class JNTokenizedText
{
   public string Text { get; }

   /// <summary>The tokens the tokenizer cut <see cref="Text"/> into, as it gave them, linked as neighbours.</summary>
   public List<JNToken> Tokens { get; }

   /// <summary>The tokenizer dictionary the text was tokenized with, which the pre-processing stage asks about the words it may correct or split.</summary>
   public TokenizerDictionary Dictionary { get; }

   public JNTokenizedText(string text, List<JNToken> tokens, TokenizerDictionary dictionary)
   {
      Text = text;
      Tokens = tokens;
      Dictionary = dictionary;
   }

   /// <summary>The tokens JAStudio's analysis works with, made from <see cref="Tokens"/> by the pre-processing stage, which leaves <see cref="Tokens"/> as they are.</summary>
   public List<IAnalysisToken> PreProcess(VocabCollection vocab, DictLookup dictLookup) =>
      new PreProcessingStage.PreProcessingStage(vocab, dictLookup, Dictionary).PreProcess(Tokens);
}
