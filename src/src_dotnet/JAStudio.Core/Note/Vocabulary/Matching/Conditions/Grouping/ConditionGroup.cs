namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

/// <summary>Makes the conditions that combine others: <see cref="AllOf{TSubject}"/>, <see cref="AnyOf{TSubject}"/> and <see cref="Not{TSubject}"/>.</summary>
public static class ConditionGroup
{
   /// <summary>A condition that a subject meets when it meets all of <paramref name="conditions"/>.</summary>
   public static AllOfGroup<TSubject> AllOf<TSubject>(params Condition<TSubject>[] conditions) where TSubject : class => new([.. conditions]);

   /// <summary>A condition that a subject meets when it meets any of <paramref name="conditions"/>.</summary>
   public static AnyOfGroup<TSubject> AnyOf<TSubject>(params Condition<TSubject>[] conditions) where TSubject : class => new([.. conditions]);

   /// <summary><paramref name="condition"/> as an "all of": itself when it's one, else an "all of" holding only it, which a subject meets when it meets it.</summary>
   public static AllOfGroup<TSubject> AsAnAllOf<TSubject>(Condition<TSubject> condition) where TSubject : class => condition as AllOfGroup<TSubject> ?? AllOf(condition);

   /// <summary>A condition that a subject meets when it doesn't meet <paramref name="condition"/>.</summary>
   public static NegatedCondition<TSubject> Not<TSubject>(Condition<TSubject> condition) where TSubject : class => new(condition);
}
