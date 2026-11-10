using System;
using System.Text.Json.Nodes;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A vocab's match-condition as the corpus stores it, a condition on a word, and back. Each condition is an object whose key says what kind it<br/>
/// is, such as <c>{"allOf": [...]}</c>, <c>{"not": {...}}</c>, <c>{"fact": "singleToken"}</c>, or <c>{"precedingToken": {"fact": "godanVerb"}}</c> for a<br/>
/// condition on a subject found from the word.
/// </summary>
public static class MatchConditionJson
{
   /// <summary>The JSON the corpus stores <paramref name="matchCondition"/> as.</summary>
   public static JsonObject Of(Condition<CandidateWordVariant> matchCondition) => matchCondition.ToJson();

   /// <summary>The match-condition <paramref name="json"/> holds, as <see cref="Of"/> writes it.</summary>
   /// <exception cref="FormatException">When <paramref name="json"/> isn't a condition on a word.</exception>
   public static Condition<CandidateWordVariant> MatchConditionFrom(JsonObject json) => WordConditions.Level.ConditionFrom(json);
}
