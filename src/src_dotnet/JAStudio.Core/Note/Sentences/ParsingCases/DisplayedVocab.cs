using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Sentences.ParsingCases.Breakdowns;

namespace JAStudio.Core.Note.Sentences.ParsingCases;

/// <summary>
/// The vocab a sentence's breakdown displays with every option off: for each word it displays, in order, where the word starts and the vocab it's<br/>
/// matched to. What a person sees of the sentence with nothing expanded and nothing hidden, and the part of a parsing case's result that a<br/>
/// person gives a <see cref="Verdict"/> on (<see cref="ParsingCase.VerdictOnTheDisplayedVocab"/>).
/// </summary>
/// <remarks>
/// It leaves out what the rest of a <see cref="ParsingCaseResult"/> holds: the words parsed but not displayed, the breakdowns with options on,<br/>
/// and whether a word is matched by its form in the sentence or by its dictionary form.
/// </remarks>
public sealed partial record DisplayedVocab(IReadOnlyList<DisplayedVocab.Word> Words)
{
   /// <summary>The displayed vocab of <paramref name="breakdownWithEveryOptionOff"/>.</summary>
   public static DisplayedVocab Of(BreakdownUnderOptions breakdownWithEveryOptionOff) => new([.. breakdownWithEveryOptionOff.DisplayedWords.Select(Word.Of)]);

   /// <summary>Whether <paramref name="other"/> displays the same vocab at the same places, in the same order.</summary>
   public bool IsTheSameAs(DisplayedVocab other) => Words.SequenceEqual(other.Words);
}
