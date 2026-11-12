using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;

/// <summary>
/// The matching rules the Matching tab of a vocab offers (<see cref="MatchingRule"/>), each with the condition it is in a vocab's match-condition.<br/>
/// They're the rules a vocab had before match-conditions, and the corpus's vocab were migrated from them, so each condition is exactly what its rule<br/>
/// tested, but for <see cref="SentenceStart"/>.
/// </summary>
public static class MatchingRules
{
   /// <summary>The word follows a token in an irrealis form, such as the 食べ of 食べない.</summary>
   public static readonly RequireForbidMatchingRule Irrealis = new("irrealis", PrecedingToken.Meets(TokenConditions.IsInAnIrrealisForm), requiredWeight: 50);

   /// <summary>The word follows a godan verb.</summary>
   public static readonly RequireForbidMatchingRule Godan = new("godan", PrecedingToken.Meets(TokenConditions.IsAGodanVerb), requiredWeight: 50);

   /// <summary>The word follows an ichidan verb.</summary>
   public static readonly RequireForbidMatchingRule Ichidan = new("ichidan", PrecedingToken.Meets(TokenConditions.IsAnIchidanVerb), requiredWeight: 50);

   /// <summary>The word doesn't follow a token whose surface ends with one of the texts.</summary>
   public static readonly TextsMatchingRule PrefixIsNot = new("prefixIsNot", texts => ConditionGroup.Not(PrecedingTokenSurfaceEndsWithOneOf(texts)), weight: 2);

   /// <summary>The word follows a token whose surface ends with one of the texts.</summary>
   public static readonly TextsMatchingRule RequiredPrefix = new("requiredPrefix", PrecedingTokenSurfaceEndsWithOneOf, weight: 10);

   /// <summary>Nothing precedes the word in the sentence but what may start one, such as a quote or a full stop.</summary>
   public static readonly RequireForbidMatchingRule SentenceStart = new("sentenceStart", IsAtTheSentencesStart(), requiredWeight: 10);

   /// <summary>The word follows a token with a て-form stem.</summary>
   public static readonly RequireForbidMatchingRule TeFormStemHead = new("teFormStemHead", PrecedingToken.Meets(TokenConditions.HasATeFormStem), requiredWeight: 20);

   /// <summary>The word's first token has a て-form stem.</summary>
   public static readonly RequireForbidMatchingRule TeFormStemStart = new("teFormStemStart", FirstToken.Meets(TokenConditions.HasATeFormStem), requiredWeight: 20);

   /// <summary>
   /// The word follows a past-tense stem, or starts with the past-tense marker that follows one. Not at the sentence's start, where a marker<br/>
   /// follows nothing: there it's the tokenizer's misreading, such as the だ of だーめ.
   /// </summary>
   public static readonly RequireForbidMatchingRule PastTenseStem =
      new("pastTenseStem",
          ConditionGroup.AnyOf(PrecedingToken.Meets(TokenConditions.IsAPastTenseStem),
                               ConditionGroup.AllOf(ConditionGroup.Not(PrecedingText.Meets(TextConditions.IsEmpty)), FirstToken.Meets(TokenConditions.IsAPastTenseMarker))),
          requiredWeight: 30);

   /// <summary>The word follows the inflection of a godan verb's imperative.</summary>
   public static readonly RequireForbidMatchingRule GodanImperativeHead =
      new("godanImperativeHead", PrecedingToken.Meets(TokenConditions.IsAGodanImperativeInflection), requiredWeight: 30);

   /// <summary>The word starts with the stem or the inflection of a godan verb's potential form.</summary>
   public static readonly RequireForbidMatchingRule GodanPotential =
      new("godanPotential", FirstToken.Meets(ConditionGroup.AnyOf(TokenConditions.IsAGodanPotentialStem, TokenConditions.IsAGodanPotentialInflection)), requiredWeight: 30);

   /// <summary>The word starts with the stem or the inflection of a godan verb's imperative.</summary>
   public static readonly RequireForbidMatchingRule GodanImperative =
      new("godanImperative", FirstToken.Meets(ConditionGroup.AnyOf(TokenConditions.IsAGodanImperativeStem, TokenConditions.IsAGodanImperativeInflection)), requiredWeight: 30);

   /// <summary>The word starts with the stem or the inflection of an ichidan verb's imperative.</summary>
   public static readonly RequireForbidMatchingRule IchidanImperative =
      new("ichidanImperative", FirstToken.Meets(ConditionGroup.AnyOf(TokenConditions.IsAnIchidanImperativeStem, TokenConditions.IsAnIchidanImperativeInflection)), requiredWeight: 30);

   /// <summary>The word's last token ends a statement.</summary>
   public static readonly RequireForbidMatchingRule SentenceEnd = new("sentenceEnd", LastToken.Meets(TokenConditions.EndsAStatement), requiredWeight: 10);

   /// <summary>The word isn't followed by a token whose surface starts with one of the texts.</summary>
   public static readonly TextsMatchingRule SuffixIsNot = new("suffixIsNot", texts => ConditionGroup.Not(FollowingTokenSurfaceStartsWithOneOf(texts)), weight: 2);

   /// <summary>The word follows a masu stem, such as the 食べ of 食べます.</summary>
   public static readonly RequireForbidMatchingRule MasuStem = new("masuStem", PrecedingToken.Meets(TokenConditions.IsAMasuStem), requiredWeight: 50);

   /// <summary>The word follows an adverb.</summary>
   public static readonly RequireForbidMatchingRule AdverbHead = new("adverbHead", PrecedingToken.Meets(TokenConditions.IsAnAdverb), requiredWeight: 30);

   /// <summary>The word follows the ending JAStudio split off a dictionary-form verb.</summary>
   public static readonly RequireForbidMatchingRule DictionaryFormHead =
      new("dictionaryFormHead", PrecedingToken.Meets(TokenConditions.IsADictionaryVerbInflection), requiredWeight: 30);

   /// <summary>Required, the word is matched by its surface; forbidden, it's not matched by an inflected surface, one that differs from its base form.</summary>
   public static readonly RequireForbidMatchingRule Surface =
      new("surface", IsMatchedByItsSurface, ConditionGroup.Not(IsMatchedByAnInflectedSurface), requiredWeight: 10, forbiddenWeight: 1);

   /// <summary>Required, the word is a single token; forbidden, it's a compound of several.</summary>
   public static readonly RequireForbidMatchingRule SingleToken = new("singleToken", IsASingleToken, requiredWeight: 30);

   /// <summary>The word's surface isn't one of the texts.</summary>
   public static readonly TextsMatchingRule SurfaceIsNot = new("surfaceIsNot", texts => ConditionGroup.Not(WordConditions.Surface.Meets(TextConditions.In(texts))), weight: 2);

   /// <summary>Every rule, in the order a vocab's match-condition holds their conditions: the order matching tested the rules in before match-conditions.</summary>
   public static readonly IReadOnlyList<MatchingRule> All =
   [
      Irrealis, Godan, Ichidan, PrefixIsNot, RequiredPrefix, SentenceStart, TeFormStemHead, TeFormStemStart, PastTenseStem, GodanImperativeHead, GodanPotential,
      GodanImperative, IchidanImperative, SentenceEnd, SuffixIsNot, MasuStem, AdverbHead, DictionaryFormHead, Surface, SingleToken, SurfaceIsNot
   ];

   static Condition<CandidateWordVariant> PrecedingTokenSurfaceEndsWithOneOf(string[] texts) =>
      PrecedingToken.Meets(TokenConditions.Surface.Meets(TextConditions.EndsWithOneOf(texts)));

   static Condition<CandidateWordVariant> FollowingTokenSurfaceStartsWithOneOf(string[] texts) =>
      FollowingToken.Meets(TokenConditions.Surface.Meets(TextConditions.StartsWithOneOf(texts)));

   static Condition<CandidateWordVariant> IsAtTheSentencesStart() =>
      //review:matching: not quite the rule before match-conditions, which read the preceding token's surface: an empty one counted as the sentence's
      //   start. The stem JAStudio splits off the く of 寄ってく is one, so the old rule found a sentence start mid-sentence. In the corpus that was 25
      //   matches, such as おう on such a う, which its required prefix rejects anyway: the full rebuild changed nothing.
      PrecedingText.Meets(ConditionGroup.AnyOf(TextConditions.IsEmpty,
                                               TextConditions.EndsWithOneOf([.. AnalysisConstants.SentenceStartCharacters.Select(it => it.ToString())])));
}
