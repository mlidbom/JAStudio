using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// One level of a match-condition's tree: the conditions there are on one kind of subject, a word, a token or a text. It knows their facts, the<br/>
/// subjects found from it, and the other conditions on it, and reads each back from the JSON that <see cref="Condition{TSubject}"/> writes.
/// </summary>
public sealed class ConditionLevel<TSubject> where TSubject : class
{
   //review:matching: "level", after the tree's three levels in the design document: word, token, text. What it holds is more a catalog of
   //   the conditions there are on one kind of subject.
   readonly string _subjectName;
   readonly Dictionary<string, Fact<TSubject>> _factsByJsonName;
   readonly Dictionary<string, IConditionSubject<TSubject>> _subjectsByJsonKey;
   readonly IReadOnlyDictionary<string, Func<JsonNode, Condition<TSubject>>> _otherConditionsByJsonKey;

   /// <param name="subjectName">What the conditions are on, such as "a token", for the errors reading them.</param>
   /// <param name="facts">The <see cref="Facts"/>.</param>
   /// <param name="subjects">The subjects found from this level's subject, each with the conditions on it.</param>
   /// <param name="otherConditionsByJsonKey">Reads the other conditions, each from the value under its key, such as <c>partOfSpeech</c>.</param>
   internal ConditionLevel(string subjectName,
                           IEnumerable<Fact<TSubject>> facts,
                           IEnumerable<IConditionSubject<TSubject>> subjects,
                           IReadOnlyDictionary<string, Func<JsonNode, Condition<TSubject>>> otherConditionsByJsonKey)
   {
      _subjectName = subjectName;
      Facts = [.. facts];
      _factsByJsonName = Facts.ToDictionary(it => it.JsonName);
      _subjectsByJsonKey = subjects.ToDictionary(it => it.JsonKey);
      _otherConditionsByJsonKey = otherConditionsByJsonKey;
   }

   /// <summary>The facts about this level's subject, such as a token being a godan verb.</summary>
   public IReadOnlyList<Fact<TSubject>> Facts { get; }

   /// <summary>The condition <paramref name="json"/> holds, as <see cref="Condition{TSubject}"/> writes it.</summary>
   /// <exception cref="FormatException">When it isn't a condition on this level's subject.</exception>
   internal Condition<TSubject> ConditionFrom(JsonObject json)
   {
      if(json.Count != 1) throw new FormatException($"A condition on {_subjectName} is an object with one key, which says what kind of condition it is: {json.ToJsonString()}");
      var kind = json.First().Key;
      var value = json[kind] ?? throw new FormatException($"A condition on {_subjectName} without its value: {json.ToJsonString()}");
      return kind switch
      {
         _ when kind == AllOfGroup<TSubject>.JsonKey => ConditionGroup.AllOf([.. ConditionsIn(value)]),
         _ when kind == AnyOfGroup<TSubject>.JsonKey => ConditionGroup.AnyOf([.. ConditionsIn(value)]),
         _ when kind == NegatedCondition<TSubject>.JsonKey => ConditionGroup.Not(ConditionFrom(ObjectIn(value))),
         _ when kind == Fact<TSubject>.JsonKey => FactNamed(value.GetValue<string>()),
         _ when _subjectsByJsonKey.TryGetValue(kind, out var subject) => subject.ConditionFrom(ObjectIn(value)),
         _ when _otherConditionsByJsonKey.TryGetValue(kind, out var read) => read(value),
         _ => throw new FormatException($"A condition of a kind there is none of on {_subjectName}: {json.ToJsonString()}")
      };

      IEnumerable<Condition<TSubject>> ConditionsIn(JsonNode array) =>
         (array as JsonArray ?? throw new FormatException($"A group of conditions on {_subjectName} without its array: {json.ToJsonString()}"))
        .Select(it => ConditionFrom(ObjectIn(it)));

      JsonObject ObjectIn(JsonNode? node) => node as JsonObject ?? throw new FormatException($"A condition on {_subjectName} that isn't a JSON object: {json.ToJsonString()}");

      Fact<TSubject> FactNamed(string name) =>
         _factsByJsonName.TryGetValue(name, out var fact) ? fact : throw new FormatException($"The fact {name}, which there is none of about {_subjectName}.");
   }
}
