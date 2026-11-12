namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>Makes the ways from a subject that aren't through another: <see cref="Itself{TSubject}"/>.</summary>
public static class ConditionPath
{
   /// <summary>The way from a subject to itself, such as a token's, for writing a condition on it as its code reads: <c>PartOfSpeech.Is("動詞")</c>.</summary>
   public static IConditionPath<TSubject, TSubject> Itself<TSubject>() where TSubject : class => ToItself<TSubject>.Path;

   static class ToItself<TSubject> where TSubject : class
   {
      internal static readonly IConditionPath<TSubject, TSubject> Path = new SubjectItself();

      sealed class SubjectItself : IConditionPath<TSubject, TSubject>
      {
         public Condition<TSubject> Placed(Condition<TSubject> condition) => condition;
      }
   }
}
