using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;
using JAStudio.Core.SysUtils.Json;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A vocab's match-condition as the corpus stores it, a condition on a word, and back. Each condition is an object whose key says what kind it<br/>
/// is, such as <c>{"allOf": [...]}</c>, <c>{"not": {...}}</c>, <c>{"fact": "singleToken"}</c>, <c>{"precedingToken": {"fact": "godanVerb"}}</c> for a<br/>
/// condition on a subject found from the word, or <c>{"sharedCondition": "FollowsAPastTenseStem"}</c> for a shared-condition, used by its name.
/// </summary>
/// <remarks>Reading one finds its shared-conditions by name in the corpus's (<see cref="SharedConditions"/>), so they must be loaded first.</remarks>
public static class MatchConditionJson
{
   static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true, Encoder = FullUnicodeJsonEncoder.Instance };

   /// <summary>The JSON the corpus stores <paramref name="matchCondition"/> as.</summary>
   public static JsonObject Of(Condition<CandidateWordVariant> matchCondition) => matchCondition.ToJson();

   /// <summary>The match-condition <paramref name="json"/> holds, as <see cref="Of"/> writes it, its shared-conditions found by name in <paramref name="sharedConditions"/>.</summary>
   /// <exception cref="FormatException">When <paramref name="json"/> isn't a condition on a word, or uses a shared-condition there's none of.</exception>
   public static Condition<CandidateWordVariant> MatchConditionFrom(JsonObject json, SharedConditions sharedConditions) => WordConditions.Level.ConditionFrom(json, sharedConditions);

   /// <summary>The JSON a vocab stores its <paramref name="matchCondition"/> as in the corpus: none when it has no conditions.</summary>
   public static JsonObject? AsAVocabStoresIt(AllOfGroup<CandidateWordVariant> matchCondition) => matchCondition.IsEmpty ? null : Of(matchCondition);

   /// <summary>
   /// The match-condition of a vocab that <paramref name="json"/> holds, as <see cref="AsAVocabStoresIt"/> writes it: none when it's null, and an "all of",<br/>
   /// as a vocab's match-condition always is. Any other condition is made the only condition in one. Its shared-conditions are found by name in<br/>
   /// <paramref name="sharedConditions"/>.
   /// </summary>
   /// <exception cref="FormatException">When <paramref name="json"/> isn't a condition on a word, or uses a shared-condition there's none of.</exception>
   public static AllOfGroup<CandidateWordVariant> VocabsMatchConditionFrom(JsonObject? json, SharedConditions sharedConditions) =>
      json is null ? ConditionGroup.AllOf<CandidateWordVariant>() : AsAnAllOf(MatchConditionFrom(json, sharedConditions));

   /// <summary>A vocab's <paramref name="matchCondition"/> as indented JSON text, as a person edits it as a whole: empty for none.</summary>
   public static string AsJsonText(AllOfGroup<CandidateWordVariant> matchCondition) => AsAVocabStoresIt(matchCondition)?.ToJsonString(IndentedJson) ?? "";

   /// <summary>
   /// The match-condition of a vocab that the JSON text <paramref name="text"/> holds, as <see cref="AsJsonText"/> writes it: none when it's empty. Its<br/>
   /// shared-conditions are found by name in <paramref name="sharedConditions"/>.
   /// </summary>
   /// <exception cref="FormatException">When it isn't a condition on a word, or uses a shared-condition there's none of.</exception>
   public static AllOfGroup<CandidateWordVariant> VocabsMatchConditionFromJsonText(string text, SharedConditions sharedConditions) =>
      VocabsMatchConditionFrom(text.Trim().Length == 0 ? null : JsonObjectIn(text), sharedConditions);

   static AllOfGroup<CandidateWordVariant> AsAnAllOf(Condition<CandidateWordVariant> condition) => condition as AllOfGroup<CandidateWordVariant> ?? ConditionGroup.AllOf(condition);

   static JsonObject JsonObjectIn(string text) => JsonNode.Parse(text) as JsonObject ?? throw new FormatException($"A match-condition is a JSON object: {text}");
}
