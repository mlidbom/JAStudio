using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

/// <summary>
/// A condition that a subject meets when it doesn't meet <see cref="Condition"/> (NOT). Made by <see cref="ConditionGroup.Not{TSubject}"/>.<br/>
/// The negation of an empty condition is empty too, so it's ignored like one.
/// </summary>
public sealed record NegatedCondition<TSubject> : Condition<TSubject>, INegatedCondition where TSubject : class
{
   internal NegatedCondition(Condition<TSubject> condition) => Condition = condition;

   /// <summary>The condition a subject must not meet.</summary>
   public Condition<TSubject> Condition { get; }

   Condition INegatedCondition.Condition => Condition;

   public override IReadOnlyList<Condition> Children => [Condition];

   public override bool IsEmpty => Condition.IsEmpty;

   public override bool IsMetBy(TSubject subject) => IsEmpty || !Condition.IsMetBy(subject);

   /// <summary>The negated condition's code, with <c>!</c> in front of it, on its line, such as <c>!PrecedingToken.IsAGodanVerb</c>.</summary>
   internal override IEnumerable<CodeLine> CodeLines(CodeWriting writing) => Condition.CodeLines(writing.ForTheConditionHeldWith(CodePiece.Negation));

   /// <summary>The key a negation is stored under in the corpus.</summary>
   internal const string JsonKey = "not";

   internal override JsonObject ToJson() => new() { [JsonKey] = Condition.ToJson() };

   internal override IEnumerable<TextSet> ComparedTextSets() => Condition.ComparedTextSets();

   protected override Condition WithChildReplaced(int index, Condition replacement) => new NegatedCondition<TSubject>(replacement.AsAConditionOn<TSubject>());

   /// <summary>Whether <paramref name="other"/> negates a condition meaning the same as the one this negates.</summary>
   protected override bool MeansTheSameAsAConditionNotShared(Condition other) => other is NegatedCondition<TSubject> negation && Condition.MeansTheSameAs(negation.Condition);

   internal override int HashCodeOfItsMeaning() => HashCode.Combine(typeof(NegatedCondition<TSubject>), Condition.HashCodeOfItsMeaning());
}
