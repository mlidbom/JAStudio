using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>
/// A vocab's require/forbid rules (<see cref="RequireForbidFlagField"/>) that aren't conditions in its match-condition: whether a word may start<br/>
/// with a dictionary-form verb's ending (<see cref="DictionaryFormStart"/>), which every vocab forbids unless it requires it, and whether the vocab yields<br/>
/// its last token to an overlapping compound (<see cref="YieldLastToken"/>), a rule on display.
/// </summary>
/// <remarks>The corpus stores only the rules that are set, each under its <see cref="RequireForbidFlagField.Name"/>.</remarks>
public class VocabMatchingRequireForbidFlags
{
   readonly List<RequireForbidFlagField> _all = [];

   /// <param name="partsOfSpeech">The vocab's parts of speech, which decide whether it yields its last token to an overlapping compound by default.</param>
   /// <param name="storedRules">The vocab's rules that are set, as the corpus stores them. Each must name one of the rules here.</param>
   /// <param name="guard">Saves the vocab after a change.</param>
   internal VocabMatchingRequireForbidFlags(VocabNotePartsOfSpeech partsOfSpeech, IReadOnlyDictionary<string, RequiredOrForbiddenDto> storedRules, NoteGuard guard)
   {
      DictionaryFormStart = Rule("dictionaryFormStart", requiredWeight: 30, forbiddenWeight: 1);
      YieldLastToken = Add(new YieldLastTokenToOverlappingCompound(partsOfSpeech, storedRules, guard));
      AssertEveryStoredRuleIsOneOfThese();
      return;

      RequireForbidFlagField Rule(string name, int requiredWeight, int forbiddenWeight) => Add(new RequireForbidFlagField(name, requiredWeight, forbiddenWeight, storedRules, guard));

      TRule Add<TRule>(TRule rule) where TRule : RequireForbidFlagField
      {
         _all.Add(rule);
         return rule;
      }

      void AssertEveryStoredRuleIsOneOfThese()
      {
         var unknownRules = storedRules.Keys.Where(name => _all.All(rule => rule.Name != name)).ToList();
         Contract.Argument.Assert(unknownRules.Count == 0, () => $"The stored require/forbid rules {string.Join(", ", unknownRules)} are unknown, and would be lost when the vocab is saved.");
      }
   }

   /// <summary>
   /// Required, a word must start with a dictionary-form verb's ending to match, and the vocab's forms' placeholder for one is expanded. Otherwise,<br/>
   /// required or not, a word that starts with one doesn't match. Forbidding it differs from leaving it unset only in what it weighs.
   /// </summary>
   public RequireForbidFlagField DictionaryFormStart { get; }

   public YieldLastTokenToOverlappingCompound YieldLastToken { get; }

   /// <summary>What the rules weigh together: see <see cref="VocabNoteMatchingConfiguration.CustomRequirementsWeight"/>.</summary>
   public int MatchWeight => _all.Sum(it => it.MatchWeight);

   /// <summary>The rules that are set, as the corpus stores them: in ordinal order of their names, so that the same rules always write the same file.</summary>
   internal Dictionary<string, RequiredOrForbiddenDto> ToDto() =>
      _all.Where(it => it.IsConfigured)
          .OrderBy(it => it.Name, StringComparer.Ordinal)
          .ToDictionary(it => it.Name, it => it.ToDto());

   /// <summary>Configures every rule as <paramref name="other"/> has it.</summary>
   internal void OverwriteWith(VocabMatchingRequireForbidFlags other)
   {
      foreach(var (rule, otherRule) in _all.Zip(other._all)) rule.Choose(otherRule.Choice);
   }
}
