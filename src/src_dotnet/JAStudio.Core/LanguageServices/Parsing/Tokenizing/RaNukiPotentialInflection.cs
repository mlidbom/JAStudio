namespace JAStudio.Core.LanguageServices.Parsing.Tokenizing;

/// <summary>
/// The potential れる of a ら抜き potential the tokenizer gives as one token, in any form but the dictionary form (<see cref="PreProcessingStage.RaNukiPotentialSplitter"/>):<br/>
/// the れ of 寝れない, the れ of 食べれます. It inflects as an ichidan verb, in the form the whole token has.
/// </summary>
class RaNukiPotentialInflection(MeCabToken potential, string surface) : SplitTokenBase(potential, surface, RaNukiPotentialStem.PotentialEnding)
{
   public override bool IsInflectableWord => true;
}
