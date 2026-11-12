using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;
using static Compze.Must.MustActions;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>A vocab's match-condition, as the corpus stores it, is read back as the same match-condition.</summary>
public class When_keeping_a_match_condition_as_JSON : IAIGeneratedSpec
{
   /// <summary><paramref name="matchCondition"/> written as JSON text, as the corpus keeps it, and read back from that text.</summary>
   static Condition<CandidateWordVariant> ReadBack(Condition<CandidateWordVariant> matchCondition) =>
      MatchConditionJson.MatchConditionFrom(Parsed(MatchConditionJson.Of(matchCondition).ToJsonString()));

   static JsonObject Parsed(string json) => JsonNode.Parse(json) as JsonObject ?? throw new InvalidOperationException("The JSON isn't an object.");

   public class with_every_kind_of_condition : When_keeping_a_match_condition_as_JSON
   {
      static readonly Condition<CandidateWordVariant> MatchCondition =
         ConditionGroup.AllOf(
            IsASingleToken,
            ConditionGroup.Not(IsMatchedByItsSurface),
            ConditionGroup.AnyOf(IsMatchedByAnInflectedSurface),
            PrecedingToken.Meets(ConditionGroup.AllOf(
                                    TokenConditions.IsAGodanVerb,
                                    TokenConditions.PartOfSpeech.Is("動詞", "自立"),
                                    TokenConditions.InflectionType.In("五段・ラ行", "一段"),
                                    TokenConditions.InflectedForm.In("連用タ接続"),
                                    TokenConditions.Surface.Meets(TextConditions.EndsWithOneOf("ん")),
                                    TokenConditions.BaseForm.Meets(TextConditions.In("する")),
                                    TokenConditions.Reading.Meets(TextConditions.ContainsOneOf("ス")))),
            FirstToken.Meets(TokenConditions.HasATeFormStem),
            LastToken.Meets(TokenConditions.EndsAStatement),
            FollowingToken.Meets(ConditionGroup.Not(TokenConditions.IsANonWordCharacter)),
            PrecedingText.Meets(TextConditions.IsEmpty),
            FollowingText.Meets(TextConditions.StartsWithOneOf("湧", "わ")),
            Surface.Meets(TextConditions.MatchesRegularExpression("^本の$")),
            BaseForm.Meets(TextConditions.In("本の")));

      [XF] public void it_is_read_back_as_the_same_match_condition() => ReadBack(MatchCondition).Must().Be(MatchCondition);
   }

   public class with_a_condition_on_the_preceding_token_and_a_negated_one : When_keeping_a_match_condition_as_JSON
   {
      readonly JsonObject _json = MatchConditionJson.Of(ConditionGroup.AllOf(
                                                           PrecedingToken.Meets(TokenConditions.IsInAnIrrealisForm),
                                                           ConditionGroup.Not(PrecedingToken.Meets(TokenConditions.IsAnIchidanVerb)),
                                                           PrecedingToken.Meets(TokenConditions.Surface.Meets(TextConditions.EndsWithOneOf("ん")))));

      [XF] public void each_condition_is_an_object_whose_key_says_what_kind_of_condition_it_is() =>
         JsonNode.DeepEquals(_json,
                             Parsed("""
                                    {"allOf": [
                                       {"precedingToken": {"fact": "irrealis"}},
                                       {"not": {"precedingToken": {"fact": "ichidanVerb"}}},
                                       {"precedingToken": {"surface": {"endsWithOneOf": ["ん"]}}}
                                    ]}
                                    """))
                 .Must().BeTrue();
   }

   [XF] public void every_fact_about_a_word_is_read_back_as_itself() => FactsNotReadBackAsThemselves<CandidateWordVariant>(typeof(WordConditions), it => it).Must().Be("");

   [XF] public void every_fact_about_a_token_is_read_back_as_itself() => FactsNotReadBackAsThemselves<IAnalysisToken>(typeof(TokenConditions), FirstToken.Meets).Must().Be("");

   [XF] public void every_fact_about_a_text_is_read_back_as_itself() => FactsNotReadBackAsThemselves<string>(typeof(TextConditions), FollowingText.Meets).Must().Be("");

   [XF] public void reading_a_condition_that_names_a_fact_there_is_none_of_throws() =>
      Invoking(() => MatchConditionJson.MatchConditionFrom(Parsed("""{"precedingToken": {"fact": "noSuchFact"}}"""))).Must().Throw<FormatException>();

   [XF] public void reading_a_condition_on_a_token_where_a_condition_on_the_word_belongs_throws() =>
      Invoking(() => MatchConditionJson.MatchConditionFrom(Parsed("""{"partOfSpeech": "名詞"}"""))).Must().Throw<FormatException>();

   [XF] public void reading_a_condition_object_with_more_than_one_key_throws() =>
      Invoking(() => MatchConditionJson.MatchConditionFrom(Parsed("""{"fact": "singleToken", "not": {"fact": "matchedByItsSurface"}}"""))).Must().Throw<FormatException>();

   /// <summary>
   /// The names of the facts <paramref name="facts"/> defines that aren't read back as themselves, each put in a match-condition by<br/>
   /// <paramref name="inAMatchCondition"/>: a fact left out of its level's list can't be read back from the corpus.
   /// </summary>
   static string FactsNotReadBackAsThemselves<TSubject>(Type facts, Func<Fact<TSubject>, Condition<CandidateWordVariant>> inAMatchCondition) where TSubject : class =>
      string.Join(", ",
                  facts.GetFields(BindingFlags.Public | BindingFlags.Static)
                       .Select(it => it.GetValue(null))
                       .OfType<Fact<TSubject>>()
                       .Where(it => !ReadBack(inAMatchCondition(it)).Equals(inAMatchCondition(it)))
                       .Select(it => it.JsonName));
}
