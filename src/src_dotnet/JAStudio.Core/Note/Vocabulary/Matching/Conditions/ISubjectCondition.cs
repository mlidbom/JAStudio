namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A <see cref="SubjectCondition{TFrom,TSubject}"/> as the match-condition editor sees it, whatever its subjects: the editor shows its subject's name in<br/>
/// front of the condition it holds, such as <c>PrecedingToken.IsAGodanVerb</c>, and offers the conditions on that subject under it.
/// </summary>
public interface ISubjectCondition
{
   /// <summary>The subject the <see cref="Condition"/> is on.</summary>
   IConditionSubject Subject { get; }

   /// <summary>The condition the <see cref="Subject"/> must meet.</summary>
   Condition Condition { get; }
}
