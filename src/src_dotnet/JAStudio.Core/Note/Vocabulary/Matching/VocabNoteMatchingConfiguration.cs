using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
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

   /// <param name="partsOfSpeech">The vocab's parts of speech, which some rules and flags take into account.</param>
   /// <param name="data">The configuration as the corpus stores it, or null for a new vocab.</param>
   /// <param name="sharedConditions">The corpus's shared-conditions, which its match-condition uses by name.</param>
   /// <param name="guard">Saves the vocab after a change.</param>
   internal VocabNoteMatchingConfiguration(VocabNotePartsOfSpeech partsOfSpeech, VocabMatchingDto? data, SharedConditions sharedConditions, NoteGuard guard)
   {
      _guard = guard;
      MatchCondition = MatchConditionJson.VocabsMatchConditionFrom(data?.Condition, sharedConditions);
      RequiresForbids = new VocabMatchingRequireForbidFlags(partsOfSpeech, data?.RequiredOrForbidden ?? new Dictionary<string, RequiredOrForbiddenDto>(), guard);
      Flags = new VocabMatchingFlags(partsOfSpeech, data?.Flags ?? [], guard);
      YieldToSurface = new VocabYieldToSurface(data?.YieldToSurface ?? [], guard);
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
      RequiresForbids.MatchWeight + Rules.MatchWeight + YieldToSurface.MatchWeight;

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
