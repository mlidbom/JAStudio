using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace JAStudio.UI.ViewModels;

/// <summary>
/// ViewModel for editing VocabNote matching flags, register settings, and string rules.
/// </summary>
partial class VocabFlagsViewModel : ObservableObject
{
   readonly VocabNote _vocab;
   readonly Window? _parentWindow;
   readonly Core.TemporaryServiceCollection _services;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
   [Obsolete("Parameterless constructor is only for XAML designer support and should not be used directly.")]
   public VocabFlagsViewModel() {}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

   public VocabFlagsViewModel(VocabNote vocab, Core.TemporaryServiceCollection services, Window? parentWindow = null)
   {
      _vocab = vocab;
      _services = services;
      _parentWindow = parentWindow;
      Title = $"Edit Flags: {vocab.GetQuestion()}";
      LoadFromNote();
   }

   public string Title { get; }

   // --- Matching Configuration: Bool Flags ---

   [ObservableProperty] bool _isInflectingWord;

   [ObservableProperty] bool _isPoisonWord;

   [ObservableProperty] bool _matchWithPrecedingVowel;

   [ObservableProperty] bool _questionOverridesForm;

   [ObservableProperty] bool _isCompositionallyTransparentCompound;

   // --- Register Flags ---

   [ObservableProperty] bool _archaic;

   [ObservableProperty] bool _childish;

   [ObservableProperty] bool _derogatory;

   [ObservableProperty] bool _formal;

   [ObservableProperty] bool _honorific;

   [ObservableProperty] bool _humble;

   [ObservableProperty] bool _informal;

   [ObservableProperty] bool _literary;

   [ObservableProperty] bool _polite;

   [ObservableProperty] bool _roughMasculine;

   [ObservableProperty] bool _sensitive;

   [ObservableProperty] bool _slang;

   [ObservableProperty] bool _softFeminine;

   [ObservableProperty] bool _vulgar;

   // --- Require/Forbid ViewModels ---

   public RequireForbidControlViewModel YieldLastToken { get; private set; } = null!;
   public RequireForbidControlViewModel Surface { get; private set; } = null!;
   public RequireForbidControlViewModel SentenceEnd { get; private set; } = null!;
   public RequireForbidControlViewModel SentenceStart { get; private set; } = null!;
   public RequireForbidControlViewModel SingleToken { get; private set; } = null!;
   public RequireForbidControlViewModel GodanImperative { get; private set; } = null!;
   public RequireForbidControlViewModel GodanImperativePrefix { get; private set; } = null!;
   public RequireForbidControlViewModel GodanPotential { get; private set; } = null!;
   public RequireForbidControlViewModel IchidanImperative { get; private set; } = null!;
   public RequireForbidControlViewModel MasuStem { get; private set; } = null!;
   public RequireForbidControlViewModel Godan { get; private set; } = null!;
   public RequireForbidControlViewModel Ichidan { get; private set; } = null!;
   public RequireForbidControlViewModel Irrealis { get; private set; } = null!;
   public RequireForbidControlViewModel PastTenseStem { get; private set; } = null!;
   public RequireForbidControlViewModel DictionaryFormStem { get; private set; } = null!;
   public RequireForbidControlViewModel DictionaryFormPrefix { get; private set; } = null!;
   public RequireForbidControlViewModel PrecedingAdverb { get; private set; } = null!;
   public RequireForbidControlViewModel TeFormStem { get; private set; } = null!;
   public RequireForbidControlViewModel TeFormPrefix { get; private set; } = null!;

   // --- String Set ViewModels ---

   public StringSetControlViewModel PrefixIsNot { get; private set; } = null!;
   public StringSetControlViewModel RequiredPrefix { get; private set; } = null!;
   public StringSetControlViewModel SuffixIsNot { get; private set; } = null!;
   public StringSetControlViewModel SurfaceIsNot { get; private set; } = null!;
   public StringSetControlViewModel YieldToSurface { get; private set; } = null!;

   // List of all require/forbid controls for change tracking
   List<RequireForbidControlViewModel> AllRequireForbidControls { get; set; } = [];

   // List of all string set controls for change tracking
   List<StringSetControlViewModel> AllStringSetControls { get; set; } = [];

   // --- Commands (note: these will be replaced in the code-behind) ---

   public IAsyncRelayCommand SaveCommand { get; set; } = null!;
   public IRelayCommand CancelCommand { get; set; } = null!;

   // --- Load/Save ---

   void LoadFromNote()
   {
      // Bool flags
      IsInflectingWord = _vocab.MatchingConfiguration.BoolFlags.IsInflectingWord.IsSet();
      IsPoisonWord = _vocab.MatchingConfiguration.BoolFlags.IsPoisonWord.IsSet();
      MatchWithPrecedingVowel = _vocab.MatchingConfiguration.BoolFlags.MatchWithPrecedingVowel.IsSet();
      QuestionOverridesForm = _vocab.MatchingConfiguration.BoolFlags.QuestionOverridesForm.IsSet();
      IsCompositionallyTransparentCompound = _vocab.MatchingConfiguration.BoolFlags.IsCompositionallyTransparentCompound.IsSet();

      // Register flags
      Archaic = _vocab.Register.Archaic.IsSet();
      Childish = _vocab.Register.Childish.IsSet();
      Derogatory = _vocab.Register.Derogatory.IsSet();
      Formal = _vocab.Register.Formal.IsSet();
      Honorific = _vocab.Register.Honorific.IsSet();
      Humble = _vocab.Register.Humble.IsSet();
      Informal = _vocab.Register.Informal.IsSet();
      Literary = _vocab.Register.Literary.IsSet();
      Polite = _vocab.Register.Polite.IsSet();
      RoughMasculine = _vocab.Register.RoughMasculine.IsSet();
      Sensitive = _vocab.Register.Sensitive.IsSet();
      Slang = _vocab.Register.Slang.IsSet();
      SoftFeminine = _vocab.Register.SoftFeminine.IsSet();
      Vulgar = _vocab.Register.Vulgar.IsSet();

      // Require/Forbid controls (Display section)
      YieldLastToken = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.YieldLastToken, "Yield to overlapping following compound", reparseTrigger: false);
      AllRequireForbidControls.Add(YieldLastToken);

      // Require/Forbid controls (Misc matching rules)
      Surface = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.Surface, "Surface");
      SentenceEnd = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.SentenceEnd, "Sentence end");
      SentenceStart = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.SentenceStart, "Sentence start");
      SingleToken = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.SingleToken, "Single token");
      AllRequireForbidControls.AddRange([Surface, SentenceEnd, SentenceStart, SingleToken]);

      // Require/Forbid controls (Stem matching rules)
      GodanImperative = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.GodanImperative, "Godan imperative");
      GodanImperativePrefix = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.GodanImperativePrefix, "Godan imperative prefix");
      GodanPotential = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.GodanPotential, "Godan potential");
      IchidanImperative = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.IchidanImperative, "Ichidan imperative");
      MasuStem = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.MasuStem, "Masu stem");
      Godan = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.Godan, "Godan");
      Ichidan = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.Ichidan, "Ichidan");
      Irrealis = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.Irrealis, "Irrealis");
      PastTenseStem = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.PastTenseStem, "Past tense stem");
      DictionaryFormStem = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.DictionaryFormStem, "Dictionary form stem");
      DictionaryFormPrefix = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.DictionaryFormPrefix, "Dictionary form prefix");
      PrecedingAdverb = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.PrecedingAdverb, "Preceding adverb");
      TeFormStem = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.TeFormStem, "て-form stem");
      TeFormPrefix = new RequireForbidControlViewModel(_vocab.MatchingConfiguration.RequiresForbids.TeFormPrefix, "て-form prefix");
      AllRequireForbidControls.AddRange([GodanImperative, GodanImperativePrefix, GodanPotential, IchidanImperative, MasuStem, Godan, Ichidan, Irrealis, PastTenseStem, DictionaryFormStem, DictionaryFormPrefix, PrecedingAdverb, TeFormStem, TeFormPrefix]);

      // String set controls
      var rules = _vocab.MatchingConfiguration.ConfigurableRules;
      PrefixIsNot = new StringSetControlViewModel(rules.PrefixIsNot, "Prefix is not", _parentWindow);
      RequiredPrefix = new StringSetControlViewModel(rules.RequiredPrefix, "Required prefix", _parentWindow);
      SuffixIsNot = new StringSetControlViewModel(rules.SuffixIsNot, "Suffix is not", _parentWindow);
      SurfaceIsNot = new StringSetControlViewModel(rules.SurfaceIsNot, "Surface is not", _parentWindow);
      YieldToSurface = new StringSetControlViewModel(rules.YieldToSurface, "Yield to surface", _parentWindow);
      AllStringSetControls.AddRange([PrefixIsNot, RequiredPrefix, SuffixIsNot, SurfaceIsNot, YieldToSurface]);
   }

   public async Task SaveAsync()
   {
      SaveToNote();

      // Check if we should prompt to reparse
      if(ShouldPromptToReparse() && _parentWindow != null)
      {
         var messageBox = MessageBoxManager.GetMessageBoxStandard(
            "Reparse Sentences?",
            "You changed settings that affect sentence parsing. Would you like to reparse sentences for this vocab now?",
            ButtonEnum.YesNo,
            Icon.Question);

         var result = await messageBox.ShowWindowDialogAsync(_parentWindow);

         if(result == ButtonResult.Yes)
         {
            // Trigger reparse using C# batch updater
            await TaskCE.Run(() =>
            {
               _services.LocalNoteUpdater.ReparseSentencesForVocab(_vocab);
            });
         }
      }

      // Note: UI refresh is handled by Python layer when dialog closes
   }

   void SaveToNote()
   {
      // Bool flags
      _vocab.MatchingConfiguration.BoolFlags.IsInflectingWord.SetTo(IsInflectingWord);
      _vocab.MatchingConfiguration.BoolFlags.IsPoisonWord.SetTo(IsPoisonWord);
      _vocab.MatchingConfiguration.BoolFlags.MatchWithPrecedingVowel.SetTo(MatchWithPrecedingVowel);
      _vocab.MatchingConfiguration.BoolFlags.QuestionOverridesForm.SetTo(QuestionOverridesForm);
      _vocab.MatchingConfiguration.BoolFlags.IsCompositionallyTransparentCompound.SetTo(IsCompositionallyTransparentCompound);

      // Register flags
      _vocab.Register.Archaic.SetTo(Archaic);
      _vocab.Register.Childish.SetTo(Childish);
      _vocab.Register.Derogatory.SetTo(Derogatory);
      _vocab.Register.Formal.SetTo(Formal);
      _vocab.Register.Honorific.SetTo(Honorific);
      _vocab.Register.Humble.SetTo(Humble);
      _vocab.Register.Informal.SetTo(Informal);
      _vocab.Register.Literary.SetTo(Literary);
      _vocab.Register.Polite.SetTo(Polite);
      _vocab.Register.RoughMasculine.SetTo(RoughMasculine);
      _vocab.Register.Sensitive.SetTo(Sensitive);
      _vocab.Register.Slang.SetTo(Slang);
      _vocab.Register.SoftFeminine.SetTo(SoftFeminine);
      _vocab.Register.Vulgar.SetTo(Vulgar);

      // Save string rules
      _vocab.MatchingConfiguration.ConfigurableRules.Save();

      // Note: Require/Forbid controls save directly to the note via their fields
   }

   bool ShouldPromptToReparse()
   {
      // Check if any reparse-triggering flag has changed
      var changedReparseFlags = AllRequireForbidControls.Any(c => c.RepraiseTrigger && c.HasChanged());
      var stringRulesModified = AllStringSetControls.Any(c => c.HasChanges());

      return changedReparseFlags || stringRulesModified;
   }
}
