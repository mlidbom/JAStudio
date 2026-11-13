using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing.PreProcessingStage;

/// <summary>
/// Splits the tokens of an ichidan verb the tokenizer's dictionary lacks, which the tokenizer cuts into a godan e-stem and an ending<br/>
/// (<see cref="IchidanVerbUnknownToTheTokenizer"/>), into the pieces the verbs the dictionary knows are split into: so 磨げ and る come out as 泳げる<br/>
/// does, 磨, げ and る. Each piece is split off one of the tokenizer's tokens, its <see cref="IAnalysisToken.SourceToken"/>, whose facts are the verb's.
/// </summary>
/// <remarks>
/// A godan verb's potential form is split as <see cref="IchidanGodanPotentialOrImperativeHybridSplitter"/> splits one the dictionary knows, and a<br/>
/// genuine ichidan verb in its dictionary form as <see cref="DictionaryFormVerbSplitter"/> does. A genuine ichidan verb in its hypothetical form, a<br/>
/// rare case, keeps the tokenizer's れ as it is: its stem's piece has the verb's base form.
/// </remarks>
static class IchidanVerbUnknownToTheTokenizerSplitter
{
   const string IchidanDictionaryFormEnding = "る";

   /// <summary>The pieces of <paramref name="token"/> if it's part of an ichidan verb unknown to the tokenizer, or null.</summary>
   public static List<IAnalysisToken>? TrySplit(JNToken token, VocabCollection vocabs, DictLookup dictLookup)
   {
      if(token.TheIchidanVerbUnknownToTheTokenizerItIsPartOf is not {} verb) return null;

      var godan = verb.TheGodanVerbWhosePotentialFormItIs(vocabs, dictLookup);
      return ReferenceEquals(token, verb.EStem) ? PiecesOfTheEStem(verb, godan) : PiecesOfItsOwnEnding(verb, godan);
   }

   /// <summary>
   /// The pieces of the verb's e-stem: for the potential form of the godan verb <paramref name="godan"/>, the godan verb's stem and the potential's<br/>
   /// ending, 磨 and げ; for a genuine ichidan verb, when <paramref name="godan"/> is null, its stem, with the verb's base form.
   /// </summary>
   static List<IAnalysisToken> PiecesOfTheEStem(IchidanVerbUnknownToTheTokenizer verb, string? godan)
   {
      var eStem = verb.EStem;
      var godanStem = eStem.Surface[..^1];
      var potentialStem = eStem.Surface[^1..];

      if(godan == null)
         return verb.IsInTheDictionaryForm ? [new IchidanDictionaryFormStem(eStem, eStem.Surface, verb.BaseForm)] : [new StemOfAnIchidanVerbUnknownToTheTokenizer(eStem, verb.BaseForm)];

      if(verb.IsInTheDictionaryForm)
         return [new GodanPotentialDictionaryFormStem(eStem, godanStem, godan), new GodanPotentialInflectionDictionaryFormStem(eStem, potentialStem, potentialStem + IchidanDictionaryFormEnding)];

      if(verb.IsInTheHypotheticalForm)
         return [new SplitToken(eStem, eStem.Surface, godan, isInflectableWord: true, isGodanPotentialStem: true)];

      return
      [
         new SplitToken(eStem, godanStem, godan, isInflectableWord: true, isGodanPotentialStem: true),
         new SplitToken(eStem, potentialStem, potentialStem + IchidanDictionaryFormEnding, isInflectableWord: true, isGodanPotentialInflection: true)
      ];
   }

   /// <summary>The piece of the verb's own ending, its dictionary form's る or its hypothetical form's れ, for a godan verb's potential or a genuine ichidan verb.</summary>
   static List<IAnalysisToken> PiecesOfItsOwnEnding(IchidanVerbUnknownToTheTokenizer verb, string? godan)
   {
      var ending = verb.Ending;
      if(verb.IsInTheDictionaryForm)
         return godan != null ? [new GodanPotentialInflectionDictionaryFormInflection(ending)] : [new IchidanDictionaryFormInflection(ending)];

      return godan != null
                ? [new SplitToken(ending, ending.Surface, ending.Surface + IchidanDictionaryFormEnding, isInflectableWord: true, isGodanPotentialInflection: true)]
                : [ending];
   }
}
