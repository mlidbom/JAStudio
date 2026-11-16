namespace JAStudio.Core.Note.Sentences.ParsingCases;

/// <summary>
/// A parsing case of the corpus, compared: the sentence that is the parsing case, and how what it parses to now compares with what its case<br/>
/// expects. What a list of the parsing cases shows of each.
/// </summary>
/// <param name="Sentence">The sentence that is the parsing case.</param>
/// <param name="Comparison">How what <paramref name="Sentence"/> parsed to, when this was made, compares with what its parsing case expects.</param>
public sealed record ComparedParsingCase(SentenceNote Sentence, ParsingCaseComparison Comparison)
{
   /// <summary><paramref name="sentence"/>, a parsing case, parsed now and compared with what its case expects.</summary>
   public static ComparedParsingCase Of(SentenceNote sentence) => new(sentence, sentence.CompareWithWhatTheParsingCaseExpects());

   /// <summary>The parsing case as it was when it was compared.</summary>
   public ParsingCase Case => Comparison.Case;
}
