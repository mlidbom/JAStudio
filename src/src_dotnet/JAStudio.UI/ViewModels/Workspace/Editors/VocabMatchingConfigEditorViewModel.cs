using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// The "Matching" tab of a vocab's <see cref="VocabEditorSectionViewModel"/> (<see cref="VocabMatchingConfigEditor"/>):<br/>
/// the vocab's matching config, everything that decides how sentences are parsed into words and which words match this vocab.<br/>
/// Every change is written to the note at once, and the vocab's sentences are then reparsed to reflect it.
/// </summary>
/// <remarks>
/// While the sentences are reparsed, on a background thread, the rules can't be changed (<see cref="IsReparsingSentences"/>):<br/>
/// the reparse reads the vocab's tags, which a change would modify at the same time from the UI thread.
/// </remarks>
partial class VocabMatchingConfigEditorViewModel : ObservableObject
{
   readonly TemporaryServiceCollection _services;
   readonly Func<VocabNote> _currentVocab;
   readonly Action _sentencesReparsed;
   readonly IUIThreadDispatcher _uiThread;
   readonly IFatalErrorHandler _fatalErrorHandler;

   /// <param name="services">The application's services.</param>
   /// <param name="currentVocab">Finds the vocab being edited, as the collection holds it now.</param>
   /// <param name="sentencesReparsed">Called on the UI thread after a change's reparse has finished, so the vocab's page can show the sentences it now matches.</param>
   public VocabMatchingConfigEditorViewModel(TemporaryServiceCollection services, Func<VocabNote> currentVocab, Action sentencesReparsed)
   {
      _services = services;
      _currentVocab = currentVocab;
      _sentencesReparsed = sentencesReparsed;
      _uiThread = services.Resolve<IUIThreadDispatcher>();
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();

      YieldLastToken = RequireForbidRule("Yield to overlapping following compound", it => it.YieldLastToken);
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
         Flag("Inflecting word", it => it.IsInflectingWord),
         Flag("Poison word", it => it.IsPoisonWord)
      ];
      MiscFlags =
      [
         Flag("Match with preceding vowel", it => it.MatchWithPrecedingVowel),
         Flag("Question overrides form: Show the question in results even if the match was another form", it => it.QuestionOverridesForm)
      ];
      StringRules =
      [
         StringRule("Prefix is not", it => it.PrefixIsNot),
         StringRule("Required prefix", it => it.RequiredPrefix),
         StringRule("Suffix is not", it => it.SuffixIsNot),
         StringRule("Surface is not", it => it.SurfaceIsNot),
         StringRule("Yield to surface", it => it.YieldToSurface)
      ];
   }

   public RequireForbidControlViewModel YieldLastToken { get; }
   public IReadOnlyList<RequireForbidControlViewModel> MiscMatchingRules { get; }
   public IReadOnlyList<RequireForbidControlViewModel> StemMatchingRules { get; }
   public IReadOnlyList<TagFlagCheckBoxViewModel> IsFlags { get; }
   public IReadOnlyList<TagFlagCheckBoxViewModel> MiscFlags { get; }
   public IReadOnlyList<StringSetControlViewModel> StringRules { get; }

   /// <summary>Whether the vocab's sentences are being reparsed after a change. The rules can't be changed meanwhile.</summary>
   [ObservableProperty] bool _isReparsingSentences;

   /// <summary>Shows every rule as the note has it now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      foreach(var rule in AllRequireForbidRules) rule.Refresh();
      foreach(var flag in AllFlags) flag.Refresh();
      foreach(var rule in StringRules) rule.Refresh();
   }

   IEnumerable<RequireForbidControlViewModel> AllRequireForbidRules => [YieldLastToken, ..MiscMatchingRules, ..StemMatchingRules];

   IEnumerable<TagFlagCheckBoxViewModel> AllFlags => [..IsFlags, ..MiscFlags];

   /// <summary>
   /// Reparses the sentences that may contain the vocab, in the background and without the progress dialog:<br/>
   /// the editor shows its own progress, and a dialog popping up after every click would be in the way.
   /// </summary>
   void ReparseSentences()
   {
      IsReparsingSentences = true;
      var vocab = _currentVocab();
      _services.BackgroundTaskManager.Run(() =>
      {
         try
         {
            _services.LocalNoteUpdater.ReparseSentencesForVocabWithoutProgressDialog(vocab);
         }
         finally
         {
            _uiThread.PostToUIThread(ReparseFinished);
         }
      });
   }

   void ReparseFinished()
   {
      IsReparsingSentences = false;
      _sentencesReparsed();
   }

   RequireForbidControlViewModel RequireForbidRule(string label, Func<VocabMatchingRulesConfigurationRequiresForbidsFlags, RequireForbidFlagField> rule) =>
      new(_fatalErrorHandler, label, () => rule(_currentVocab().MatchingConfiguration.RequiresForbids), ReparseSentences);

   TagFlagCheckBoxViewModel Flag(string label, Func<VocabMatchingRulesConfigurationBoolFlags, TagFlagField> flag) =>
      new(_fatalErrorHandler, label, () => flag(_currentVocab().MatchingConfiguration.BoolFlags), ReparseSentences);

   StringSetControlViewModel StringRule(string title, Func<VocabNoteMatchingRules, HashSet<string>> rule) =>
      new(title,
          () => rule(_currentVocab().MatchingConfiguration.ConfigurableRules),
          () => _currentVocab().MatchingConfiguration.ConfigurableRules.Save(),
          ReparseSentences);
}
