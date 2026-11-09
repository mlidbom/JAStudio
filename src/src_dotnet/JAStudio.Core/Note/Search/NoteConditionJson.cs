using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using JAStudio.Core.Note.Search.Grouping;
using JAStudio.Core.Note.Search.NoteTypeProperties;
using JAStudio.Core.Note.Search.TagProperties;
using JAStudio.Core.Note.Search.TextProperties;
using JAStudio.Core.Note.Search.TrueOrFalseProperties;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.Search;

/// <summary>
/// Writes a <see cref="NoteCondition"/>, a whole search, as JSON, and reads it back, so that a search can be kept:<br/>
/// the workspace's, between sessions. Each condition is an object whose key says its kind, such as <c>{"allOf": [...]}</c>,<br/>
/// <c>{"not": {...}}</c>, <c>{"text": "Question", "contains": "家"}</c> or <c>{"tagsInclude": "source::anime"}</c>.
/// </summary>
public static class NoteConditionJson
{
   public static JsonObject Of(NoteCondition condition) => condition switch
   {
      AllOfGroup group => new JsonObject { ["allOf"] = ArrayOf(group.Conditions) },
      AnyOfGroup group => new JsonObject { ["anyOf"] = ArrayOf(group.Conditions) },
      NegatedCondition negated => new JsonObject { ["not"] = Of(negated.Condition) },
      TextCondition text => TextConditionOf(text),
      TagCondition tag => new JsonObject { ["tagsInclude"] = tag.TagPath },
      TrueOrFalseCondition trueOrFalse => new JsonObject { ["trueOrFalse"] = trueOrFalse.Property.Name, ["is"] = trueOrFalse.Value },
      NoteTypeCondition noteType => new JsonObject { ["noteTypeIs"] = NoteTypeNameOf(noteType.NoteType) },
      _ => throw new ArgumentOutOfRangeException(nameof(condition), condition.GetType(), "A kind of condition with no JSON form.")
   };

   public static NoteCondition ConditionFrom(JsonObject json) => KindOf(json) switch
   {
      "allOf" => ConditionGroup.AllOf([.. ConditionsFrom(json["allOf"])]),
      "anyOf" => ConditionGroup.AnyOf([.. ConditionsFrom(json["anyOf"])]),
      "not" => ConditionGroup.Not(ConditionFrom(ObjectIn(json, "not"))),
      "text" => TextConditionFrom(json),
      "tagsInclude" => NoteProperties.Tags.Include(StringIn(json, "tagsInclude")),
      "trueOrFalse" => NoteProperties.EveryTrueOrFalseProperty.Single(it => it.Name == StringIn(json, "trueOrFalse")).Is(BoolIn(json, "is")),
      "noteTypeIs" => NoteTypeConditionFor(StringIn(json, "noteTypeIs")),
      var kind => throw new FormatException($"A condition of the unknown kind {kind}: {json.ToJsonString()}")
   };

   static JsonArray ArrayOf(IEnumerable<NoteCondition> conditions) => [.. conditions.Select(JsonNode (it) => Of(it))];

   static IEnumerable<NoteCondition> ConditionsFrom(JsonNode? array) =>
      (array as JsonArray ?? throw new FormatException("A group of conditions without its array of conditions."))
     .Select(it => ConditionFrom(it as JsonObject ?? throw new FormatException("A condition that isn't a JSON object.")));

   static JsonObject TextConditionOf(TextCondition text)
   {
      var json = new JsonObject { ["text"] = text.Property.Name };
      switch(text.Match)
      {
         case TextContains contains: json["contains"] = contains.Text; break;
         case TextIsExactly isExactly: json["is"] = isExactly.Text; break;
         case TextMatchesWildcards wildcards: json["matchesWildcards"] = wildcards.Pattern; break;
         case TextMatchesRegex regex: json["matchesRegex"] = regex.Pattern; break;
         default: throw new ArgumentOutOfRangeException(nameof(text), text.Match.GetType(), "A kind of text match with no JSON form.");
      }

      return json;
   }

   static TextCondition TextConditionFrom(JsonObject json)
   {
      var property = NoteProperties.EveryTextProperty.Single(it => it.Name == StringIn(json, "text"));
      if(json.ContainsKey("contains")) return property.Contains(StringIn(json, "contains"));
      if(json.ContainsKey("is")) return property.Is(StringIn(json, "is"));
      if(json.ContainsKey("matchesWildcards")) return property.MatchesWildcards(StringIn(json, "matchesWildcards"));
      if(json.ContainsKey("matchesRegex")) return property.MatchesRegex(StringIn(json, "matchesRegex"));
      throw new FormatException($"A text condition without a match: {json.ToJsonString()}");
   }

   /// <summary>The name a search's JSON gives the note type <paramref name="noteType"/>: <c>vocab</c>, <c>kanji</c> or <c>sentence</c>.</summary>
   public static string NoteTypeNameOf(Type noteType) =>
      noteType == typeof(VocabNote) ? "vocab"
      : noteType == typeof(KanjiNote) ? "kanji"
      : noteType == typeof(SentenceNote) ? "sentence"
      : throw new ArgumentOutOfRangeException(nameof(noteType), noteType, "Not a note type a search can have.");

   static NoteTypeCondition NoteTypeConditionFor(string noteTypeName) => noteTypeName switch
   {
      "vocab" => NoteProperties.NoteType.Is<VocabNote>(),
      "kanji" => NoteProperties.NoteType.Is<KanjiNote>(),
      "sentence" => NoteProperties.NoteType.Is<SentenceNote>(),
      _ => throw new FormatException($"The unknown note type {noteTypeName}.")
   };

   /// <summary>The key that says what kind of condition <paramref name="json"/> is: its first.</summary>
   static string KindOf(JsonObject json) => json.FirstOrDefault().Key ?? throw new FormatException("An empty JSON object, which is no condition.");

   static string StringIn(JsonObject json, string key) => json[key]?.GetValue<string>() ?? throw new FormatException($"A condition without its {key}: {json.ToJsonString()}");

   static bool BoolIn(JsonObject json, string key) => json[key]?.GetValue<bool>() ?? throw new FormatException($"A condition without its {key}: {json.ToJsonString()}");

   static JsonObject ObjectIn(JsonObject json, string key) => json[key] as JsonObject ?? throw new FormatException($"A condition without its {key}: {json.ToJsonString()}");
}
