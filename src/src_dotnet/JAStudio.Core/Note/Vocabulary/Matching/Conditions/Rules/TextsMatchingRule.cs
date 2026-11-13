using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;

/// <summary>
/// A matching rule that a vocab gives texts, such as the prefixes a word mustn't follow: with texts, it's the condition<br/>
/// <see cref="ConditionOn"/> makes of them, and without any, it's no condition.
/// </summary>
public sealed class TextsMatchingRule : MatchingRule
{
   readonly Func<string[], Condition<CandidateWordVariant>> _conditionOn;
   readonly int _weight;

   /// <param name="name">The <see cref="MatchingRule.Name"/>.</param>
   /// <param name="conditionOn">Makes the rule's condition on texts, of which there's at least one.</param>
   /// <param name="weight">What the rule adds to the vocab's priority while it has texts.</param>
   internal TextsMatchingRule(string name, Func<string[], Condition<CandidateWordVariant>> conditionOn, int weight) : base(name)
   {
      _conditionOn = conditionOn;
      _weight = weight;
   }

   /// <summary>The rule's condition on the <paramref name="texts"/>, of which a <see cref="TextSet"/> always has at least one.</summary>
   Condition<CandidateWordVariant> ConditionOn(TextSet texts) => _conditionOn([.. texts.Texts]);

   /// <summary>The texts of <paramref name="condition"/> when it's this rule's condition on them, in ordinal order; none when it isn't this rule's.</summary>
   internal IReadOnlyList<string> TextsIn(Condition<CandidateWordVariant> condition)
   {
      return condition.ComparedTextSets().ToList() is [var theOnlyComparedTexts] && IsThisRulesConditionOn(theOnlyComparedTexts) ? theOnlyComparedTexts.Texts : [];

      bool IsThisRulesConditionOn(TextSet texts) => condition.MeansTheSameAs(ConditionOn(texts));
   }

   internal override bool Recognizes(Condition<CandidateWordVariant> condition) => TextsIn(condition).Count > 0;

   internal override int WeightOf(Condition<CandidateWordVariant> condition) => Recognizes(condition) ? _weight : 0;
}
