namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>Makes the ways from a subject that aren't through another: <see cref="Itself{TSubject}"/>.</summary>
public static class ConditionPath
{
   /// <summary>The way from a subject to itself, such as a token's, for writing a condition on it as its code reads: <c>PartOfSpeech.Is("動詞")</c>.</summary>
   public static IConditionPath<TSubject, TSubject> Itself<TSubject>() where TSubject : class => new SubjectItself<TSubject>();

   /// <summary>The way from a subject to itself: a condition on where it leads is the condition on where it starts, as it is.</summary>
   sealed class SubjectItself<TSubject> : IConditionPath<TSubject, TSubject> where TSubject : class
   {
      public Condition<TSubject> Placed(Condition<TSubject> condition) => condition;
   }
}
