using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Matching;

/// <summary>A vocab match on which a rule's condition and today's requirement disagree.</summary>
/// <param name="SentenceNoteId">The sentence's note id.</param>
/// <param name="Sentence">The sentence's text.</param>
/// <param name="Word">The form of the candidate word matched.</param>
/// <param name="Variant"><c>surface</c> or <c>base</c>: which of the candidate word's forms the vocab matched.</param>
/// <param name="VocabId">The vocab's note id.</param>
/// <param name="VocabQuestion">The vocab's question.</param>
/// <param name="TodaysRequirementIsMet">Whether today's requirement lets the match be valid. The condition says the opposite.</param>
/// <param name="Condition">The condition the rule becomes for this vocab, as the editor will show it.</param>
public record RuleDisagreement(
   string SentenceNoteId,
   string Sentence,
   string Word,
   string Variant,
   string VocabId,
   string VocabQuestion,
   bool TodaysRequirementIsMet,
   IReadOnlyList<string> Condition);
