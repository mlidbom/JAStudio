using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>
/// A vocab's matching configuration: everything configured on the vocab that decides which words in sentences match it, which of its valid matches<br/>
/// are displayed, and which of two vocab matching the same word wins.
/// </summary>
public class VocabNoteMatchingConfiguration
{
   readonly NoteGuard _guard;
   readonly SharedConditions _sharedConditions;

   /// <param name="partsOfSpeech">The vocab's parts of speech, which some rules and flags take into account.</param>
   /// <param name="data">The configuration as the corpus stores it, or null for a new vocab.</param>
   /// <param name="sharedConditions">The corpus's shared-conditions, which its match-condition uses by name.</param>
   /// <param name="guard">Saves the vocab after a change.</param>
   internal VocabNoteMatchingConfiguration(VocabNotePartsOfSpeech partsOfSpeech, VocabMatchingDto? data, SharedConditions sharedConditions, NoteGuard guard)
   {
      _guard = guard;
      _sharedConditions = sharedConditions;
      MatchCondition = MatchConditionJson.VocabsMatchConditionFrom(data?.Condition, sharedConditions);
      RequiresForbids = new VocabMatchingRequireForbidFlags(partsOfSpeech, data?.RequiredOrForbidden ?? new Dictionary<string, RequiredOrForbiddenDto>(), guard);
      Flags = new VocabMatchingFlags(partsOfSpeech, data?.Flags ?? [], guard);
      YieldToSurface = new VocabYieldToSurface(data?.YieldToSurface ?? [], guard);
   }

   /// <summary>The condition a word must meet to match the vocab: all of its conditions, none when it has none.</summary>
   public AllOfGroup<CandidateWordVariant> MatchCondition { get; private set; }

   /// <summary>The require/forbid rules that aren't conditions in the <see cref="MatchCondition"/>: dictionary form (start), and yielding the last token.</summary>
   public VocabMatchingRequireForbidFlags RequiresForbids { get; }

   /// <summary>
   /// Whether one of the <see cref="MatchCondition"/>'s conditions is <paramref name="rule"/>'s: means the same as one of its conditions, as the conditions<br/>
   /// of the vocab migrated from the rule do. The tags below <see cref="Tagging.Tags.Vocab.Matching.Uses"/> say which vocab hold the rules that take texts.
   /// </summary>
   public bool MatchConditionHolds(MatchingRule rule) => MatchCondition.Conditions.Any(rule.Recognizes);

   /// <summary>Whether one of the <see cref="MatchCondition"/>'s conditions is <paramref name="rule"/>'s condition while required (<see cref="RequireForbidMatchingRule.WhenRequired"/>).</summary>
   public bool MatchConditionRequires(RequireForbidMatchingRule rule) => MatchCondition.Conditions.Any(it => rule.ChoiceOf(it) == RequireForbidChoice.Required);

   public VocabMatchingFlags Flags { get; }

   public VocabYieldToSurface YieldToSurface { get; }

   /// <summary>
   /// Makes <paramref name="matchCondition"/> the vocab's <see cref="MatchCondition"/>, and saves the vocab. Each shared-condition it uses must be one of the<br/>
   /// corpus's (<see cref="SharedConditions.AssertHoldsEachUsedBy"/>).
   /// </summary>
   public void SetMatchCondition(AllOfGroup<CandidateWordVariant> matchCondition)
   {
      _sharedConditions.AssertHoldsEachUsedBy(matchCondition);
      _guard.Update(() => MatchCondition = matchCondition);
   }

   /// <summary>
   /// Writes <paramref name="sharedCondition"/>'s definition out in the <see cref="MatchCondition"/> wherever it uses it (<see cref="Condition.WithTheDefinitionWrittenOutOf"/>),<br/>
   /// as when the shared-condition is deleted, and saves the vocab when that changes it. It then means the same, so the vocab matches the same words.
   /// </summary>
   internal void WriteOutTheDefinitionOf(ISharedCondition sharedCondition) => SetMatchConditionWhenItIsAnother(MatchCondition.WithTheDefinitionWrittenOutOf(sharedCondition));

   /// <summary>
   /// Saves the vocab after a shared-condition its <see cref="MatchCondition"/> uses was renamed (<see cref="SharedConditions.Rename"/>): the match-condition<br/>
   /// holds the shared-condition itself, so it's unchanged, but the corpus stores it by the shared-condition's name.
   /// </summary>
   internal void SaveAfterASharedConditionItUsesWasRenamed() => SetMatchCondition(MatchCondition);

   /// <summary>
   /// Uses <paramref name="sharedCondition"/> by name in the <see cref="MatchCondition"/> wherever its definition is (<see cref="Condition.WithSharedConditionUsed"/>),<br/>
   /// and saves the vocab when that changes it. It then means the same, so the vocab matches the same words, and weighs the same.
   /// </summary>
   public void UseWhereverItsDefinitionIs(ISharedCondition sharedCondition) => SetMatchConditionWhenItIsAnother(MatchCondition.WithSharedConditionUsed(sharedCondition));

   /// <summary>
   /// Makes <paramref name="condition"/>, a condition on a word, the vocab's <see cref="MatchCondition"/> (<see cref="AsAMatchCondition"/>), and saves the<br/>
   /// vocab, when it's another than the one it has: a vocab that a change elsewhere, such as to a shared-condition, leaves as it was isn't saved.
   /// </summary>
   void SetMatchConditionWhenItIsAnother(Condition condition)
   {
      //review:matching: SetMatchConditionWhenItIsAnother and AsAMatchCondition were written out in WriteOutTheDefinitionOf and
      //   UseWhereverItsDefinitionIs each, and AsAMatchCondition in EditedMatchCondition too.
      var matchCondition = AsAMatchCondition(condition);
      if(!matchCondition.Equals(MatchCondition)) SetMatchCondition(matchCondition);
   }

   /// <summary><paramref name="condition"/>, a condition on a word, as a vocab's match-condition: itself when it's an "all of" group, else a group holding it.</summary>
   /// <exception cref="System.ArgumentException">When it's a condition on another subject.</exception>
   internal static AllOfGroup<CandidateWordVariant> AsAMatchCondition(Condition condition) => ConditionGroup.AsAnAllOf(condition.AsAConditionOn<CandidateWordVariant>());

   /// <summary>
   /// What the vocab's configuration weighs: of two vocab that match the same word, and both or neither own the form matched, the one that weighs<br/>
   /// more wins. Each rule set weighs what it did before match-conditions, so the more specific a vocab's configuration, the more it weighs.
   /// </summary>
   public int CustomRequirementsWeight =>
      //review:matching: a condition that isn't a matching rule's weighs nothing, so priority knows only the rules. Priority once conditions can say
      //   more is open question 2 in dev_docs/matching-configuration.md.
      RequiresForbids.MatchWeight + MatchConditionWeight + YieldToSurface.MatchWeight;

   /// <summary>
   /// What the <see cref="MatchCondition"/>'s conditions add to the vocab's priority: what each rule's weighs, as the rule did before match-conditions. Each<br/>
   /// of the match-condition's flattened conditions is weighed (<see cref="ConditionGroup{TSubject}.FlattenedConditions"/>), and each that means the same as<br/>
   /// a rule's (<see cref="Condition.MeansTheSameAs"/>) is that rule's, so a shared-condition weighs what its definition weighs.
   /// </summary>
   int MatchConditionWeight => MatchCondition.FlattenedConditions.Sum(condition => MatchingRules.All.Sum(rule => rule.WeightOf(condition)));

   internal VocabMatchingDto ToDto() => new()
   {
      RequiredOrForbidden = RequiresForbids.ToDto(),
      Flags = Flags.ToDto(),
      Condition = MatchConditionJson.AsAVocabStoresIt(MatchCondition),
      YieldToSurface = YieldToSurface.ToDto()
   };

   /// <summary>Configures the vocab as <paramref name="other"/> is configured.</summary>
   internal void OverwriteWith(VocabNoteMatchingConfiguration other)
   {
      SetMatchCondition(other.MatchCondition);
      RequiresForbids.OverwriteWith(other.RequiresForbids);
      Flags.OverwriteWith(other.Flags);
      YieldToSurface.OverwriteWith(other.YieldToSurface);
   }
}
