using System;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;

/// <summary>
/// A matching rule that a vocab can require or forbid, such as following a masu stem: required, it's the condition <see cref="WhenRequired"/>, and<br/>
/// forbidden, <see cref="WhenForbidden"/>, which is usually that condition negated.
/// </summary>
public sealed class RequireForbidMatchingRule : MatchingRule
{
   readonly int _requiredWeight;
   readonly int _forbiddenWeight;

   /// <param name="name">The <see cref="MatchingRule.Name"/>.</param>
   /// <param name="whenRequired">The <see cref="WhenRequired"/>.</param>
   /// <param name="whenForbidden">The <see cref="WhenForbidden"/>.</param>
   /// <param name="requiredWeight">What the rule adds to the vocab's priority while required.</param>
   /// <param name="forbiddenWeight">What the rule adds to the vocab's priority while forbidden.</param>
   internal RequireForbidMatchingRule(string name,
                                      Condition<CandidateWordVariant> whenRequired,
                                      Condition<CandidateWordVariant> whenForbidden,
                                      int requiredWeight,
                                      int forbiddenWeight) : base(name)
   {
      WhenRequired = whenRequired;
      WhenForbidden = whenForbidden;
      _requiredWeight = requiredWeight;
      _forbiddenWeight = forbiddenWeight;
   }

   /// <summary>A rule that, forbidden, is its condition when required, negated. Forbidden, it adds 1 to the vocab's priority, as every forbidden rule did.</summary>
   internal RequireForbidMatchingRule(string name, Condition<CandidateWordVariant> whenRequired, int requiredWeight)
      : this(name, whenRequired, ConditionGroup.Not(whenRequired), requiredWeight, forbiddenWeight: 1) {}

   /// <summary>The condition the rule is while a vocab requires it.</summary>
   public Condition<CandidateWordVariant> WhenRequired { get; }

   /// <summary>The condition the rule is while a vocab forbids it.</summary>
   public Condition<CandidateWordVariant> WhenForbidden { get; }

   /// <summary>The condition the rule is when <paramref name="choice"/> is made: <see cref="WhenRequired"/>, <see cref="WhenForbidden"/>, or none when it's unset.</summary>
   internal Condition<CandidateWordVariant>? ConditionFor(RequireForbidChoice choice) => choice switch
   {
      RequireForbidChoice.Required => WhenRequired,
      RequireForbidChoice.Forbidden => WhenForbidden,
      RequireForbidChoice.Unset => null,
      _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, null)
   };

   /// <summary>The choice <paramref name="condition"/> stands for, meaning the same as one of the rule's conditions: required, forbidden, or unset when it isn't this rule's.</summary>
   internal RequireForbidChoice ChoiceOf(Condition<CandidateWordVariant> condition) =>
      condition.MeansTheSameAs(WhenRequired) ? RequireForbidChoice.Required
      : condition.MeansTheSameAs(WhenForbidden) ? RequireForbidChoice.Forbidden
      : RequireForbidChoice.Unset;

   internal override bool Recognizes(Condition<CandidateWordVariant> condition) => ChoiceOf(condition) != RequireForbidChoice.Unset;

   internal override int WeightOf(Condition<CandidateWordVariant> condition) => ChoiceOf(condition) switch
   {
      RequireForbidChoice.Required => _requiredWeight,
      RequireForbidChoice.Forbidden => _forbiddenWeight,
      _ => 0
   };
}
