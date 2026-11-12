using System.Collections.Generic;
using System.Text.Json.Nodes;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing.Candidates;

/// <summary>
/// A condition that the match-conditions of several vocab hold, which a shared-condition could give a name: found by <see cref="SharedConditionCandidates"/>.<br/>
/// It's a condition in their trees, or a combination of some of the conditions in a group of theirs, such as two of the conditions their match-conditions<br/>
/// require.
/// </summary>
/// <param name="Condition">The condition, as the first vocab found holding it holds it: a combination as a group of the same kind with those conditions.</param>
/// <param name="Level">The conditions on its subject: what a shared-condition defined as it would be on.</param>
/// <param name="IsACombination">Whether it's a combination of some of a group's conditions, rather than a condition in a tree.</param>
/// <param name="Vocab">The vocab whose match-conditions hold it, each once.</param>
public sealed record SharedConditionCandidate(Condition Condition, IConditionLevel Level, bool IsACombination, IReadOnlyList<VocabNote> Vocab)
{
   /// <summary>The condition as the corpus stores conditions: the definition a shared-condition made of it would have, for <see cref="SharedConditions.DefineFrom"/>.</summary>
   public JsonObject DefinitionAsStored => Condition.ToJson();

   /// <summary>The condition as code, on one line.</summary>
   public string Code => ConditionCode.OnOneLine(Condition);
}
