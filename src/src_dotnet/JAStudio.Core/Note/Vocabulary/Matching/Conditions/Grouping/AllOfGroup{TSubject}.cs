using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

/// <summary>A group of conditions that a subject meets when it meets all of them (AND). Made by <see cref="ConditionGroup.AllOf{TSubject}"/>.</summary>
public sealed record AllOfGroup<TSubject> : ConditionGroup<TSubject> where TSubject : class
{
   internal AllOfGroup(IReadOnlyList<Condition<TSubject>> conditions) : base(conditions) {}

   public override bool IsMetBy(TSubject subject) => ConditionsThatAddSomething.All(it => it.IsMetBy(subject));

   protected override string Heading => "All of:";

   /// <summary>The key an "all of" group is stored under in the corpus.</summary>
   internal const string JsonKey = "allOf";

   internal override JsonObject ToJson() => ToJsonUnder(JsonKey);
}
