namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

/// <summary>
/// The stem of a ら抜き potential the tokenizer gives as one token (<see cref="PreProcessingStage.RaNukiPotentialSplitter"/>): the 食べ of 食べれる,<br/>
/// the 寝 of 寝れない. The verb's irrealis stem, which the potential's れる follows, with the verb's base form.
/// </summary>
/// <param name="verbsInflectionType">The inflection type of the verb whose stem it is, as the tokenizer's dictionary gives the verb: 食べる's, not 食べれる's.</param>
class RaNukiPotentialStem(JNToken potential, string stem, string verb, InflectionType verbsInflectionType) : SplitTokenBase(potential, stem, verb)
{
   /// <summary>The potential's ending, which a ら抜き potential's base form adds to the verb's stem: the れる of 食べれる.</summary>
   internal const string PotentialEnding = "れる";

   public override bool IsIrrealis => true;
   public override bool IsInflectableWord => true;
   public override bool IsIchidanVerb => InflectionTypes.Ichidan.Includes(verbsInflectionType);
   public override bool IsGodanVerb => false;
   public override bool IsMasuStem => false;
   public override bool IsPastTenseStem => false;
   public override bool IsEndOfStatement => false;
}
