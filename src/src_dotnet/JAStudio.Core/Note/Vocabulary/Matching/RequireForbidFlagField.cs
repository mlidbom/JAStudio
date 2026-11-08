using System;
using System.Collections.Generic;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>
/// One require/forbid rule of a vocab's matching configuration, such as <see cref="VocabMatchingRequireForbidFlags.Godan"/>: something about<br/>
/// a word, such as following a godan verb's stem, that a word must have to match the vocab, must not have, or that doesn't matter (<see cref="Choice"/>).
/// </summary>
/// <remarks>
/// A rule that's set also weighs in when two vocab match the same word: the vocab whose rules weigh more wins<br/>
/// (<see cref="VocabNoteMatchingConfiguration.CustomRequirementsWeight"/>).
/// </remarks>
public class RequireForbidFlagField
{
   readonly NoteGuard _guard;

   /// <param name="name">The rule's <see cref="Name"/>.</param>
   /// <param name="requiredWeight">The <see cref="RequiredWeight"/>.</param>
   /// <param name="forbiddenWeight">The <see cref="ForbiddenWeight"/>.</param>
   /// <param name="storedRules">The vocab's rules that are set, as the corpus stores them: the rule is unset unless it's among them.</param>
   /// <param name="guard">Saves the vocab after a change.</param>
   internal RequireForbidFlagField(string name, int requiredWeight, int forbiddenWeight, IReadOnlyDictionary<string, RequiredOrForbiddenDto> storedRules, NoteGuard guard)
   {
      Name = name;
      RequiredWeight = requiredWeight;
      ForbiddenWeight = forbiddenWeight;
      Choice = storedRules.TryGetValue(name, out var stored) ? ChoiceStoredAs(stored) : RequireForbidChoice.Unset;
      _guard = guard;
   }

   /// <summary>The rule's name in the corpus, such as <c>godan</c>: the key the vocab's stored rules have it under.</summary>
   public string Name { get; }

   /// <summary>What the rule weighs while it's required: see <see cref="VocabNoteMatchingConfiguration.CustomRequirementsWeight"/>.</summary>
   public int RequiredWeight { get; }

   /// <summary>What the rule weighs while it's forbidden: see <see cref="VocabNoteMatchingConfiguration.CustomRequirementsWeight"/>.</summary>
   public int ForbiddenWeight { get; }

   /// <summary>What the vocab's matching configuration says about the rule.</summary>
   public RequireForbidChoice Choice { get; private set; }

   /// <summary>Whether a match requires what the rule is about: what's configured, but some rules also require it of vocab that haven't configured them.</summary>
   public virtual bool IsRequired => Choice == RequireForbidChoice.Required;

   public bool IsForbidden => Choice == RequireForbidChoice.Forbidden;

   /// <summary>Whether the rule is configured at all, required or forbidden.</summary>
   public bool IsConfigured => Choice != RequireForbidChoice.Unset;

   /// <summary>What the rule weighs now: its <see cref="RequiredWeight"/> while required, its <see cref="ForbiddenWeight"/> while forbidden, else nothing.</summary>
   public int MatchWeight => IsRequired ? RequiredWeight : IsForbidden ? ForbiddenWeight : 0;

   /// <summary>Configures the rule as <paramref name="choice"/> says, and saves the vocab.</summary>
   public void Choose(RequireForbidChoice choice) => _guard.Update(() => Choice = choice);

   /// <summary>The rule as the corpus stores it. Only a rule that <see cref="IsConfigured"/> is stored.</summary>
   internal RequiredOrForbiddenDto ToDto() => Choice switch
   {
      RequireForbidChoice.Required => RequiredOrForbiddenDto.Required,
      RequireForbidChoice.Forbidden => RequiredOrForbiddenDto.Forbidden,
      RequireForbidChoice.Unset => throw new InvalidOperationException($"The rule {Name} is unset, and an unset rule isn't stored."),
      _ => throw new ArgumentOutOfRangeException(nameof(Choice), Choice, null)
   };

   static RequireForbidChoice ChoiceStoredAs(RequiredOrForbiddenDto stored) => stored switch
   {
      RequiredOrForbiddenDto.Required => RequireForbidChoice.Required,
      RequiredOrForbiddenDto.Forbidden => RequireForbidChoice.Forbidden,
      _ => throw new ArgumentOutOfRangeException(nameof(stored), stored, null)
   };

   public override string ToString() => $"{Name} required: {IsRequired}, forbidden: {IsForbidden}";
}
