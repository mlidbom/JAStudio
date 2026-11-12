using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Nodes;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Matching;

/// <summary>The tools about the corpus's shared-conditions (<see cref="SharedConditions"/>): conditions with a name, used by name in any vocab's match-condition.</summary>
[McpServerToolType]
public class SharedConditionTools
{
   readonly SharedConditions _sharedConditions;
   readonly JPCollection _collection;

   public SharedConditionTools(SharedConditions sharedConditions, JPCollection collection)
   {
      _sharedConditions = sharedConditions;
      _collection = collection;
   }

   [McpServerTool(Name = "list_the_shared_conditions", ReadOnly = true, Idempotent = true)]
   [Description("""
                Lists the corpus's shared-conditions: conditions with a name, defined once in the corpus checkout's folder shared-conditions and used
                by name in any vocab's match-condition, as a fact is. Each comes with its definition as code, as the match-condition editor shows it,
                such as "Word.FollowsAPastTenseStem =" then its definition under it, and with the vocab and the shared-conditions that use it.
                Those on a word come first, then a token's, then a text's.
                """)]
   public IReadOnlyList<ListedSharedCondition> ListTheSharedConditions() => [.. _sharedConditions.All.Select(Listed)];

   [McpServerTool(Name = "define_a_shared_condition", Destructive = false)]
   [Description("""
                Defines a new shared-condition and saves it in its own file in the corpus checkout's folder shared-conditions, named by its full
                name, such as Word.FollowsAPastTenseStem.json. It changes no note: a vocab uses it once its match-condition names it. Its definition
                is a condition on its subject as the corpus stores conditions, in the same JSON as a vocab's match-condition ("matching.condition"
                in get_the_editable_fields_of_a_note), such as {"precedingToken": {"fact": "pastTenseStem"}} on a word, and may use other
                shared-conditions by name, as {"sharedCondition": "IsAdverbLike"}. Fails when the name is taken on that subject or is a fact's, a
                subject's or a test's name there, or the definition isn't a condition on that subject.
                """)]
   public ListedSharedCondition DefineASharedCondition(
      [Description("What it's about, as code names it in front of its name: Word, Token or Text. One on a token is offered under any token, such as PrecedingToken.")]
      string subject,
      [Description("Its name in code: a capital letter, then letters and digits, such as FollowsAPastTenseStem.")]
      string name,
      [Description("""Its definition: a condition on the subject, as JSON text, such as {"anyOf": [{"precedingToken": {"fact": "pastTenseStem"}}, ...]}.""")]
      string definition) =>
      Listed(_sharedConditions.DefineFrom(LevelOfTheSubjectNamed(subject), name, JsonObjectIn(definition)));

   [McpServerTool(Name = "use_a_shared_condition_wherever_its_definition_is", Destructive = false)]
   [Description("""
                Uses a shared-condition by name wherever its definition is: in every vocab's match-condition, and in the definitions of the other
                shared-conditions, saving each vocab and shared-condition it changes. Each condition that means the same as the definition is replaced
                by the shared-condition, whatever the order of the conditions in its groups. When the definition is a group of two or more conditions,
                such as AllOf(A, B), a group of the same kind that holds them among others has them replaced by it: AllOf(A, B, C) becomes
                AllOf(Shared, C). What it changes means the same as before, so no vocab matches other words, or weighs other than it did: a full
                rebuild after it changes nothing more. Reports the shared-condition as listed, and the vocab and shared-conditions it made use it.
                """)]
   public SharedConditionPutToUse UseASharedConditionWhereverItsDefinitionIs(
      [Description("What it's about, as code names it in front of its name: Word, Token or Text.")]
      string subject,
      [Description("Its name, such as FollowsAPastTenseStem.")]
      string name)
   {
      var sharedCondition = SharedConditionNamed(LevelOfTheSubjectNamed(subject), name);
      var listedBefore = Listed(sharedCondition);

      _sharedConditions.UseWhereverItsDefinitionIs(sharedCondition, _collection.Vocab.All());

      return SharedConditionPutToUse.Between(before: listedBefore, after: Listed(sharedCondition));
   }

   ListedSharedCondition Listed(ISharedCondition sharedCondition) =>
      new(sharedCondition.FullName, ConditionCode.AsText(ConditionCode.LinesOfASharedCondition(sharedCondition)), TheVocabUsing(sharedCondition), TheSharedConditionsUsing(sharedCondition));

   /// <summary>The questions of the vocab whose match-condition uses <paramref name="sharedCondition"/>, in ordinal order.</summary>
   IReadOnlyList<string> TheVocabUsing(ISharedCondition sharedCondition) =>
      [.. _collection.Vocab.All().Where(vocab => vocab.MatchingConfiguration.MatchCondition.Uses(sharedCondition)).Select(it => it.GetQuestion()).Order(StringComparer.Ordinal)];

   /// <summary>The full names of the shared-conditions whose definition uses <paramref name="sharedCondition"/>, as they're listed.</summary>
   IReadOnlyList<string> TheSharedConditionsUsing(ISharedCondition sharedCondition) =>
      [.. _sharedConditions.All.Where(other => other.Definition.Uses(sharedCondition)).Select(it => it.FullName)];

   ISharedCondition SharedConditionNamed(IConditionLevel level, string name) =>
      _sharedConditions.On(level).SingleOrDefault(it => it.Name == name)
      ?? throw new ArgumentException($"There's no shared-condition {level.SubjectNameInCode}.{name}: there are {string.Join(", ", _sharedConditions.All.Select(it => it.FullName))}.");

   static IConditionLevel LevelOfTheSubjectNamed(string subject) =>
      SharedConditions.LevelOfTheSubjectNamed(subject)
      ?? throw new ArgumentException($"A shared-condition is on one of {string.Join(", ", SharedConditions.SubjectsTheyCanBeOn.Select(it => it.SubjectNameInCode))}, not on {subject}.");

   static JsonObject JsonObjectIn(string definition) => JsonNode.Parse(definition) as JsonObject ?? throw new ArgumentException($"A definition is a JSON object: {definition}");
}
