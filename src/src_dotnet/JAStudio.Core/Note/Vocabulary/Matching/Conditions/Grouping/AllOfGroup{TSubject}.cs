using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

/// <summary>A group of conditions that a subject meets when it meets all of them (AND). Made by <see cref="ConditionGroup.AllOf{TSubject}"/>.</summary>
public sealed record AllOfGroup<TSubject> : ConditionGroup<TSubject> where TSubject : class
{
   internal AllOfGroup(IReadOnlyList<Condition<TSubject>> conditions) : base(conditions) {}

   public override bool IsMetBy(TSubject subject) => ConditionsThatAddSomething.All(it => it.IsMetBy(subject));

   /// <summary>The conditions in the group that <paramref name="subject"/> doesn't meet: why it doesn't meet the group, when it doesn't.</summary>
   public IEnumerable<Condition<TSubject>> ConditionsNotMetBy(TSubject subject) => ConditionsThatAddSomething.Where(it => !it.IsMetBy(subject));

   public override bool IsAnyOf => false;

   public override Condition SwitchedBetweenAllOfAndAnyOf() => new AnyOfGroup<TSubject>(Conditions);

   protected override ConditionGroup<TSubject> WithConditions(IReadOnlyList<Condition<TSubject>> conditions) => new AllOfGroup<TSubject>(conditions);

   protected override string Heading => "All of:";

   /// <summary>The key an "all of" group is stored under in the corpus.</summary>
   internal const string JsonKey = "allOf";

   internal override JsonObject ToJson() => ToJsonUnder(JsonKey);
}
