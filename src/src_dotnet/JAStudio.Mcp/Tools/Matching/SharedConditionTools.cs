using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Nodes;
using JAStudio.Core.Batches;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing.Candidates;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Matching;

/// <summary>The tools about the corpus's shared-conditions (<see cref="SharedConditions"/>): conditions with a name, used by name in any vocab's match-condition.</summary>
[McpServerToolType]
public class SharedConditionTools
{
   readonly SharedConditions _sharedConditions;
   readonly JPCollection _collection;
   readonly LocalNoteUpdater _localNoteUpdater;

   public SharedConditionTools(SharedConditions sharedConditions, JPCollection collection, LocalNoteUpdater localNoteUpdater)
   {
      _sharedConditions = sharedConditions;
      _collection = collection;
      _localNoteUpdater = localNoteUpdater;
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

   [McpServerTool(Name = "redefine_a_shared_condition", Destructive = true)]
   [Description("""
                Gives a shared-condition another definition, saves it in its file, and reparses the sentences of every vocab whose match-condition
                depends on it: those that use it, directly or through other shared-conditions' definitions. What those vocab match can change, as
                with any change to matching: run a full rebuild and look at what it changed before committing. The definition is in the JSON
                define_a_shared_condition takes, and can't use the shared-condition itself, directly or through others. Reports the shared-condition
                as listed with its new definition, and the vocab whose sentences were reparsed.
                """)]
   public RedefinedSharedCondition RedefineASharedCondition(
      [Description("What it's about, as code names it in front of its name: Word, Token or Text.")]
      string subject,
      [Description("Its name, such as FollowsAPastTenseStem.")]
      string name,
      [Description("""Its new definition: a condition on the subject, as JSON text, such as {"anyOf": [{"precedingToken": {"fact": "pastTenseStem"}}, ...]}.""")]
      string definition)
   {
      var sharedCondition = SharedConditionNamed(LevelOfTheSubjectNamed(subject), name);
      _sharedConditions.RedefineFrom(sharedCondition, JsonObjectIn(definition));

      var vocabDependingOnIt = _collection.Vocab.WhoseMatchConditionsDependOnTheDefinitionOf(sharedCondition);
      _localNoteUpdater.ReparseSentencesForVocabWithoutProgressDialog(vocabDependingOnIt);

      return new RedefinedSharedCondition(Listed(sharedCondition), [.. vocabDependingOnIt.Select(it => it.Question.DisambiguationName).Order(StringComparer.Ordinal)]);
   }

   [McpServerTool(Name = "rename_a_shared_condition", Destructive = false)]
   [Description("""
                Renames a shared-condition, with its file in the corpus checkout's folder shared-conditions, and saves every vocab and shared-condition
                that uses it, since the corpus stores the shared-conditions a condition uses by name. What each means is unchanged, so no vocab matches
                other words: a full rebuild after it changes nothing more. Fails when the new name can't name one on its subject: not a name in code, a
                fact's, a subject's or a test's name there, or another shared-condition's. Reports the shared-condition as listed by its new name, with
                what uses it.
                """)]
   public ListedSharedCondition RenameASharedCondition(
      [Description("What it's about, as code names it in front of its name: Word, Token or Text.")]
      string subject,
      [Description("Its name now, such as FollowsAPastTenseStem.")]
      string name,
      [Description("Its new name in code: a capital letter, then letters and digits.")]
      string newName)
   {
      var sharedCondition = SharedConditionNamed(LevelOfTheSubjectNamed(subject), name);
      _sharedConditions.Rename(sharedCondition, newName, _collection.Vocab.All());
      return Listed(sharedCondition);
   }

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

   [McpServerTool(Name = "find_the_conditions_vocab_share", ReadOnly = true, Idempotent = true)]
   [Description("""
                Finds the conditions that the match-conditions of several vocab hold, each a candidate for a shared-condition: a condition in their
                trees, or a combination of some of the conditions of a group, such as two conditions several match-conditions require among others.
                Conditions are compared by what they mean, so the order of a group's conditions doesn't matter. Left out: what's a name already, a
                fact or a shared-condition, on a subject or negated, such as !PrecedingToken.IsAnIchidanVerb; a negation, since naming what it
                negates names it too, and what it negates counts as held by the vocab holding the negation; and a condition only ever found within
                a larger one held by the same vocab. Those held by the most vocab come first, then the largest. Each comes with its subject, its code,
                its definition as define_a_shared_condition takes it, and the vocab holding it; one whose meaning a shared-condition has already is
                named so, to be used with use_a_shared_condition_wherever_its_definition_is instead. It proposes; a mind decides: a shared-condition is
                mostly its name, and vocab holding the same condition may hold it for different reasons, which one shared-condition would tie together.
                """)]
   public FoundSharedConditionCandidates FindTheConditionsVocabShare(
      [Description("The fewest vocab whose match-conditions must hold a condition for it to be found.")]
      int heldByAtLeast = 3,
      [Description("The most conditions to list. Every one found is counted either way.")]
      int maxListed = 30,
      [Description("The most vocab to list for each condition. Every one holding it is counted either way.")]
      int maxVocabListedForEach = 10)
   {
      var found = SharedConditionCandidates.FoundIn(_collection.Vocab.All(), heldByAtLeast);
      return new FoundSharedConditionCandidates(found.Count, [.. found.Take(maxListed).Select(it => ListedSharedConditionCandidate.Of(it, _sharedConditions, maxVocabListedForEach))]);
   }

   ListedSharedCondition Listed(ISharedCondition sharedCondition) =>
      new(sharedCondition.FullName, ConditionCode.AsText(ConditionCode.LinesOfASharedCondition(sharedCondition)), TheVocabUsing(sharedCondition), TheSharedConditionsUsing(sharedCondition));

   /// <summary>The questions of the vocab whose match-condition uses <paramref name="sharedCondition"/>, each with its disambiguation, in ordinal order.</summary>
   IReadOnlyList<string> TheVocabUsing(ISharedCondition sharedCondition) =>
      [.. _collection.Vocab.All().Where(vocab => vocab.MatchingConfiguration.MatchCondition.Uses(sharedCondition)).Select(it => it.Question.DisambiguationName).Order(StringComparer.Ordinal)];

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
