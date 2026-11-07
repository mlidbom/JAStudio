using System;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Search.Grouping;

/// <summary>
/// Conditions combined into one: <see cref="AllOfGroup"/> (AND) or <see cref="AnyOfGroup"/> (OR).<br/>
/// It also makes the conditions that combine others: <see cref="AllOf"/>, <see cref="AnyOf"/> and <see cref="Not"/>.
/// </summary>
/// <remarks>
/// The empty conditions in a group are ignored (see <see cref="NoteCondition.IsEmpty"/>),<br/>
/// so a group the search editor has just added, with nothing in it yet, doesn't change what the search finds.
/// </remarks>
public abstract record ConditionGroup : NoteCondition
{
   protected ConditionGroup(IReadOnlyList<NoteCondition> conditions) => Conditions = conditions;

   /// <summary>The conditions in this group, in the order the search editor shows them.</summary>
   public IReadOnlyList<NoteCondition> Conditions { get; }

   /// <summary>A condition that a note matches when it matches all of <paramref name="conditions"/>.</summary>
   public static AllOfGroup AllOf(params NoteCondition[] conditions) => new([..conditions]);

   /// <summary>A condition that a note matches when it matches any of <paramref name="conditions"/>.</summary>
   public static AnyOfGroup AnyOf(params NoteCondition[] conditions) => new([..conditions]);

   /// <summary>A condition that a note matches when it doesn't match <paramref name="condition"/>.</summary>
   public static NegatedCondition Not(NoteCondition condition) => new(condition);

   public override bool IsEmpty => Conditions.All(it => it.IsEmpty);

   /// <summary>The conditions that decide whether a note matches this group: all but the empty ones.</summary>
   protected IEnumerable<NoteCondition> ConditionsThatAddSomething => Conditions.Where(it => !it.IsEmpty);

   /// <summary>The line the search editor shows for the group itself, above its conditions, such as "All of:".</summary>
   protected abstract string Heading { get; }

   public override IEnumerable<string> DescriptionLines() => [Heading, ..Conditions.SelectMany(it => it.DescriptionLines()).Select(Indented)];

   public virtual bool Equals(ConditionGroup? other) => other is not null && base.Equals(other) && Conditions.SequenceEqual(other.Conditions);

   public override int GetHashCode() => Conditions.Aggregate(EqualityContract.GetHashCode(), HashCode.Combine);
}
