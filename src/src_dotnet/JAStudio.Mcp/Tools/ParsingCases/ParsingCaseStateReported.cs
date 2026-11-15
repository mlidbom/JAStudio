using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Mcp.Tools.Editing;

namespace JAStudio.Mcp.Tools.ParsingCases;

/// <summary>
/// How a sentence that is a parsing case compares with what its case expects, as an edit or a command reports having changed it<br/>
/// (<see cref="NoteFieldValues"/>): marking a sentence as a parsing case, accepting what it parses to and its no longer being one change no<br/>
/// editable field, and an edit that changes how the sentence parses changes how it compares.
/// </summary>
/// <param name="Outcome">How what the sentence parses to now compares with what the case expects.</param>
/// <param name="Differences">How many differences there are. <see cref="ParsingCaseTools.ListTheParsingCases"/> lists them.</param>
public record ParsingCaseStateReported(ParsingCaseOutcome Outcome, int Differences)
{
   /// <summary>How <paramref name="sentence"/>, a parsing case, compares now.</summary>
   public static ParsingCaseStateReported Of(SentenceNote sentence)
   {
      var comparison = sentence.CompareWithWhatTheParsingCaseExpects();
      return new ParsingCaseStateReported(comparison.Outcome, comparison.Differences.Count);
   }
}
