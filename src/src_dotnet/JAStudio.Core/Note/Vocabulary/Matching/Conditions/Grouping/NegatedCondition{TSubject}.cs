using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

/// <summary>
/// A condition that a subject meets when it doesn't meet <see cref="Condition"/> (NOT). Made by <see cref="ConditionGroup.Not{TSubject}"/>.<br/>
/// The negation of an empty condition is empty too, so it's ignored like one.
/// </summary>
public sealed record NegatedCondition<TSubject> : Condition<TSubject> where TSubject : class
{
   const string Prefix = "Not: ";

   internal NegatedCondition(Condition<TSubject> condition) => Condition = condition;

   /// <summary>The condition a subject must not meet.</summary>
   public Condition<TSubject> Condition { get; }

   public override bool IsEmpty => Condition.IsEmpty;

   public override bool IsMetBy(TSubject subject) => IsEmpty || !Condition.IsMetBy(subject);

   /// <summary>The negated condition's own lines, with "Not: " in front of the first, so a negation shows on the same line as what it negates.</summary>
   public override IEnumerable<string> DescriptionLines() => Condition.DescriptionLinesPrefixedWith(Prefix);

   /// <summary>The key a negation is stored under in the corpus.</summary>
   internal const string JsonKey = "not";

   internal override JsonObject ToJson() => new() { [JsonKey] = Condition.ToJson() };
}
