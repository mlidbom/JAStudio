namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A <see cref="ConditionSubject{TFrom,TSubject}"/> as the tree editor offers it, whatever it's found from and whatever it is: a subject a<br/>
/// condition can be on, such as a word's preceding token, with the conditions there are on it.
/// </summary>
public interface IConditionSubject
{
   /// <summary>The subject's name as the editor shows it, such as <c>PrecedingToken</c>.</summary>
   string Name { get; }

   /// <summary>The conditions there are on this subject.</summary>
   IConditionLevel Level { get; }

   /// <summary>A condition that this subject meets <paramref name="condition"/>, which must be a condition on this subject.</summary>
   Condition Meets(Condition condition);
}
