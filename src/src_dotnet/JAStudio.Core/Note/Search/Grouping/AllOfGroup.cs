using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Search.Grouping;

/// <summary>A group of conditions that a note matches when it matches all of them (AND). Made by <see cref="ConditionGroup.AllOf"/>.</summary>
public sealed record AllOfGroup : ConditionGroup
{
   internal AllOfGroup(IReadOnlyList<NoteCondition> conditions) : base(conditions) {}

   public override bool Matches(JPNote note) => ConditionsThatAddSomething.All(it => it.Matches(note));

   protected override string Heading => "All of:";
}
