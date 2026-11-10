namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

/// <summary>Makes the conditions that combine others: <see cref="AllOf{TSubject}"/>, <see cref="AnyOf{TSubject}"/> and <see cref="Not{TSubject}"/>.</summary>
public static class ConditionGroup
{
   /// <summary>A condition that a subject meets when it meets all of <paramref name="conditions"/>.</summary>
   public static AllOfGroup<TSubject> AllOf<TSubject>(params Condition<TSubject>[] conditions) where TSubject : class => new([.. conditions]);

   /// <summary>A condition that a subject meets when it meets any of <paramref name="conditions"/>.</summary>
   public static AnyOfGroup<TSubject> AnyOf<TSubject>(params Condition<TSubject>[] conditions) where TSubject : class => new([.. conditions]);

   /// <summary>A condition that a subject meets when it doesn't meet <paramref name="condition"/>.</summary>
   public static NegatedCondition<TSubject> Not<TSubject>(Condition<TSubject> condition) where TSubject : class => new(condition);
}
