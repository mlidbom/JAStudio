using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Mcp.Tools.Matching;

/// <summary>
/// Vocab as the matching tools list them, such as the vocab using a shared-condition (<see cref="ListedSharedCondition.VocabUsingIt"/>): each by its<br/>
/// question with its disambiguation, such as <c>んだ:た</c>, so that two vocab with the same text are told apart, in ordinal order.
/// </summary>
static class ListedVocab
{
   /// <summary><paramref name="vocab"/> as listed: their questions, each with its disambiguation, in ordinal order.</summary>
   internal static IReadOnlyList<string> Of(IEnumerable<VocabNote> vocab) =>
      //review:matching: ListedVocab.Of was written out three times: in SharedConditionTools' listing of a shared-condition and of the vocab whose
      //   sentences redefining one reparsed, and in ListedSharedConditionCandidate.Of.
      [.. vocab.Select(it => it.Question.DisambiguationName).Order(StringComparer.Ordinal)];
}
