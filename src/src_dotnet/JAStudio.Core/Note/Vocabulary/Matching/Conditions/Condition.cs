using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using JAStudio.Core.Note.ConditionTrees;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A node in a vocab's match-condition, whatever its subject: a word, a token or a text. What a condition is met by is <see cref="Condition{TSubject}"/>'s;<br/>
/// this is the tree's shape, as the match-condition editor walks and changes it: each condition's <see cref="Children"/>, and the tree with a<br/>
/// condition at a <see cref="ConditionPosition"/> replaced.
/// </summary>
/// <remarks>
/// A tree holds conditions on different subjects: a condition on a word holds one on its preceding token (<see cref="ISubjectCondition"/>), which<br/>
/// can hold one on the token's surface. So the editor works with conditions as this type, and a condition put into a tree must be on the subject<br/>
/// of the place it's put: a group's conditions are on the group's subject.
/// </remarks>
public abstract record Condition
{
   /// <summary>
   /// Whether this condition adds nothing: a group with no conditions in it, or only empty ones, or a condition on an empty one.<br/>
   /// The group it's in ignores it, as the note search ignores an empty group (<see cref="JAStudio.Core.Note.Search.NoteCondition.IsEmpty"/>), so that a<br/>
   /// group just added changes nothing until it has conditions.
   /// </summary>
   public virtual bool IsEmpty => false;

   /// <summary>
   /// The condition written as code, where <paramref name="writing"/> says: its line, or lines, with what goes in front of it and after it. See<br/>
   /// <see cref="ConditionCode"/>.
   /// </summary>
   internal abstract IEnumerable<CodeLine> CodeLines(CodeWriting writing);

   /// <summary>
   /// The conditions directly under this one in the tree: a group's conditions, the one a <see cref="NegatedCondition{TSubject}"/> negates, or the one a<br/>
   /// <see cref="ISubjectCondition"/> holds on its subject. Any other condition has none.
   /// </summary>
   public virtual IReadOnlyList<Condition> Children => [];

   /// <summary>The condition at <paramref name="position"/> in the tree this condition is the root of.</summary>
   public Condition ConditionAt(ConditionPosition position) =>
      position.IsRoot ? this : Children[position.FirstIndex].ConditionAt(position.BelowTheFirstChild);

   /// <summary>
   /// The tree this condition is the root of, with the condition at <paramref name="position"/> replaced by <paramref name="replacement"/>, which must<br/>
   /// be on the same subject. Conditions are values, so this is a new tree, and this one is unchanged.
   /// </summary>
   public Condition WithConditionAt(ConditionPosition position, Condition replacement) =>
      position.IsRoot
         ? replacement
         : WithChildReplaced(position.FirstIndex, Children[position.FirstIndex].WithConditionAt(position.BelowTheFirstChild, replacement));

   /// <summary>A condition that a subject meets when it doesn't meet this one.</summary>
   public abstract Condition Negated();

   /// <summary>This condition with its child at <paramref name="index"/> replaced by <paramref name="replacement"/>. Only conditions with <see cref="Children"/> have one to replace.</summary>
   protected virtual Condition WithChildReplaced(int index, Condition replacement) =>
      throw new InvalidOperationException($"A {GetType().Name} has no children to replace.");

   /// <summary>This condition as the condition on <typeparamref name="TSubject"/> it must be, such as a condition put into a group on that subject.</summary>
   /// <exception cref="ArgumentException">When it's a condition on another subject.</exception>
   internal Condition<TSubject> AsAConditionOn<TSubject>() where TSubject : class =>
      this as Condition<TSubject> ?? throw new ArgumentException($"Not a condition on a {typeof(TSubject).Name}: {this}");

   /// <summary>The condition as the corpus stores it: an object whose key says what kind of condition it is. <see cref="ConditionLevel{TSubject}"/> reads it back.</summary>
   internal abstract JsonObject ToJson();

   /// <summary>
   /// The texts that the <see cref="TextComparison"/>s in this condition compare with, one <see cref="TextSet"/> for each: its own when it is one,<br/>
   /// else those of the conditions it holds. None for a condition that compares no text.
   /// </summary>
   internal virtual IEnumerable<TextSet> ComparedTextSets() => [];

   /// <summary>
   /// The shared-conditions this condition uses by name: itself when it is one, else those the conditions it holds use, each as often as it's used. Not<br/>
   /// those a shared-condition's definition uses: its own <see cref="ISharedCondition.Definition"/> says those.
   /// </summary>
   public virtual IEnumerable<ISharedCondition> SharedConditionsUsed() => Children.SelectMany(it => it.SharedConditionsUsed());

   /// <summary>Whether this condition uses <paramref name="sharedCondition"/> by name: whether it's among its <see cref="SharedConditionsUsed"/>.</summary>
   public bool Uses(ISharedCondition sharedCondition) => SharedConditionsUsed().Contains(sharedCondition);

   /// <summary>
   /// Whether this condition means the same as <paramref name="other"/>: whether the two are built of the same conditions, but for the order of those in<br/>
   /// a group, and with each shared-condition taken as its definition. So a group's conditions can be put in any order, and a condition given a name,<br/>
   /// without changing what it weighs in a vocab's priority, or where <see cref="WithSharedConditionUsed"/> finds a shared-condition's definition.
   /// </summary>
   /// <remarks>
   /// Equality says more: that the two are built the same way, in the same order, of the same names. The match-condition editor relies on it, since<br/>
   /// putting a group's conditions in another order is a change to the match-condition.
   /// </remarks>
   public bool MeansTheSameAs(Condition other) => other is ISharedCondition sharedCondition ? MeansTheSameAs(sharedCondition.Definition) : MeansTheSameAsAConditionNotShared(other);

   /// <summary>
   /// Whether this condition means the same as <paramref name="other"/>, which isn't a shared-condition (<see cref="MeansTheSameAs"/>): whether they're<br/>
   /// equal, for a condition that holds no others.
   /// </summary>
   protected virtual bool MeansTheSameAsAConditionNotShared(Condition other) => Equals(other);

   /// <summary>
   /// This condition with <paramref name="sharedCondition"/> used, by name, wherever its definition is in it: each condition that means the same as the<br/>
   /// definition (<see cref="MeansTheSameAs"/>) is replaced by the shared-condition, this one too. And when the definition is a group of two or more<br/>
   /// conditions, a group of the same kind that holds conditions meaning the same as each of them, among others, has them replaced by it:<br/>
   /// <c>AllOf(A, B, C)</c> with a definition <c>AllOf(A, B)</c> becomes <c>AllOf(Shared, C)</c>. Conditions are values, so this is a new tree.
   /// </summary>
   /// <remarks>
   /// What it makes means the same as this condition, so a vocab whose match-condition uses the shared-condition in its definition's place matches the<br/>
   /// same words and weighs the same. A shared-condition's definition isn't looked into: only what uses it by name.
   /// </remarks>
   public Condition WithSharedConditionUsed(ISharedCondition sharedCondition) =>
      MeansTheSameAs(sharedCondition.Definition) ? sharedCondition.AsACondition : WithSharedConditionUsedInTheConditionsItHolds(sharedCondition);

   /// <summary>
   /// This condition, which doesn't mean the same as <paramref name="sharedCondition"/>'s definition, with the shared-condition used wherever its<br/>
   /// definition is in the conditions it holds (<see cref="WithSharedConditionUsed"/>).
   /// </summary>
   protected virtual Condition WithSharedConditionUsedInTheConditionsItHolds(ISharedCondition sharedCondition) =>
      Children.Index().Aggregate(this, (condition, child) => condition.WithChildReplaced(child.Index, child.Item.WithSharedConditionUsed(sharedCondition)));

   /// <summary>The condition as code on one line, such as <c>!PrecedingToken.IsAGodanVerb</c>, as a failure reason shows it (<see cref="ConditionCode.OnOneLine(Condition)"/>).</summary>
   public sealed override string ToString() => ConditionCode.OnOneLine(this);
}
