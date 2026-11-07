using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Search.Grouping;

/// <summary>
/// A condition that a note matches when it doesn't match <see cref="Condition"/> (NOT). Made by <see cref="ConditionGroup.Not"/>.<br/>
/// The negation of an empty condition is empty too, so it's ignored like one.
/// </summary>
public sealed record NegatedCondition : NoteCondition
{
   const string Prefix = "Not: ";

   internal NegatedCondition(NoteCondition condition) => Condition = condition;

   /// <summary>The condition a note must not match.</summary>
   public NoteCondition Condition { get; }

   public override bool IsEmpty => Condition.IsEmpty;

   public override bool Matches(JPNote note) => IsEmpty || !Condition.Matches(note);

   /// <summary>
   /// The negated condition's own lines, with "Not: " in front of the first, such as "Not: Tags include x" or "Not: All of:".<br/>
   /// So the search editor shows a negation on the same line as what it negates.
   /// </summary>
   public override IEnumerable<string> DescriptionLines()
   {
      var conditionLines = Condition.DescriptionLines().ToList();
      return [Prefix + conditionLines[0], ..conditionLines.Skip(1)];
   }

   public override IReadOnlyList<NoteCondition> Children => [Condition];

   protected override NoteCondition WithChildReplaced(int index, NoteCondition replacement) => new NegatedCondition(replacement);
}
