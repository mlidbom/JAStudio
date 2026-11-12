using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

/// <summary>A group of conditions that a subject meets when it meets any of them (OR). Made by <see cref="ConditionGroup.AnyOf{TSubject}"/>.</summary>
public sealed record AnyOfGroup<TSubject> : ConditionGroup<TSubject> where TSubject : class
{
   internal AnyOfGroup(IReadOnlyList<Condition<TSubject>> conditions) : base(conditions) {}

   public override bool IsMetBy(TSubject subject) => IsEmpty || ConditionsThatAddSomething.Any(it => it.IsMetBy(subject));

   public override bool IsAnyOf => true;

   public override Condition SwitchedBetweenAllOfAndAnyOf() => new AllOfGroup<TSubject>(Conditions);

   protected override ConditionGroup<TSubject> WithConditions(IReadOnlyList<Condition<TSubject>> conditions) => new AnyOfGroup<TSubject>(conditions);

   public override string Name => "AnyOf";

   /// <summary>The key an "any of" group is stored under in the corpus.</summary>
   internal const string JsonKey = "anyOf";

   internal override JsonObject ToJson() => ToJsonUnder(JsonKey);
}
