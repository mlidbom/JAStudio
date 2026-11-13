using System.Collections.Generic;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

/// <summary>
/// A token JAStudio's pre-processing stage puts in place of the tokenizer's where the tokenizer got a word wrong: the token the tokenizer gives a<br/>
/// word its dictionary knows, made for one it doesn't, such as 磨げる, the potential form of 磨ぐ, for the 磨げ and る the tokenizer cut it into<br/>
/// (<see cref="PreProcessingStage.MisTokenizedIchidanVerbUnknownToTheTokenizer"/>). Described in the tokenizer dictionary's terms, as the tokenizer's<br/>
/// own tokens are, so the rest of the pre-processing treats it as one of them; its type and <see cref="TokenizedAs"/> tell it apart.
/// </summary>
public class CorrectedToken : JNToken
{
   internal CorrectedToken(
      IReadOnlyList<JNToken> tokenizedAs,
      JNPartsOfSpeech partsOfSpeech,
      string baseForm,
      string surface,
      InflectionType inflectionType,
      InflectionForm inflectedForm,
      string reading,
      string phonetic,
      string lemma)
      : base(partsOfSpeech, baseForm, surface, inflectionType, inflectedForm, reading, phonetic, lemma) =>
      TokenizedAs = tokenizedAs;

   /// <summary>The tokenizer's tokens this one is in place of, as the tokenizer gave them: the 磨げ and る of 磨げる.</summary>
   public IReadOnlyList<JNToken> TokenizedAs { get; }

   public override bool IsTheTokenizersOwn => false;
}
