using System;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;

/// <summary>
/// A matching rule that a vocab gives texts, such as the prefixes a word mustn't follow: with texts, it's the condition <see cref="ConditionOn"/> makes<br/>
/// of them, and without any, it's no condition.
/// </summary>
public sealed class TextsMatchingRule : MatchingRule
{
   readonly Func<string[], Condition<CandidateWordVariant>> _conditionOn;
   readonly int _weight;

   /// <param name="name">The <see cref="MatchingRule.Name"/>.</param>
   /// <param name="conditionOn">Makes the rule's condition on texts.</param>
   /// <param name="weight">What the rule adds to the vocab's priority while it has texts.</param>
   internal TextsMatchingRule(string name, Func<string[], Condition<CandidateWordVariant>> conditionOn, int weight) : base(name)
   {
      _conditionOn = conditionOn;
      _weight = weight;
   }

   /// <summary>The rule's condition on <paramref name="texts"/>.</summary>
   internal Condition<CandidateWordVariant> ConditionOn(IEnumerable<string> texts) => _conditionOn([.. texts]);

   /// <summary>The texts of <paramref name="condition"/> when it's this rule's condition on them, else null.</summary>
   internal TextSet? TextsIn(Condition<CandidateWordVariant> condition)
   {
      var texts = TheComparedTextsWithin(condition);
      return texts != null && ConditionOn(texts.Texts).Equals(condition) ? texts : null;

      //Finds the texts through the conditions a rule's comparison can be inside; building the rule's condition on them then tells whether it's the rule's.
      static TextSet? TheComparedTextsWithin(object node) => node switch
      {
         TextComparison comparison => comparison.Texts,
         NegatedCondition<CandidateWordVariant> negated => TheComparedTextsWithin(negated.Condition),
         SubjectCondition<CandidateWordVariant, IAnalysisToken> onAToken => TheComparedTextsWithin(onAToken.Condition),
         SubjectCondition<CandidateWordVariant, string> onAText => TheComparedTextsWithin(onAText.Condition),
         SubjectCondition<IAnalysisToken, string> onATokensText => TheComparedTextsWithin(onATokensText.Condition),
         _ => null
      };
   }

   internal override bool Recognizes(Condition<CandidateWordVariant> condition) => TextsIn(condition) != null;

   internal override int WeightOf(Condition<CandidateWordVariant> condition) => Recognizes(condition) ? _weight : 0;
}
