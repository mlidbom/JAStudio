using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

/// <summary>Conditions combined into one: <see cref="AllOfGroup{TSubject}"/> or <see cref="AnyOfGroup{TSubject}"/>. Made by <see cref="ConditionGroup"/>.</summary>
/// <remarks>The empty conditions in a group are ignored (see <see cref="Condition.IsEmpty"/>).</remarks>
public abstract record ConditionGroup<TSubject> : Condition<TSubject>, IConditionGroup where TSubject : class
{
   protected ConditionGroup(IReadOnlyList<Condition<TSubject>> conditions) => Conditions = conditions;

   /// <summary>The conditions in this group, in the order the editor shows them.</summary>
   public IReadOnlyList<Condition<TSubject>> Conditions { get; }

   /// <summary>
   /// The group's conditions, flattened: a group of the same kind among them, or a shared-condition defined as one, taken as the conditions it holds,<br/>
   /// all the way down. <c>AllOf(A, AllOf(B, C))</c> holds <c>A</c>, <c>B</c> and <c>C</c>, as <c>AllOf(A, B, C)</c> does, and means the same<br/>
   /// (<see cref="Condition.MeansTheSameAs"/>). A vocab's priority weighs these (<see cref="Rules.MatchConditionAsRules"/>), so that putting some of a<br/>
   /// match-condition's conditions in a shared-condition of their own changes nothing it weighs.
   /// </summary>
   public IEnumerable<Condition<TSubject>> FlattenedConditions => Conditions.SelectMany(FlattenedIntoThisGroup);

   IReadOnlyList<Condition> IConditionGroup.Conditions => Conditions;

   public override IReadOnlyList<Condition> Children => Conditions;

   public override bool IsEmpty => Conditions.All(it => it.IsEmpty);

   public abstract bool IsAnyOf { get; }

   /// <summary>Whether <paramref name="group"/> is of the same kind as this one: both "all of", or both "any of".</summary>
   bool IsOfTheSameKindAs(ConditionGroup<TSubject> group) => group.IsAnyOf == IsAnyOf;

   public Condition WithConditionAdded(Condition condition) => WithConditions([..Conditions, condition.AsAConditionOn<TSubject>()]);

   public Condition WithoutConditionAt(int index) => WithConditions([..Conditions.Where((_, conditionIndex) => conditionIndex != index)]);

   /// <summary>
   /// This group with <paramref name="replacement"/> in place of its conditions at <paramref name="indexes"/>: in the place of the first of them, and the<br/>
   /// others gone.
   /// </summary>
   ConditionGroup<TSubject> WithConditionsAtReplacedBy(IReadOnlyList<int> indexes, Condition<TSubject> replacement)
   {
      var placeOfTheReplacement = indexes.Min();
      return WithConditions([.. Conditions.Index().Where(it => !IsGone(it.Index)).Select(it => it.Index == placeOfTheReplacement ? replacement : it.Item)]);

      bool IsGone(int index) => index != placeOfTheReplacement && indexes.Contains(index);
   }

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
   internal override IEnumerable<CodeLine> CodeLines(CodeWriting writing) =>
      Conditions.Count == 0
         ? [writing.LineOf($"{Name}()")]
         : [writing.OpeningLineOf($"{Name}("), ..Conditions.SelectMany((condition, index) => condition.CodeLines(writing.ForTheConditionInTheGroupAt(index, AfterTheConditionAt(index, writing))))];

   /// <summary>What goes after the condition at <paramref name="index"/>: a comma, or after the last, the closing parenthesis and what goes after the group.</summary>
   string AfterTheConditionAt(int index, CodeWriting writing) => index < Conditions.Count - 1 ? "," : ")" + writing.After;

   /// <summary>The group as the corpus stores it: its conditions under <paramref name="jsonKey"/>, such as <c>allOf</c>.</summary>
   protected JsonObject ToJsonUnder(string jsonKey) => new() { [jsonKey] = new JsonArray([.. Conditions.Select(JsonNode (it) => it.ToJson())]) };

   internal override IEnumerable<TextSet> ComparedTextSets() => Conditions.SelectMany(it => it.ComparedTextSets());

   /// <summary>
   /// Whether <paramref name="other"/> is a group of the same kind whose <see cref="FlattenedConditions"/>, in whatever order, each mean the same as one of<br/>
   /// this group's.
   /// </summary>
   protected override bool MeansTheSameAsAConditionNotShared(Condition other) =>
      other is ConditionGroup<TSubject> group && IsOfTheSameKindAs(group) && FlattenedConditions.MeanTheSameInAnyOrderAs(group.FlattenedConditions);

   /// <summary>
   /// <paramref name="condition"/>, one of this group's, flattened into it (<see cref="FlattenedConditions"/>): the conditions it holds, flattened, when<br/>
   /// it's a group of this group's kind, or a shared-condition defined as one, else itself.
   /// </summary>
   IEnumerable<Condition<TSubject>> FlattenedIntoThisGroup(Condition<TSubject> condition) => condition switch
   {
      //review:matching: FlattenedIntoThisGroup was ConditionsHeldFlattened, which didn't say that what it gives depends on this group's kind:
      //   an "any of" in an "all of" stays as it is.
      ConditionGroup<TSubject> group when IsOfTheSameKindAs(group) => group.FlattenedConditions,
      SharedCondition<TSubject> { Definition: ConditionGroup<TSubject> definition } when IsOfTheSameKindAs(definition) => definition.FlattenedConditions,
      _ => [condition]
   };

   /// <summary>The group with <paramref name="sharedCondition"/> used in each of its conditions, then in place of some of them when they're its definition's.</summary>
   protected override Condition WithSharedConditionUsedInTheConditionsItHolds(ISharedCondition sharedCondition) =>
      WithConditions([.. Conditions.Select(it => it.WithSharedConditionUsed(sharedCondition).AsAConditionOn<TSubject>())]).WithSharedConditionInPlaceOfTheConditionsOfItsDefinition(sharedCondition);

   /// <summary>
   /// This group with <paramref name="sharedCondition"/> in place of the conditions that are its definition's, when the definition is a group of the same<br/>
   /// kind with two or more conditions, and this group holds conditions meaning the same as each of them: <c>AllOf(A, B, C)</c> with a definition<br/>
   /// <c>AllOf(A, B)</c> is <c>AllOf(Shared, C)</c>. The shared-condition takes the place of the first of them. Else this group as it is.
   /// </summary>
   ConditionGroup<TSubject> WithSharedConditionInPlaceOfTheConditionsOfItsDefinition(ISharedCondition sharedCondition)
   {
      return sharedCondition.Definition is ConditionGroup<TSubject> definition
             && IsOfTheSameKindAndHoldsTwoOrMore(definition)
             && Conditions.IndexesOfConditionsMeaningTheSameAsEachOf(definition.Conditions) is {} indexesOfTheDefinitionsConditions
                ? WithConditionsAtReplacedBy(indexesOfTheDefinitionsConditions, sharedCondition.AsACondition.AsAConditionOn<TSubject>())
                : this;

      bool IsOfTheSameKindAndHoldsTwoOrMore(ConditionGroup<TSubject> group) => IsOfTheSameKindAs(group) && group.Conditions.Count >= 2;
   }

   public virtual bool Equals(ConditionGroup<TSubject>? other) => other is not null && base.Equals(other) && Conditions.SequenceEqual(other.Conditions);

   public override int GetHashCode() => Conditions.Aggregate(EqualityContract.GetHashCode(), HashCode.Combine);
}
