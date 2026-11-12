using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing.Candidates;
using JAStudio.Core.SysUtils.Json;

namespace JAStudio.Mcp.Tools.Matching;

/// <summary>
/// A condition several vocab's match-conditions hold, as <see cref="SharedConditionTools"/> lists it for someone to decide whether to make it a<br/>
/// shared-condition (<see cref="SharedConditionCandidate"/>).
/// </summary>
/// <param name="Subject">The subject a shared-condition defined as it would be on, as code names it: Word, Token or Text.</param>
/// <param name="Code">The condition as code, on one line.</param>
/// <param name="Definition">The condition as JSON text, as define_a_shared_condition takes a definition.</param>
/// <param name="IsACombination">Whether it's a combination of some of the conditions of a group, rather than a condition in the vocab's trees.</param>
/// <param name="VocabHoldingIt">How many vocab's match-conditions hold it.</param>
/// <param name="Vocab">The questions, each with its disambiguation, of the first of those vocab, in ordinal order.</param>
/// <param name="SameAsTheSharedCondition">The full name of the shared-condition whose definition means the same, if there's one: then it's to be used, not defined.</param>
public sealed record ListedSharedConditionCandidate(
   string Subject,
   string Code,
   string Definition,
   bool IsACombination,
   int VocabHoldingIt,
   IReadOnlyList<string> Vocab,
   string? SameAsTheSharedCondition)
{
   /// <summary>JSON that keeps texts as they're written, such as ん, rather than escaped.</summary>
   static readonly JsonSerializerOptions JsonWithItsTextsAsWritten = new() { Encoder = FullUnicodeJsonEncoder.Instance };

   /// <summary>
   /// <paramref name="candidate"/> as listed, with the first <paramref name="maxVocabListed"/> of its vocab, and the shared-condition of<br/>
   /// <paramref name="sharedConditions"/> whose definition means the same, if there's one (<see cref="SharedConditions.WhoseDefinitionMeansTheSameAs"/>).
   /// </summary>
   internal static ListedSharedConditionCandidate Of(SharedConditionCandidate candidate, SharedConditions sharedConditions, int maxVocabListed) =>
      //review:matching: Of was the local function Listed in SharedConditionTools.FindTheConditionsVocabShare, hiding the tools' own Listed of a
      //   shared-condition. A static factory on the listed record, as SharedConditionPutToUse.Between is.
      new(candidate.Level.SubjectNameInCode,
          candidate.Code,
          candidate.DefinitionAsStored.ToJsonString(JsonWithItsTextsAsWritten),
          candidate.IsACombination,
          candidate.Vocab.Count,
          [.. candidate.Vocab.Select(it => it.Question.DisambiguationName).Order(StringComparer.Ordinal).Take(maxVocabListed)],
          sharedConditions.WhoseDefinitionMeansTheSameAs(candidate.Condition)?.FullName);
}
