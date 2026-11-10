using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>
/// A vocab's matching configuration: everything configured on the vocab that decides which words in sentences match it, which of its valid matches<br/>
/// are displayed, and which of two vocab matching the same word wins.
/// </summary>
public class VocabNoteMatchingConfiguration
{
   readonly NoteGuard _guard;

   /// <param name="partsOfSpeech">The vocab's parts of speech, which some rules and flags take into account.</param>
   /// <param name="data">The configuration as the corpus stores it, or null for a new vocab.</param>
   /// <param name="guard">Saves the vocab after a change.</param>
   internal VocabNoteMatchingConfiguration(VocabNotePartsOfSpeech partsOfSpeech, VocabMatchingDto? data, NoteGuard guard)
   {
      _guard = guard;
      var storedRequireForbidRules = data?.RequiredOrForbidden ?? new Dictionary<string, RequiredOrForbiddenDto>();
      //wip: reads the format from before match-conditions too, whose rules become the match-condition, until the corpus is migrated (increment 3 in
      //   dev_docs/matching-configuration.md). Then a vocab file with the old rules fails to load, as a file of an older format should.
      MatchCondition = data?.Condition is {} storedCondition ? MatchConditionJson.VocabsMatchConditionFrom(storedCondition) : MatchConditionOfTheRulesStoredBefore(data);
      RequiresForbids = new VocabMatchingRequireForbidFlags(partsOfSpeech, storedRequireForbidRules.Where(it => MatchingRules.Named(it.Key) == null).ToDictionary(), guard);
      Flags = new VocabMatchingFlags(partsOfSpeech, data?.Flags ?? [], guard);
      YieldToSurface = new VocabYieldToSurface([.. data?.YieldToSurface ?? [], .. data?.Rules?.YieldToSurface ?? []], guard);
      Rules = new MatchConditionAsRules(this);
   }

   /// <summary>The condition a word must meet to match the vocab: all of its conditions, none when it has none.</summary>
   public AllOfGroup<CandidateWordVariant> MatchCondition { get; private set; }

   /// <summary>The <see cref="MatchCondition"/> as the matching rules its conditions are, as the Matching tab shows and edits it.</summary>
   public MatchConditionAsRules Rules { get; }

   /// <summary>The require/forbid rules that aren't conditions in the <see cref="MatchCondition"/>: dictionary form (start), and yielding the last token.</summary>
   public VocabMatchingRequireForbidFlags RequiresForbids { get; }

   public VocabMatchingFlags Flags { get; }

   public VocabYieldToSurface YieldToSurface { get; }

   /// <summary>Makes <paramref name="matchCondition"/> the vocab's <see cref="MatchCondition"/>, and saves the vocab.</summary>
   public void SetMatchCondition(AllOfGroup<CandidateWordVariant> matchCondition) => _guard.Update(() => MatchCondition = matchCondition);

   /// <summary>
   /// What the vocab's configuration weighs: of two vocab that match the same word, and both or neither own the form matched, the one that weighs<br/>
   /// more wins. Each rule set weighs what it did before match-conditions, so the more specific a vocab's configuration, the more it weighs.
   /// </summary>
   public int CustomRequirementsWeight =>
      //review:matching: a condition that isn't a matching rule's weighs nothing, so priority knows only the rules. Priority once conditions can say
      //   more is open question 2 in dev_docs/matching-configuration.md.
      RequiresForbids.MatchWeight + Rules.Weight + YieldToSurface.MatchWeight;

   internal VocabMatchingDto ToDto() => new()
   {
      RequiredOrForbidden = RequiresForbids.ToDto(),
      Flags = Flags.ToDto(),
      Condition = MatchCondition.IsEmpty ? null : MatchConditionJson.Of(MatchCondition),
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

   /// <summary>The match-condition that the rules <paramref name="data"/> stores in the format from before match-conditions become, each rule's condition in the order of <see cref="MatchingRules.All"/>.</summary>
   static AllOfGroup<CandidateWordVariant> MatchConditionOfTheRulesStoredBefore(VocabMatchingDto? data)
   {
      return ConditionGroup.AllOf([.. MatchingRules.All.Select(ConditionStoredFor).OfType<Condition<CandidateWordVariant>>()]);

      Condition<CandidateWordVariant>? ConditionStoredFor(MatchingRule rule) => rule switch
      {
         RequireForbidMatchingRule requireForbid when data?.RequiredOrForbidden.TryGetValue(rule.Name, out var stored) == true =>
            requireForbid.ConditionFor(stored == RequiredOrForbiddenDto.Required ? RequireForbidChoice.Required : RequireForbidChoice.Forbidden),
         TextsMatchingRule texts when TextsStoredFor(texts) is { Count: > 0 } storedTexts => texts.ConditionOn(storedTexts),
         _ => null
      };

      List<string> TextsStoredFor(TextsMatchingRule rule) => data?.Rules is not {} rules ? []
         : rule == MatchingRules.PrefixIsNot ? rules.PrefixIsNot
         : rule == MatchingRules.RequiredPrefix ? rules.RequiredPrefix
         : rule == MatchingRules.SuffixIsNot ? rules.SuffixIsNot
         : rule == MatchingRules.SurfaceIsNot ? rules.SurfaceIsNot
         : [];
   }
}
