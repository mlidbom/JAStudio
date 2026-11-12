using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// One level of a match-condition's tree: the conditions there are on one kind of subject, a word, a token or a text. It knows their facts, the<br/>
/// subjects found from it, and the other conditions on it, and reads each back from the JSON that <see cref="Condition{TSubject}"/> writes.
/// </summary>
public sealed partial class ConditionLevel<TSubject> : IConditionLevel where TSubject : class
{
   //review:matching: "level", after the tree's three levels in the design document: word, token, text. What it holds is more a catalog of
   //   the conditions there are on one kind of subject.
   readonly Dictionary<string, Fact<TSubject>> _factsByJsonName;
   readonly Dictionary<string, IConditionSubject<TSubject>> _subjectsByJsonKey;
   readonly IReadOnlyDictionary<string, Func<JsonNode, Condition<TSubject>>> _otherConditionsByJsonKey;

   /// <param name="subjectName">The <see cref="SubjectName"/>.</param>
   /// <param name="subjectNameInCode">The <see cref="SubjectNameInCode"/>.</param>
   /// <param name="facts">The <see cref="Facts"/>.</param>
   /// <param name="subjects">The subjects found from this level's subject, each with the conditions on it.</param>
   /// <param name="testsWithTexts">The <see cref="TestsWithTexts"/>.</param>
   /// <param name="otherConditionsByJsonKey">Reads the other conditions, each from the value under its key, such as <c>partOfSpeech</c>.</param>
   internal ConditionLevel(string subjectName,
                           string subjectNameInCode,
                           IEnumerable<Fact<TSubject>> facts,
                           IEnumerable<IConditionSubject<TSubject>> subjects,
                           IEnumerable<ConditionTestWithTexts> testsWithTexts,
                           IReadOnlyDictionary<string, Func<JsonNode, Condition<TSubject>>> otherConditionsByJsonKey)
   {
      //review:matching: a test with texts and the reading of its conditions from JSON (otherConditionsByJsonKey) are listed apart, each kind of
      //   condition once in each. Each test could know its JSON key and read its conditions, and the dictionary go.
      SubjectName = subjectName;
      SubjectNameInCode = subjectNameInCode;
      Facts = [.. facts];
      _factsByJsonName = Facts.ToDictionary(it => it.JsonName);
      Subjects = [.. subjects];
      _subjectsByJsonKey = Subjects.ToDictionary(it => it.JsonKey);
      TestsWithTexts = [.. testsWithTexts];
      _otherConditionsByJsonKey = otherConditionsByJsonKey;
   }

   public string SubjectName { get; }

   public string SubjectNameInCode { get; }

   /// <summary>The facts about this level's subject, such as a token being a godan verb.</summary>
   public IReadOnlyList<Fact<TSubject>> Facts { get; }

   IReadOnlyList<Condition> IConditionLevel.Facts => Facts;

   /// <summary>The subjects found from this level's subject, such as a word's preceding token, each with the conditions on it.</summary>
   IReadOnlyList<IConditionSubject<TSubject>> Subjects { get; }

   IReadOnlyList<IConditionSubject> IConditionLevel.Subjects => Subjects;

   public IReadOnlyList<ConditionTestWithTexts> TestsWithTexts { get; }

   public Condition NewAllOfGroup() => ConditionGroup.AllOf<TSubject>();

   public Condition NewAnyOfGroup() => ConditionGroup.AnyOf<TSubject>();

   ISharedCondition IConditionLevel.NewSharedCondition(string name) => new SharedCondition<TSubject>(name, this);

   string? IConditionLevel.WhyItCannotNameASharedCondition(string name) =>
      !NameInCode().IsMatch(name) ? $"A shared-condition's name is a name in code, a capital letter then letters and digits, such as FollowsAPastTenseStem: {name}"
      : NamesOfTheOtherConditions().Contains(name) ? $"{name} names one of the other conditions on {SubjectName} already."
      : null;

   IEnumerable<string> NamesOfTheOtherConditions() =>
      [.. Facts.Select(it => it.Name), .. Subjects.Select(it => it.Name), .. TestsWithTexts.Select(it => it.Name), ConditionGroup.AllOf<TSubject>().Name, ConditionGroup.AnyOf<TSubject>().Name];

   [GeneratedRegex("^[A-Z][A-Za-z0-9]*$")]
   private static partial Regex NameInCode();

   /// <summary>
   /// The condition <paramref name="json"/> holds, as <see cref="Condition{TSubject}"/> writes it, its shared-conditions found by name in<br/>
   /// <paramref name="sharedConditions"/>.
   /// </summary>
   /// <exception cref="FormatException">When it isn't a condition on this level's subject, or uses a shared-condition there's none of.</exception>
   internal Condition<TSubject> ConditionFrom(JsonObject json, SharedConditions sharedConditions)
   {
      if(json.Count != 1) throw new FormatException($"A condition on {SubjectName} is an object with one key, which says what kind of condition it is: {json.ToJsonString()}");
      var kind = json.First().Key;
      var value = json[kind] ?? throw new FormatException($"A condition on {SubjectName} without its value: {json.ToJsonString()}");
      return kind switch
      {
         _ when kind == AllOfGroup<TSubject>.JsonKey => ConditionGroup.AllOf([.. ConditionsIn(value)]),
         _ when kind == AnyOfGroup<TSubject>.JsonKey => ConditionGroup.AnyOf([.. ConditionsIn(value)]),
         _ when kind == NegatedCondition<TSubject>.JsonKey => ConditionGroup.Not(ConditionFrom(ObjectIn(value), sharedConditions)),
         _ when kind == Fact<TSubject>.JsonKey => FactNamed(value.GetValue<string>()),
         _ when kind == SharedCondition<TSubject>.JsonKey => sharedConditions.Named(this, value.GetValue<string>()),
         _ when _subjectsByJsonKey.TryGetValue(kind, out var subject) => subject.ConditionFrom(ObjectIn(value), sharedConditions),
         _ when _otherConditionsByJsonKey.TryGetValue(kind, out var read) => read(value),
         _ => throw new FormatException($"A condition of a kind there is none of on {SubjectName}: {json.ToJsonString()}")
      };

      IEnumerable<Condition<TSubject>> ConditionsIn(JsonNode array) =>
         (array as JsonArray ?? throw new FormatException($"A group of conditions on {SubjectName} without its array: {json.ToJsonString()}"))
        .Select(it => ConditionFrom(ObjectIn(it), sharedConditions));

      JsonObject ObjectIn(JsonNode? node) => node as JsonObject ?? throw new FormatException($"A condition on {SubjectName} that isn't a JSON object: {json.ToJsonString()}");

      Fact<TSubject> FactNamed(string name) =>
         _factsByJsonName.TryGetValue(name, out var fact) ? fact : throw new FormatException($"The fact {name}, which there is none of about {SubjectName}.");
   }
}
