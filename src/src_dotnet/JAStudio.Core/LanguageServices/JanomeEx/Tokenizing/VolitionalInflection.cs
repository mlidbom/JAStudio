namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

/// <summary>
/// The う of a volitional the tokenizer gives as one token (<see cref="PreProcessingStage.VolitionalSplitter"/>), as in 書こう and だろう: the<br/>
/// auxiliary that follows the irrealis stem (<see cref="VolitionalStem"/>). It doesn't inflect, nor is it the verb it follows.
/// </summary>
class VolitionalInflection(JNToken volitional) : SplitTokenBase(volitional, U, U)
{
   internal const string U = "う";

   public override bool IsGodanVerb => false;
   public override bool IsIchidanVerb => false;
   public override bool IsIrrealis => false;
   public override bool IsMasuStem => false;
   public override bool IsPastTenseStem => false;
   public override bool IsPastTenseMarker => false;
   public override bool IsAdverb => false;
}
