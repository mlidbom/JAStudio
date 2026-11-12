namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// The way from a subject to another that a condition can be on, such as from a word to its preceding token (<see cref="ConditionSubject{TFrom,TSubject}"/>),<br/>
/// or on from there to the token's surface (<see cref="ConditionPath{TFrom,TSubject}"/>): a condition on where it leads is placed through it to be a<br/>
/// condition on where it starts. It's what lets C# build a condition as its code reads, <c>PrecedingToken.Surface.EndsWithOneOf("ん")</c>: see<br/>
/// <see cref="Tokens.TokenConditionsInCode"/> and <see cref="Texts.TextConditionsInCode"/>.
/// </summary>
public interface IConditionPath<TFrom, TSubject> where TFrom : class where TSubject : class
{
   //review:matching: "path", for the way from a word to its preceding token's surface, and "Placed", for the condition on where it leads
   //   made a condition on where it starts. A ConditionSubject is the one-step path.
   /// <summary>The condition on where the path starts that what it leads to meets <paramref name="condition"/>.</summary>
   Condition<TFrom> Placed(Condition<TSubject> condition);
}
