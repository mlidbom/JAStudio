using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A condition that a subject either meets or doesn't. The subject is a word in its sentence (<see cref="CandidateWordVariant"/>), a token<br/>
/// (<see cref="IAnalysisToken"/>), or a text. A vocab's match-condition is a condition on a word: a word can match the vocab only when it meets it.
/// </summary>
/// <remarks>
/// Conditions combine into a tree: <see cref="ConditionGroup"/> makes "all of", "any of" and "not", and a <see cref="SubjectCondition{TFrom,TSubject}"/><br/>
/// holds a tree of conditions on another subject, such as the token preceding the word. <see cref="Condition"/> is the tree's shape, for the editor.<br/>
/// Conditions are values: two built from the same parts, in the same order, are equal. See <c>dev_docs/matching-configuration.md</c>.
/// </remarks>
public abstract record Condition<TSubject> : Condition where TSubject : class
{
   /// <summary>Whether <paramref name="subject"/> meets this condition. An empty condition (<see cref="Condition.IsEmpty"/>) is met by every subject.</summary>
   public abstract bool IsMetBy(TSubject subject);

   public override Condition Negated() => ConditionGroup.Not(this);
}
