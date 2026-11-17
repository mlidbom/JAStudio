using System.Collections.Generic;
using System.Linq;
using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary.Tevents;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>
/// A vocab's matching configuration: everything configured on the vocab that decides which words in sentences match it, which of its valid matches<br/>
/// are displayed, and which of two vocab matching the same word wins.
/// </summary>
/// <remarks>
/// A component of its vocab: its match-condition, its flags, its require/forbid rules and its yield-to-surface rule publish tevents of their own,<br/>
/// <see cref="IVocabTevent.MatchingConfiguration"/>, which its appliers apply.
/// </remarks>
public class VocabNoteMatchingConfiguration : TeventiveComponent<VocabNote, INoteTevent, NoteTevent, VocabNoteMatchingConfiguration, IVocabTevent.MatchingConfiguration, VocabTevent.MatchingConfiguration>, ITeventRouted
{
   /// <remarks>Every tevent of the configuration, applied, also discards what the vocab's forms derived from it (<see cref="VocabNoteForms.OnMatchingConfigurationChanged"/>).</remarks>
   static readonly TeventRouting<VocabNoteMatchingConfiguration, IVocabTevent.MatchingConfiguration> Appliers = DeclareAppliers(it => it
     .For<IVocabTevent.MatchingConfiguration.PropertyUpdated.NamesOfTheSetFlags>(static (configuration, tevent) => configuration.Flags.Apply(tevent.NamesOfTheSetFlags))
     .For<IVocabTevent.MatchingConfiguration.PropertyUpdated.DictionaryFormStart>(static (configuration, tevent) => configuration.RequiresForbids.DictionaryFormStart.Apply(tevent.DictionaryFormStart))
     .For<IVocabTevent.MatchingConfiguration.PropertyUpdated.YieldLastToken>(static (configuration, tevent) => configuration.RequiresForbids.YieldLastToken.Apply(tevent.YieldLastToken))
     .For<IVocabTevent.MatchingConfiguration.PropertyUpdated.YieldToSurface>(static (configuration, tevent) => configuration.YieldToSurface.Apply(tevent.YieldToSurface))
     .For<IVocabTevent.MatchingConfiguration.PropertyUpdated.MatchCondition>(static (configuration, tevent) => configuration.MatchCondition = tevent.MatchCondition)
     .For<IVocabTevent.MatchingConfiguration>(static (configuration, _) => configuration._vocab.Forms.OnMatchingConfigurationChanged()));

   TeventRouting ITeventRouted.TeventAppliers => Appliers;

   readonly VocabNote _vocab;
   readonly SharedConditions _sharedConditions;

   /// <param name="vocab">The vocab it's of, whose parts of speech some rules and flags take into account.</param>
   /// <param name="data">The configuration as the corpus stores it, or null for a new vocab.</param>
   /// <param name="sharedConditions">The corpus's shared-conditions, which its match-condition uses by name.</param>
   internal VocabNoteMatchingConfiguration(VocabNote vocab, VocabMatchingDto? data, SharedConditions sharedConditions) : base(vocab)
   {
      _vocab = vocab;
      _sharedConditions = sharedConditions;
      MatchCondition = MatchConditionJson.VocabsMatchConditionFrom(data?.Condition, sharedConditions);
      RequiresForbids = new VocabMatchingRequireForbidFlags(vocab, vocab.PartsOfSpeech, data?.RequiredOrForbidden ?? new Dictionary<string, RequiredOrForbiddenDto>());
      Flags = new VocabMatchingFlags(vocab, vocab.PartsOfSpeech, data?.Flags ?? []);
      YieldToSurface = new VocabYieldToSurface(vocab, data?.YieldToSurface ?? []);
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
   /// Makes <paramref name="matchCondition"/> the vocab's <see cref="MatchCondition"/>, publishing its tevent when it's another than the one it has. Each<br/>
   /// shared-condition it uses must be one of the corpus's (<see cref="SharedConditions.AssertHoldsEachUsedBy"/>).
   /// </summary>
   public void SetMatchCondition(AllOfGroup<CandidateWordVariant> matchCondition)
   {
      _sharedConditions.AssertHoldsEachUsedBy(matchCondition);
      _vocab.PublishIfChanged(MatchCondition, matchCondition, static it => new VocabTevent.MatchingConfiguration.MatchConditionUpdated(it));
   }

   /// <summary>
   /// Has the vocab written again after a shared-condition its <see cref="MatchCondition"/> uses was renamed (<see cref="SharedConditions.Rename"/>), by<br/>
   /// publishing <see cref="IVocabTevent.MatchingConfiguration.SharedConditionRenamed"/>. The match-condition holds the shared-condition object itself,<br/>
   /// which <see cref="SharedConditions"/> renamed in place, so nothing of the vocab's own changed; but its file stores the shared-condition by its name.
   /// </summary>
   internal void SaveAfterASharedConditionItUsesWasRenamed() => Publish(new VocabTevent.MatchingConfiguration.SharedConditionRenamed());

   /// <summary>
   /// Writes <paramref name="sharedCondition"/>'s definition out in the <see cref="MatchCondition"/> wherever it uses it (<see cref="Condition.WithTheDefinitionWrittenOutOf"/>),<br/>
   /// as when the shared-condition is deleted, and saves the vocab when that changes it. It then means the same, so the vocab matches the same words.
   /// </summary>
   internal void WriteOutTheDefinitionOf(ISharedCondition sharedCondition) => SetMatchConditionWhenItIsAnother(MatchCondition.WithTheDefinitionWrittenOutOf(sharedCondition));

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
