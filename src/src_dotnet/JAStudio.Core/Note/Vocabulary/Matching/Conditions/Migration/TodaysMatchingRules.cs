using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Tail;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Migration;

/// <summary>
/// The matching rules the corpus stores today that a vocab's match-condition replaces, each with the condition it becomes.
/// </summary>
/// <remarks>
/// <para>
/// The rules it doesn't replace: the poison word flag stays a flag, and dictionary form (start) is forbidden by default, for every vocab, so it<br/>
/// can't be a condition of the vocab that set it (open question 1 in dev_docs/matching-configuration.md). Nor does it replace the rules that<br/>
/// decide whether a valid match is displayed, rather than whether it's valid: yield to an overlapping following compound, and yield to surface.
/// </para>
/// <para>Goes once the corpus stores match-conditions (increment 3 in dev_docs/matching-configuration.md).</para>
/// </remarks>
public static class TodaysMatchingRules
{
   /// <summary>The rules a match-condition replaces, in the order <see cref="VocabMatch"/> tests them today.</summary>
   public static readonly IReadOnlyList<TodaysMatchingRule> ThatAMatchConditionReplaces =
   [
      TodaysMatchingRule.RequireForbid("irrealis", it => it.Irrealis, PrecedingToken.Meets(TokenConditions.IsInAnIrrealisForm), RequiresOrForbids.Irrealis.ApplyTo),
      TodaysMatchingRule.RequireForbid("godan", it => it.Godan, PrecedingToken.Meets(TokenConditions.IsAGodanVerb), RequiresOrForbids.Godan.ApplyTo),
      TodaysMatchingRule.RequireForbid("ichidan", it => it.Ichidan, PrecedingToken.Meets(TokenConditions.IsAnIchidanVerb), RequiresOrForbids.Ichidan.ApplyTo),
      TodaysMatchingRule.Texts("prefixIsNot",
                               it => it.PrefixIsNot,
                               texts => ConditionGroup.Not(PrecedingTokenSurfaceEndsWithOneOf(texts)),
                               ForbidsPrefixIsIn.ApplyTo),
      TodaysMatchingRule.Texts("requiredPrefix", it => it.RequiredPrefix, PrecedingTokenSurfaceEndsWithOneOf, RequiresPrefixIsIn.ApplyTo),
      TodaysMatchingRule.RequireForbid("sentenceStart", it => it.SentenceStart, IsAtTheSentencesStart(), RequiresOrForbidsIsSentenceStart.ApplyTo),
      TodaysMatchingRule.RequireForbid("teFormStemHead", it => it.TeFormStemHead, PrecedingToken.Meets(TokenConditions.HasATeFormStem), RequiresOrForbids.TeFormStemHead.ApplyTo),
      TodaysMatchingRule.RequireForbid("teFormStemStart", it => it.TeFormStemStart, FirstToken.Meets(TokenConditions.HasATeFormStem), RequiresOrForbidsTeFormStemStart.ApplyTo),
      TodaysMatchingRule.RequireForbid("pastTenseStem", it => it.PastTenseStem, FollowsAPastTenseStem(), RequiresOrForbidsPastTenseStem.ApplyTo),
      TodaysMatchingRule.RequireForbid("godanImperativeHead",
                                       it => it.GodanImperativeHead,
                                       PrecedingToken.Meets(TokenConditions.IsAGodanImperativeInflection),
                                       RequiresOrForbidsGodanImperativeHead.ApplyTo),
      TodaysMatchingRule.RequireForbid("godanPotential",
                                       it => it.GodanPotential,
                                       FirstToken.Meets(ConditionGroup.AnyOf(TokenConditions.IsAGodanPotentialStem, TokenConditions.IsAGodanPotentialInflection)),
                                       RequiresOrForbidsStartsWithGodanPotentialStemOrInflection.ApplyTo),
      TodaysMatchingRule.RequireForbid("godanImperative",
                                       it => it.GodanImperative,
                                       FirstToken.Meets(ConditionGroup.AnyOf(TokenConditions.IsAGodanImperativeStem, TokenConditions.IsAGodanImperativeInflection)),
                                       RequiresOrForbidsStartsWithGodanImperativeStemOrInflection.ApplyTo),
      TodaysMatchingRule.RequireForbid("ichidanImperative",
                                       it => it.IchidanImperative,
                                       FirstToken.Meets(ConditionGroup.AnyOf(TokenConditions.IsAnIchidanImperativeStem, TokenConditions.IsAnIchidanImperativeInflection)),
                                       RequiresOrForbidsStartsWithIchidanImperativeStemOrInflection.ApplyTo),
      TodaysMatchingRule.RequireForbid("sentenceEnd", it => it.SentenceEnd, LastToken.Meets(TokenConditions.EndsAStatement), RequiresOrForbidsIsSentenceEnd.ApplyTo),
      TodaysMatchingRule.Texts("suffixIsNot",
                               it => it.SuffixIsNot,
                               texts => ConditionGroup.Not(FollowingTokenSurfaceStartsWithOneOf(texts)),
                               ForbidsSuffixIsIn.ApplyTo),
      TodaysMatchingRule.RequireForbid("masuStem", it => it.MasuStem, PrecedingToken.Meets(TokenConditions.IsAMasuStem), RequiresOrForbidsMasuStem.ApplyTo),
      TodaysMatchingRule.RequireForbid("adverbHead", it => it.AdverbHead, PrecedingToken.Meets(TokenConditions.IsAnAdverb), RequiresOrForbidsAdverbHead.ApplyTo),
      TodaysMatchingRule.RequireForbid("dictionaryFormHead",
                                       it => it.DictionaryFormHead,
                                       PrecedingToken.Meets(TokenConditions.IsADictionaryVerbInflection),
                                       RequiresOrForbidsDictionaryFormHead.ApplyTo),
      TodaysMatchingRule.RequireForbid("surface",
                                       it => it.Surface,
                                       IsMatchedByItsSurface,
                                       ConditionGroup.Not(IsMatchedByAnInflectedSurface),
                                       RequiresOrForbidsSurface.ApplyTo),
      TodaysMatchingRule.RequireForbid("singleToken", it => it.SingleToken, IsASingleToken, RequiresOrForbidsIsSingleToken.ApplyTo),
      TodaysMatchingRule.Texts("surfaceIsNot", it => it.SurfaceIsNot, texts => ConditionGroup.Not(Surface.Meets(TextConditions.IsOneOf(texts))), ForbidsSurfaceIsIn.ApplyTo)
   ];

   /// <summary>
   /// The match-condition the rules <paramref name="configuration"/> sets become: all of the conditions each becomes. Empty, and so met by every<br/>
   /// word, when it sets none.
   /// </summary>
   public static AllOfGroup<CandidateWordVariant> MatchConditionOf(VocabNoteMatchingConfiguration configuration) =>
      ConditionGroup.AllOf([.. SetIn(configuration).Select(it => it.ConditionIn(configuration))]);

   /// <summary>The rules a match-condition replaces that <paramref name="configuration"/> sets, in the order <see cref="VocabMatch"/> tests them today.</summary>
   public static IEnumerable<TodaysMatchingRule> SetIn(VocabNoteMatchingConfiguration configuration) => ThatAMatchConditionReplaces.Where(it => it.IsSetIn(configuration));

   static Condition<CandidateWordVariant> PrecedingTokenSurfaceEndsWithOneOf(string[] texts) =>
      PrecedingToken.Meets(TokenConditions.Surface.Meets(TextConditions.EndsWithOneOf(texts)));

   static Condition<CandidateWordVariant> FollowingTokenSurfaceStartsWithOneOf(string[] texts) =>
      FollowingToken.Meets(TokenConditions.Surface.Meets(TextConditions.StartsWithOneOf(texts)));

   /// <summary>Nothing precedes the word in the sentence but what may start one, such as a quote or a full stop.</summary>
   static Condition<CandidateWordVariant> IsAtTheSentencesStart() =>
      //review:matching: not quite today's rule (RequiresOrForbidsIsSentenceStart), which reads the preceding token's surface: an empty one counts as
      //   the sentence's start. The stem JAStudio splits off the く of 寄ってく is one, so today's rule finds a sentence start mid-sentence. In
      //   the corpus that's 25 matches, such as おう on such a う, which its required prefix rejects anyway: the full rebuild changes nothing.
      PrecedingText.Meets(ConditionGroup.AnyOf(TextConditions.IsEmptyText,
                                               TextConditions.EndsWithOneOf([.. AnalysisConstants.SentenceStartCharacters.Select(it => it.ToString())])));

   /// <summary>
   /// The word follows a past-tense stem, or starts with the past-tense marker that follows one. Not at the sentence's start, where a marker<br/>
   /// follows nothing: there it's the tokenizer's misreading, such as the だ of だーめ.
   /// </summary>
   static Condition<CandidateWordVariant> FollowsAPastTenseStem() =>
      ConditionGroup.AnyOf(PrecedingToken.Meets(TokenConditions.IsAPastTenseStem),
                           ConditionGroup.AllOf(ConditionGroup.Not(PrecedingText.Meets(TextConditions.IsEmptyText)), FirstToken.Meets(TokenConditions.IsAPastTenseMarker)));
}
