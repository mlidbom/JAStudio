using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>
/// A vocab's require/forbid rules (<see cref="RequireForbidFlagField"/>): one for each thing about a word that a vocab can require<br/>
/// of the words that match it, or forbid, such as following a godan verb's stem (<see cref="Godan"/>) or ending the sentence (<see cref="SentenceEnd"/>).
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
      MasuStem = Rule("masuStem", 50, 1);
      Godan = Rule("godan", 50, 1);
      Ichidan = Rule("ichidan", 50, 1);
      Irrealis = Rule("irrealis", 50, 1);
      AdverbHead = Rule("adverbHead", 30, 1);
      PastTenseStem = Rule("pastTenseStem", 30, 1);
      DictionaryFormStart = Rule("dictionaryFormStart", 30, 1);
      DictionaryFormHead = Rule("dictionaryFormHead", 30, 1);
      TeFormStemStart = Rule("teFormStemStart", 20, 1);
      TeFormStemHead = Rule("teFormStemHead", 20, 1);
      IchidanImperative = Rule("ichidanImperative", 30, 1);
      GodanPotential = Rule("godanPotential", 30, 1);
      GodanImperative = Rule("godanImperative", 30, 1);
      GodanImperativeHead = Rule("godanImperativeHead", 30, 1);
      SingleToken = Rule("singleToken", 30, 1);
      SentenceEnd = Rule("sentenceEnd", 10, 1);
      SentenceStart = Rule("sentenceStart", 10, 1);
      Surface = Rule("surface", 10, 1);
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

   public RequireForbidFlagField MasuStem { get; }
   public RequireForbidFlagField Godan { get; }
   public RequireForbidFlagField Ichidan { get; }
   public RequireForbidFlagField Irrealis { get; }
   public RequireForbidFlagField AdverbHead { get; }
   public RequireForbidFlagField PastTenseStem { get; }
   public RequireForbidFlagField DictionaryFormStart { get; }
   public RequireForbidFlagField DictionaryFormHead { get; }
   public RequireForbidFlagField TeFormStemStart { get; }
   public RequireForbidFlagField TeFormStemHead { get; }
   public RequireForbidFlagField IchidanImperative { get; }
   public RequireForbidFlagField GodanPotential { get; }
   public RequireForbidFlagField GodanImperative { get; }
   public RequireForbidFlagField GodanImperativeHead { get; }

   /// <summary>Required: the word must be a single token. Forbidden: it must be a compound of several.</summary>
   public RequireForbidFlagField SingleToken { get; }

   public RequireForbidFlagField SentenceEnd { get; }
   public RequireForbidFlagField SentenceStart { get; }
   public RequireForbidFlagField Surface { get; }
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
