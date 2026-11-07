using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Search.Grouping;

/// <summary>A group of conditions that a note matches when it matches any of them (OR). Made by <see cref="ConditionGroup.AnyOf"/>.</summary>
public sealed record AnyOfGroup : ConditionGroup
{
   internal AnyOfGroup(IReadOnlyList<NoteCondition> conditions) : base(conditions) {}

   public override bool Matches(JPNote note) => IsEmpty || ConditionsThatAddSomething.Any(it => it.Matches(note));

   protected override string Heading => "Any of:";

   public override ConditionGroup SwitchedBetweenAllOfAndAnyOf() => new AllOfGroup(Conditions);

   protected override ConditionGroup WithConditions(IReadOnlyList<NoteCondition> conditions) => new AnyOfGroup(conditions);
}
