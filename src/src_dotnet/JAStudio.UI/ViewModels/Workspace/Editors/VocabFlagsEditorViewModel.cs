using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Controls;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// The workspace's editor for a vocab note's flags (<see cref="VocabFlagsEditor"/>): its matching configuration,<br/>
/// the rules for finding the vocab in sentences, and its register. Every change is written to the note at once.
/// </summary>
/// <remarks>
/// It holds the vocab's id, not the note object, because the collection replaces a note's object when Anki edits the note<br/>
/// and when it reloads, and an editor holding the old object would edit an orphan.<br/>
/// So every row reads and writes through <see cref="CurrentVocab"/>, and the tab showing the vocab calls <see cref="Refresh"/>
/// whenever the note is updated, whoever updated it.
/// </remarks>
/// <remarks>
/// A change to a parsing rule takes effect in sentences only when they are reparsed, which can take a while for a common word.<br/>
/// So instead of reparsing after every change, the editor marks the sentences as needing it (<see cref="SentencesNeedReparsing"/>),<br/>
/// and <see cref="ReparseSentencesCommand"/> reparses them once, however many rules were changed.
/// </remarks>
partial class VocabFlagsEditorViewModel : ObservableObject
{
   readonly TemporaryServiceCollection _services;
   readonly NoteId _vocabId;

   public VocabFlagsEditorViewModel(TemporaryServiceCollection services, NoteId vocabId)
   {
      _services = services;
      _vocabId = vocabId;

      YieldLastToken = RequireForbidRule("Yield to overlapping following compound", it => it.YieldLastToken, isParsingRule: false);
      MiscMatchingRules =
      [
         RequireForbidRule("Surface", it => it.Surface),
         RequireForbidRule("Sentence end", it => it.SentenceEnd),
         RequireForbidRule("Sentence start", it => it.SentenceStart),
         RequireForbidRule("Single token", it => it.SingleToken)
      ];
      StemMatchingRules =
      [
         RequireForbidRule("Godan imperative", it => it.GodanImperative),
         RequireForbidRule("Godan imperative (head)", it => it.GodanImperativeHead),
         RequireForbidRule("Godan potential", it => it.GodanPotential),
         RequireForbidRule("Ichidan imperative", it => it.IchidanImperative),
         RequireForbidRule("Masu stem", it => it.MasuStem),
         RequireForbidRule("Godan", it => it.Godan),
         RequireForbidRule("Ichidan", it => it.Ichidan),
         RequireForbidRule("Irrealis", it => it.Irrealis),
         RequireForbidRule("Past tense stem", it => it.PastTenseStem),
         RequireForbidRule("Dictionary form (start)", it => it.DictionaryFormStart),
         RequireForbidRule("Dictionary form (head)", it => it.DictionaryFormHead),
         RequireForbidRule("Adverb (head)", it => it.AdverbHead),
         RequireForbidRule("て-form stem (start)", it => it.TeFormStemStart),
         RequireForbidRule("て-form stem (head)", it => it.TeFormStemHead)
      ];
      IsFlags =
      [
         Flag("Inflecting word", it => it.MatchingConfiguration.BoolFlags.IsInflectingWord),
         Flag("Poison word", it => it.MatchingConfiguration.BoolFlags.IsPoisonWord)
      ];
      MiscFlags =
      [
         Flag("Match with preceding vowel", it => it.MatchingConfiguration.BoolFlags.MatchWithPrecedingVowel),
         Flag("Question overrides form: Show the question in results even if the match was another form", it => it.MatchingConfiguration.BoolFlags.QuestionOverridesForm)
      ];
      StringRules =
      [
         StringRule("Prefix is not", it => it.PrefixIsNot),
         StringRule("Required prefix", it => it.RequiredPrefix),
         StringRule("Suffix is not", it => it.SuffixIsNot),
         StringRule("Surface is not", it => it.SurfaceIsNot),
         StringRule("Yield to surface", it => it.YieldToSurface)
      ];
      MetadataFlags = [Flag("Compositionally transparent compound", it => it.MatchingConfiguration.BoolFlags.IsCompositionallyTransparentCompound)];
      RegisterFlags =
      [
         Flag("Archaic", it => it.Register.Archaic),
         Flag("Childish", it => it.Register.Childish),
         Flag("Derogatory", it => it.Register.Derogatory),
         Flag("Formal", it => it.Register.Formal),
         Flag("Honorific", it => it.Register.Honorific),
         Flag("Humble", it => it.Register.Humble),
         Flag("Informal", it => it.Register.Informal),
         Flag("Literary", it => it.Register.Literary),
         Flag("Polite", it => it.Register.Polite),
         Flag("Rough, traditionally considered male", it => it.Register.RoughMasculine),
         Flag("Sensitive", it => it.Register.Sensitive),
         Flag("Slang", it => it.Register.Slang),
         Flag("Soft, traditionally considered feminine", it => it.Register.SoftFeminine),
         Flag("Vulgar, usually offensive", it => it.Register.Vulgar)
      ];
   }

   public RequireForbidControlViewModel YieldLastToken { get; }
   public IReadOnlyList<RequireForbidControlViewModel> MiscMatchingRules { get; }
   public IReadOnlyList<RequireForbidControlViewModel> StemMatchingRules { get; }
   public IReadOnlyList<TagFlagCheckBoxViewModel> IsFlags { get; }
   public IReadOnlyList<TagFlagCheckBoxViewModel> MiscFlags { get; }
   public IReadOnlyList<StringSetControlViewModel> StringRules { get; }
   public IReadOnlyList<TagFlagCheckBoxViewModel> MetadataFlags { get; }
   public IReadOnlyList<TagFlagCheckBoxViewModel> RegisterFlags { get; }

   /// <summary>
   /// Whether a parsing rule was changed here since the sentences were last reparsed for this vocab.<br/>
   /// The rules that count are the ones the old flags dialog asked to reparse for when OK was pressed:<br/>
   /// the require/forbid rules that affect matching, and the string rules.
   /// </summary>
   [ObservableProperty] bool _sentencesNeedReparsing;

   /// <summary>Shows every row as the note has it now. Called by the tab showing the vocab whenever the note is updated.</summary>
   public void Refresh()
   {
      foreach(var rule in AllRequireForbidRules) rule.Refresh();
      foreach(var flag in AllFlags) flag.Refresh();
      foreach(var rule in StringRules) rule.Refresh();
   }

   /// <summary>Reparses the sentences that may contain this vocab, so that they reflect its changed parsing rules.</summary>
   [RelayCommand]
   async Task ReparseSentencesAsync()
   {
      SentencesNeedReparsing = false;
      var vocab = CurrentVocab();
      await TaskCE.Run(() => _services.LocalNoteUpdater.ReparseSentencesForVocab(vocab));
   }

   IEnumerable<RequireForbidControlViewModel> AllRequireForbidRules => [YieldLastToken, ..MiscMatchingRules, ..StemMatchingRules];

   IEnumerable<TagFlagCheckBoxViewModel> AllFlags => [..IsFlags, ..MiscFlags, ..MetadataFlags, ..RegisterFlags];

   /// <summary>The vocab being edited, as the collection holds it now.</summary>
   VocabNote CurrentVocab() => _services.CoreApp.Collection.Vocab.WithIdOrNone(_vocabId)
                            ?? throw new InvalidOperationException($"The vocab being edited, {_vocabId}, is no longer in the collection.");

   RequireForbidControlViewModel RequireForbidRule(string label, Func<VocabMatchingRulesConfigurationRequiresForbidsFlags, RequireForbidFlagField> rule, bool isParsingRule = true) =>
      new(label,
          () => rule(CurrentVocab().MatchingConfiguration.RequiresForbids),
          isParsingRule ? ParsingRuleChanged : () => {});

   TagFlagCheckBoxViewModel Flag(string label, Func<VocabNote, TagFlagField> flag) => new(label, () => flag(CurrentVocab()));

   StringSetControlViewModel StringRule(string title, Func<VocabNoteMatchingRules, HashSet<string>> rule) =>
      new(title,
          () => rule(CurrentVocab().MatchingConfiguration.ConfigurableRules),
          () => CurrentVocab().MatchingConfiguration.ConfigurableRules.Save(),
          ParsingRuleChanged);

   void ParsingRuleChanged() => SentencesNeedReparsing = true;
}
