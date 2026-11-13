namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

/// <summary>
/// The stem of a volitional the tokenizer gives as one token (<see cref="PreProcessingStage.VolitionalSplitter"/>): the 書こ of 書こう, the だろ of<br/>
/// だろう. The verb's irrealis stem, which the volitional's う (<see cref="VolitionalInflection"/>) follows, with the verb's base form.
/// </summary>
class VolitionalStem(JNToken volitional) : SplitTokenBase(volitional, volitional.Surface[..^VolitionalInflection.U.Length], volitional.BaseForm)
{
   public override bool IsIrrealis => true;
   public override bool IsInflectableWord => SourceToken.IsInflectableWord;
   public override bool IsEndOfStatement => false;
}
