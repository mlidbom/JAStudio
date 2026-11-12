using System;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A way from a subject to another through more than one <see cref="ConditionSubject{TFrom,TSubject}"/>, such as from a word through its preceding token<br/>
/// to the token's surface, or the way from a subject to itself, which places a condition as it is. See <see cref="IConditionPath{TFrom,TSubject}"/>.
/// </summary>
public sealed class ConditionPath<TFrom, TSubject> : IConditionPath<TFrom, TSubject> where TFrom : class where TSubject : class
{
   readonly Func<Condition<TSubject>, Condition<TFrom>> _placed;

   ConditionPath(Func<Condition<TSubject>, Condition<TFrom>> placed) => _placed = placed;

   /// <summary>The way that goes along <paramref name="path"/>, then on to <paramref name="subject"/>, such as from a word's preceding token on to the token's surface.</summary>
   internal static ConditionPath<TFrom, TSubject> Through<TBetween>(IConditionPath<TFrom, TBetween> path, ConditionSubject<TBetween, TSubject> subject) where TBetween : class =>
      new(condition => path.Placed(subject.Meets(condition)));

   public Condition<TFrom> Placed(Condition<TSubject> condition) => _placed(condition);
}
