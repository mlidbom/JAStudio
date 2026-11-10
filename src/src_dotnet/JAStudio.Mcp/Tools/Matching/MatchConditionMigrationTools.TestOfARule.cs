using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Migration;

namespace JAStudio.Mcp.Tools.Matching;

public partial class MatchConditionMigrationTools
{
   /// <summary>One rule of a vocab match's vocab, tested on the match both ways: by the condition it becomes, and by the requirement that tests it today.</summary>
   sealed record TestOfARule(SentenceNote Sentence, VocabMatch Match, TodaysMatchingRule Rule)
   {
      /// <summary>Whether the rule's condition and today's requirement agree on whether the match is valid.</summary>
      public bool Agrees { get; } = Rule.ConditionAgreesWithTodaysRequirementOn(Match);

      /// <summary>The test as the tool reports a disagreement: the sentence, the match, and what each way said.</summary>
      public RuleDisagreement Disagreement() =>
         new(Sentence.GetId().ToString(),
             Sentence.Question.WithoutInvisibleSpace(),
             Match.Variant.Form,
             Match.Variant.IsSurface ? "surface" : "base",
             Match.Vocab.GetId().ToString(),
             Match.Vocab.GetQuestion(),
             Rule.TodaysRequirementIsMetBy(Match),
             [.. Rule.ConditionIn(Match.MatchingConfiguration).DescriptionLines()]);
   }
}
