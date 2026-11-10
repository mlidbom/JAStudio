using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

/// <summary>Conditions combined into one: <see cref="AllOfGroup{TSubject}"/> or <see cref="AnyOfGroup{TSubject}"/>. Made by <see cref="ConditionGroup"/>.</summary>
/// <remarks>The empty conditions in a group are ignored (see <see cref="Condition{TSubject}.IsEmpty"/>).</remarks>
public abstract record ConditionGroup<TSubject> : Condition<TSubject> where TSubject : class
{
   protected ConditionGroup(IReadOnlyList<Condition<TSubject>> conditions) => Conditions = conditions;

   /// <summary>The conditions in this group, in the order the editor shows them.</summary>
   public IReadOnlyList<Condition<TSubject>> Conditions { get; }

   public override bool IsEmpty => Conditions.All(it => it.IsEmpty);

   /// <summary>The conditions that decide whether a subject meets this group: all but the empty ones.</summary>
   protected IEnumerable<Condition<TSubject>> ConditionsThatAddSomething => Conditions.Where(it => !it.IsEmpty);

   /// <summary>The line the editor shows for the group itself, above its conditions, such as "All of:".</summary>
   protected abstract string Heading { get; }

   public override IEnumerable<string> DescriptionLines() => [Heading, ..Conditions.SelectMany(it => it.DescriptionLines()).Select(Indented)];

   /// <summary>The group as the corpus stores it: its conditions under <paramref name="jsonKey"/>, such as <c>allOf</c>.</summary>
   protected JsonObject ToJsonUnder(string jsonKey) => new() { [jsonKey] = new JsonArray([.. Conditions.Select(JsonNode (it) => it.ToJson())]) };

   public virtual bool Equals(ConditionGroup<TSubject>? other) => other is not null && base.Equals(other) && Conditions.SequenceEqual(other.Conditions);

   public override int GetHashCode() => Conditions.Aggregate(EqualityContract.GetHashCode(), HashCode.Combine);
}
