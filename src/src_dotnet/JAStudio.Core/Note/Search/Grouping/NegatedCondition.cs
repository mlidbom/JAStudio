using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Search.Grouping;

/// <summary>
/// A condition that a note matches when it doesn't match <see cref="Condition"/> (NOT). Made by <see cref="ConditionGroup.Not"/>.<br/>
/// The negation of an empty condition is empty too, so it's ignored like one.
/// </summary>
public sealed record NegatedCondition : NoteCondition
{
   internal NegatedCondition(NoteCondition condition) => Condition = condition;

   /// <summary>The condition a note must not match.</summary>
   public NoteCondition Condition { get; }

   public override bool IsEmpty => Condition.IsEmpty;

   public override bool Matches(JPNote note) => IsEmpty || !Condition.Matches(note);

   public override IEnumerable<string> DescriptionLines() => ["Not:", ..Condition.DescriptionLines().Select(Indented)];
}
