using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Compze.Internals.Logging;
using JAStudio.Core;
using JAStudio.Core.Configuration;

namespace JAStudio.UI.ViewModels;

/// <summary>
/// ViewModel for the Japanese Options dialog.
/// Provides two-way bindings to JapaneseConfig values.
/// Changes are automatically saved via ConfigurationValue SetValue().
/// </summary>
partial class OptionsDialogViewModel : ObservableObject
{
   readonly Window _window;
   readonly JapaneseConfig _config;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
   [Obsolete("Parameterless constructor is only for XAML designer support and should not be used directly.")]
   public OptionsDialogViewModel() {}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

   public OptionsDialogViewModel(Window window, TemporaryServiceCollection services)
   {
      this.Log().Info("OptionsDialogViewModel constructor: starting...");
      _window = window;

      _config = services.CoreApp.Config;
      this.Log().Info("OptionsDialogViewModel constructor: calling LoadFromConfig()...");
      LoadFromConfig();
      this.Log().Info("OptionsDialogViewModel constructor: completed");
   }

   // --- Numeric Values ---

   [ObservableProperty] public partial double AutoadvanceVocabStartingSeconds { get; set; }

   partial void OnAutoadvanceVocabStartingSecondsChanged(double value) =>
      _config.AutoadvanceVocabStartingSeconds.Value = (float)value;

   [ObservableProperty] public partial double AutoadvanceVocabHiraganaSeconds { get; set; }

   partial void OnAutoadvanceVocabHiraganaSecondsChanged(double value) =>
      _config.AutoadvanceVocabHiraganaSeconds.Value = (float)value;

   [ObservableProperty] public partial double AutoadvanceVocabKatakanaSeconds { get; set; }

   partial void OnAutoadvanceVocabKatakanaSecondsChanged(double value) =>
      _config.AutoadvanceVocabKatakanaSeconds.Value = (float)value;

   [ObservableProperty] public partial double AutoadvanceVocabKanjiSeconds { get; set; }

   partial void OnAutoadvanceVocabKanjiSecondsChanged(double value) =>
      _config.AutoadvanceVocabKanjiSeconds.Value = (float)value;

   [ObservableProperty] public partial double AutoadvanceSentenceStartingSeconds { get; set; }

   partial void OnAutoadvanceSentenceStartingSecondsChanged(double value) =>
      _config.AutoadvanceSentenceStartingSeconds.Value = (float)value;

   [ObservableProperty] public partial double AutoadvanceSentenceHiraganaSeconds { get; set; }

   partial void OnAutoadvanceSentenceHiraganaSecondsChanged(double value) =>
      _config.AutoadvanceSentenceHiraganaSeconds.Value = (float)value;

   [ObservableProperty] public partial double AutoadvanceSentenceKatakanaSeconds { get; set; }

   partial void OnAutoadvanceSentenceKatakanaSecondsChanged(double value) =>
      _config.AutoadvanceSentenceKatakanaSeconds.Value = (float)value;

   [ObservableProperty] public partial double AutoadvanceSentenceKanjiSeconds { get; set; }

   partial void OnAutoadvanceSentenceKanjiSecondsChanged(double value) =>
      _config.AutoadvanceSentenceKanjiSeconds.Value = (float)value;

   [ObservableProperty] public partial double BoostFailedCardAllowedTimeByFactor { get; set; }

   partial void OnBoostFailedCardAllowedTimeByFactorChanged(double value) =>
      _config.BoostFailedCardAllowedTimeByFactor.Value = (float)value;

   [ObservableProperty] public partial long DecreaseFailedCardIntervalsInterval { get; set; }

   partial void OnDecreaseFailedCardIntervalsIntervalChanged(long value) =>
      _config.DecreaseFailedCardIntervalsInterval.Value = value;

   [ObservableProperty] public partial long TimeboxSentenceRead { get; set; }

   partial void OnTimeboxSentenceReadChanged(long value) =>
      _config.TimeboxSentenceRead.Value = value;

   [ObservableProperty] public partial long TimeboxSentenceListen { get; set; }

   partial void OnTimeboxSentenceListenChanged(long value) =>
      _config.TimeboxSentenceListen.Value = value;

   [ObservableProperty] public partial long TimeboxVocabRead { get; set; }

   partial void OnTimeboxVocabReadChanged(long value) =>
      _config.TimeboxVocabRead.Value = value;

   [ObservableProperty] public partial long TimeboxVocabListen { get; set; }

   partial void OnTimeboxVocabListenChanged(long value) =>
      _config.TimeboxVocabListen.Value = value;

   [ObservableProperty] public partial long TimeboxKanjiRead { get; set; }

   partial void OnTimeboxKanjiReadChanged(long value) =>
      _config.TimeboxKanjiRead.Value = value;

   [ObservableProperty] public partial double MinimumTimeViewingQuestion { get; set; }

   partial void OnMinimumTimeViewingQuestionChanged(double value) =>
      _config.MinimumTimeViewingQuestion.Value = (float)value;

   [ObservableProperty] public partial double MinimumTimeViewingAnswer { get; set; }

   partial void OnMinimumTimeViewingAnswerChanged(double value) =>
      _config.MinimumTimeViewingAnswer.Value = (float)value;

   // --- Sentence Display Toggles ---

   [ObservableProperty] public partial bool ShowKanjiInSentenceBreakdown { get; set; }

   partial void OnShowKanjiInSentenceBreakdownChanged(bool value) =>
      _config.ShowKanjiInSentenceBreakdown.Value = value;

   [ObservableProperty] public partial bool ShowCompoundPartsInSentenceBreakdown { get; set; }

   partial void OnShowCompoundPartsInSentenceBreakdownChanged(bool value) =>
      _config.ShowCompoundPartsInSentenceBreakdown.Value = value;

   [ObservableProperty] public partial bool ShowKanjiMnemonicsInSentenceBreakdown { get; set; }

   partial void OnShowKanjiMnemonicsInSentenceBreakdownChanged(bool value) =>
      _config.ShowKanjiMnemonicsInSentenceBreakdown.Value = value;

   [ObservableProperty] public partial bool HideCompositionallyTransparentCompounds { get; set; }

   partial void OnHideCompositionallyTransparentCompoundsChanged(bool value) =>
      _config.HideCompositionallyTransparentCompounds.Value = value;

   [ObservableProperty] public partial bool AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound { get; set; }

   partial void OnAutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompoundChanged(bool value) =>
      _config.AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound.Value = value;

   [ObservableProperty] public partial bool AutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound { get; set; }

   partial void OnAutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompoundChanged(bool value) =>
      _config.AutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound.Value = value;

   [ObservableProperty] public partial bool AutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound { get; set; }

   partial void OnAutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompoundChanged(bool value) =>
      _config.AutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound.Value = value;

   [ObservableProperty] public partial bool HideAllCompounds { get; set; }

   partial void OnHideAllCompoundsChanged(bool value) =>
      _config.HideAllCompounds.Value = value;

   [ObservableProperty] public partial bool ShowSentenceBreakdownInEditMode { get; set; }

   partial void OnShowSentenceBreakdownInEditModeChanged(bool value) =>
      _config.ShowSentenceBreakdownInEditMode.Value = value;

   // --- Misc Toggles ---

   [ObservableProperty] public partial bool YomitanIntegrationCopyAnswerToClipboard { get; set; }

   partial void OnYomitanIntegrationCopyAnswerToClipboardChanged(bool value) =>
      _config.YomitanIntegrationCopyAnswerToClipboard.Value = value;

   [ObservableProperty] public partial bool DecreaseFailedCardIntervals { get; set; }

   partial void OnDecreaseFailedCardIntervalsChanged(bool value) =>
      _config.DecreaseFailedCardIntervals.Value = value;

   [ObservableProperty] public partial bool PreventDoubleClicks { get; set; }

   partial void OnPreventDoubleClicksChanged(bool value) =>
      _config.PreventDoubleClicks.Value = value;

   [ObservableProperty] public partial bool BoostFailedCardAllowedTime { get; set; }

   partial void OnBoostFailedCardAllowedTimeChanged(bool value) =>
      _config.BoostFailedCardAllowedTime.Value = value;


   // --- Performance and Memory Toggles ---

   [ObservableProperty] public partial bool LoadStudioInForeground { get; set; }

   partial void OnLoadStudioInForegroundChanged(bool value) =>
      _config.LoadStudioInForeground.Value = value;

   [ObservableProperty] public partial bool PreCacheCardStudyingStatus { get; set; }

   partial void OnPreCacheCardStudyingStatusChanged(bool value) =>
      _config.PreCacheCardStudyingStatus.Value = value;

   [ObservableProperty] public partial long ReanalysisThreads { get; set; }

   partial void OnReanalysisThreadsChanged(long value) =>
      _config.ReanalysisThreads.Value = value;

   // --- Developer Only Toggles ---

   [ObservableProperty] public partial bool EnableTraceMalloc { get; set; }

   partial void OnEnableTraceMallocChanged(bool value) =>
      _config.EnableTraceMalloc.Value = value;

   [ObservableProperty] public partial bool TrackInstancesInMemory { get; set; }

   partial void OnTrackInstancesInMemoryChanged(bool value) =>
      _config.TrackInstancesInMemory.Value = value;

   [ObservableProperty] public partial bool LogWhenFlushingNotes { get; set; }

   partial void OnLogWhenFlushingNotesChanged(bool value) =>
      _config.LogWhenFlushingNotes.Value = value;

   // --- Commands ---

   [RelayCommand] void Close()
   {
      _window.Close();
   }

   // --- Load from Config ---

   void LoadFromConfig()
   {
      // Numeric values
      AutoadvanceVocabStartingSeconds = _config.AutoadvanceVocabStartingSeconds.Value;
      AutoadvanceVocabHiraganaSeconds = _config.AutoadvanceVocabHiraganaSeconds.Value;
      AutoadvanceVocabKatakanaSeconds = _config.AutoadvanceVocabKatakanaSeconds.Value;
      AutoadvanceVocabKanjiSeconds = _config.AutoadvanceVocabKanjiSeconds.Value;

      AutoadvanceSentenceStartingSeconds = _config.AutoadvanceSentenceStartingSeconds.Value;
      AutoadvanceSentenceHiraganaSeconds = _config.AutoadvanceSentenceHiraganaSeconds.Value;
      AutoadvanceSentenceKatakanaSeconds = _config.AutoadvanceSentenceKatakanaSeconds.Value;
      AutoadvanceSentenceKanjiSeconds = _config.AutoadvanceSentenceKanjiSeconds.Value;

      BoostFailedCardAllowedTimeByFactor = _config.BoostFailedCardAllowedTimeByFactor.Value;
      DecreaseFailedCardIntervalsInterval = _config.DecreaseFailedCardIntervalsInterval.Value;

      TimeboxSentenceRead = _config.TimeboxSentenceRead.Value;
      TimeboxSentenceListen = _config.TimeboxSentenceListen.Value;
      TimeboxVocabRead = _config.TimeboxVocabRead.Value;
      TimeboxVocabListen = _config.TimeboxVocabListen.Value;
      TimeboxKanjiRead = _config.TimeboxKanjiRead.Value;

      MinimumTimeViewingQuestion = _config.MinimumTimeViewingQuestion.Value;
      MinimumTimeViewingAnswer = _config.MinimumTimeViewingAnswer.Value;

      // Sentence Display
      ShowKanjiInSentenceBreakdown = _config.ShowKanjiInSentenceBreakdown.Value;
      ShowCompoundPartsInSentenceBreakdown = _config.ShowCompoundPartsInSentenceBreakdown.Value;
      ShowKanjiMnemonicsInSentenceBreakdown = _config.ShowKanjiMnemonicsInSentenceBreakdown.Value;
      HideCompositionallyTransparentCompounds = _config.HideCompositionallyTransparentCompounds.Value;
      AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound = _config.AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound.Value;
      AutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound = _config.AutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound.Value;
      AutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound = _config.AutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound.Value;
      HideAllCompounds = _config.HideAllCompounds.Value;
      ShowSentenceBreakdownInEditMode = _config.ShowSentenceBreakdownInEditMode.Value;

      // Misc
      YomitanIntegrationCopyAnswerToClipboard = _config.YomitanIntegrationCopyAnswerToClipboard.Value;
      DecreaseFailedCardIntervals = _config.DecreaseFailedCardIntervals.Value;
      PreventDoubleClicks = _config.PreventDoubleClicks.Value;
      BoostFailedCardAllowedTime = _config.BoostFailedCardAllowedTime.Value;

      // Performance
      LoadStudioInForeground = _config.LoadStudioInForeground.Value;
      PreCacheCardStudyingStatus = _config.PreCacheCardStudyingStatus.Value;
      ReanalysisThreads = _config.ReanalysisThreads.Value;

      // Developer Only
      EnableTraceMalloc = _config.EnableTraceMalloc.Value;
      TrackInstancesInMemory = _config.TrackInstancesInMemory.Value;
      LogWhenFlushingNotes = _config.LogWhenFlushingNotes.Value;
   }
}
