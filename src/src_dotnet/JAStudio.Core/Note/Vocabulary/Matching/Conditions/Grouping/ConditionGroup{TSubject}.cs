using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

/// <summary>Conditions combined into one: <see cref="AllOfGroup{TSubject}"/> or <see cref="AnyOfGroup{TSubject}"/>. Made by <see cref="ConditionGroup"/>.</summary>
/// <remarks>The empty conditions in a group are ignored (see <see cref="Condition.IsEmpty"/>).</remarks>
public abstract record ConditionGroup<TSubject> : Condition<TSubject>, IConditionGroup where TSubject : class
{
   protected ConditionGroup(IReadOnlyList<Condition<TSubject>> conditions) => Conditions = conditions;

   /// <summary>The conditions in this group, in the order the editor shows them.</summary>
   public IReadOnlyList<Condition<TSubject>> Conditions { get; }

   IReadOnlyList<Condition> IConditionGroup.Conditions => Conditions;

   public override IReadOnlyList<Condition> Children => Conditions;

   public override bool IsEmpty => Conditions.All(it => it.IsEmpty);

   public abstract bool IsAnyOf { get; }

   public Condition WithConditionAdded(Condition condition) => WithConditions([..Conditions, condition.AsAConditionOn<TSubject>()]);

   public Condition WithoutConditionAt(int index) => WithConditions([..Conditions.Where((_, conditionIndex) => conditionIndex != index)]);

   public abstract Condition SwitchedBetweenAllOfAndAnyOf();

   /// <summary>A group of the same kind as this one, with <paramref name="conditions"/> in it.</summary>
   protected abstract ConditionGroup<TSubject> WithConditions(IReadOnlyList<Condition<TSubject>> conditions);

   protected override Condition WithChildReplaced(int index, Condition replacement) =>
      WithConditions([..Conditions.Select((condition, conditionIndex) => conditionIndex == index ? replacement.AsAConditionOn<TSubject>() : condition)]);

   /// <summary>The conditions that decide whether a subject meets this group: all but the empty ones.</summary>
   protected IEnumerable<Condition<TSubject>> ConditionsThatAddSomething => Conditions.Where(it => !it.IsEmpty);

   /// <summary>The group's name in code, such as <c>AllOf</c>.</summary>
   public abstract string Name { get; }

   /// <summary>
   /// The group's code: its name and an opening parenthesis on its line, such as <c>AnyOf(</c>, then each of its conditions on a line of its own, a comma<br/>
   /// after each but the last, which closes the group. An empty group is <c>AnyOf()</c>.
   /// </summary>
   internal override IEnumerable<Code.CodeLine> CodeLines(Code.CodeWriting writing) =>
      Conditions.Count == 0
         ? [writing.LineOf($"{Name}()")]
         : [writing.OpeningLineOf($"{Name}("), ..Conditions.SelectMany((condition, index) => condition.CodeLines(writing.ForTheConditionInTheGroupAt(index, AfterTheConditionAt(index, writing))))];

   /// <summary>What goes after the condition at <paramref name="index"/>: a comma, or after the last, the closing parenthesis and what goes after the group.</summary>
   string AfterTheConditionAt(int index, Code.CodeWriting writing) => index < Conditions.Count - 1 ? "," : ")" + writing.After;

   /// <summary>The group as the corpus stores it: its conditions under <paramref name="jsonKey"/>, such as <c>allOf</c>.</summary>
   protected JsonObject ToJsonUnder(string jsonKey) => new() { [jsonKey] = new JsonArray([.. Conditions.Select(JsonNode (it) => it.ToJson())]) };

   internal override IEnumerable<TextSet> ComparedTextSets() => Conditions.SelectMany(it => it.ComparedTextSets());

   public virtual bool Equals(ConditionGroup<TSubject>? other) => other is not null && base.Equals(other) && Conditions.SequenceEqual(other.Conditions);

   public override int GetHashCode() => Conditions.Aggregate(EqualityContract.GetHashCode(), HashCode.Combine);
}
