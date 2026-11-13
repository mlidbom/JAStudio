namespace JAStudio.Core.LanguageServices.Parsing.Tokenizing;

/// <summary>
/// The stem of a genuine ichidan verb the tokenizer's dictionary lacks, before an ending that follows the verb, such as ない: the tokenizer's e-stem<br/>
/// (<see cref="IchidanVerbUnknownToTheTokenizer.EStem"/>), with the verb's base form where the tokenizer has a godan verb's. Its facts are the<br/>
/// tokenizer's token's, which are the verb's (<see cref="PreProcessingStage.IchidanVerbUnknownToTheTokenizerSplitter"/>).
/// </summary>
class StemOfAnIchidanVerbUnknownToTheTokenizer(MeCabToken eStem, string verbsBaseForm) : SplitTokenBase(eStem, eStem.Surface, verbsBaseForm)
{
   public override bool IsInflectableWord => true;
}
