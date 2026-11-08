using System.Collections.Generic;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>
/// A vocab's matching configuration: everything configured on the vocab that decides which words in sentences match it,<br/>
/// and which of two vocab matching the same word wins.
/// </summary>
public class VocabNoteMatchingConfiguration
{
   /// <param name="partsOfSpeech">The vocab's parts of speech, which some rules and flags take into account.</param>
   /// <param name="data">The configuration as the corpus stores it, or null for a new vocab.</param>
   /// <param name="guard">Saves the vocab after a change.</param>
   internal VocabNoteMatchingConfiguration(VocabNotePartsOfSpeech partsOfSpeech, VocabMatchingDto? data, NoteGuard guard)
   {
      RequiresForbids = new VocabMatchingRequireForbidFlags(partsOfSpeech, data?.RequiredOrForbidden ?? new Dictionary<string, RequiredOrForbiddenDto>(), guard);
      Flags = new VocabMatchingFlags(partsOfSpeech, data?.Flags ?? [], guard);
      ConfigurableRules = new VocabNoteMatchingRules(data?.Rules, guard);
   }

   public VocabMatchingRequireForbidFlags RequiresForbids { get; }
   public VocabMatchingFlags Flags { get; }
   public VocabNoteMatchingRules ConfigurableRules { get; }

   /// <summary>
   /// What the vocab's configured rules weigh: of two vocab that match the same word, and both or neither own the form matched,<br/>
   /// the one whose rules weigh more wins. The more specific a vocab's configuration, the more it weighs.
   /// </summary>
   public int CustomRequirementsWeight => RequiresForbids.MatchWeight + ConfigurableRules.MatchWeight;

   internal VocabMatchingDto ToDto() => new()
   {
      RequiredOrForbidden = RequiresForbids.ToDto(),
      Flags = Flags.ToDto(),
      Rules = ConfigurableRules.ToDto()
   };

   /// <summary>Configures the vocab as <paramref name="other"/> is configured.</summary>
   internal void OverwriteWith(VocabNoteMatchingConfiguration other)
   {
      RequiresForbids.OverwriteWith(other.RequiresForbids);
      Flags.OverwriteWith(other.Flags);
      ConfigurableRules.OverwriteWith(other.ConfigurableRules);
   }
}
