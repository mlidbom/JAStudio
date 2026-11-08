using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNoteMatchingConfiguration
{
   readonly VocabNote _vocab;
   int? _customRequirementsWeight;

   public VocabNoteMatchingRules ConfigurableRules { get; }
   public VocabMatchingRulesConfigurationRequiresForbidsFlags RequiresForbids { get; private set; }
   public VocabMatchingRulesConfigurationBoolFlags BoolFlags { get; private set; }

   public VocabNoteMatchingConfiguration(VocabNote vocab, VocabDto? data, NoteGuard guard)
   {
      _vocab = vocab;
      ConfigurableRules = new VocabNoteMatchingRules(data?.MatchingRules, guard);
      RequiresForbids = new VocabMatchingRulesConfigurationRequiresForbidsFlags(vocab);
      BoolFlags = new VocabMatchingRulesConfigurationBoolFlags(vocab);
   }

   public void RefreshTagBasedFlags()
   {
      RequiresForbids = new VocabMatchingRulesConfigurationRequiresForbidsFlags(_vocab);
      BoolFlags = new VocabMatchingRulesConfigurationBoolFlags(_vocab);
      _customRequirementsWeight = null;
   }

   public int CustomRequirementsWeight
   {
      get
      {
         _customRequirementsWeight ??= RequiresForbids.MatchWeight + ConfigurableRules.MatchWeight;
         return _customRequirementsWeight.Value;
      }
   }
}
