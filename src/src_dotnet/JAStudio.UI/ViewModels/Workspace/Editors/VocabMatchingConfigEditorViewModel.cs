using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.EditableFields;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;
using JAStudio.UI.Views.Workspace.Editors;
using JAStudio.UI.Views.Workspace.Editors.Matching;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// The "Matching" tab of a vocab's <see cref="VocabEditorSectionViewModel"/> (<see cref="VocabMatchingConfigEditor"/>):<br/>
/// the vocab's matching config, everything that decides how sentences are parsed into words and which words match this vocab: its match-condition,<br/>
/// shown as code (<see cref="ConditionCodeView"/>) and edited as a tree in its own window (<see cref="MatchConditionWindow"/>), or through the matching<br/>
/// rules it's made of, and the rules and flags outside it. Every change is written to the note at once, and the vocab's sentences are then reparsed to<br/>
/// reflect it. Each rule and flag is one of the <see cref="VocabEditableFields"/>.
/// </summary>
partial class VocabMatchingConfigEditorViewModel : ObservableObject
{
   const string HowToEditTheMatchCondition = "Double-click it, or press Ctrl+M, to edit it in its window. Ctrl+click on a shared-condition opens its definition.";

   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Func<VocabNote> _currentVocab;
   readonly VocabSentencesReparserViewModel _sentencesReparser;
   readonly Action _openTheMatchConditionWindow;

   /// <summary>
   /// Whether the match-condition is made of matching rules only, so that the rules' editors can edit it. One with other conditions can only be<br/>
   /// edited as a tree.
   /// </summary>
   [ObservableProperty] bool _canBeEditedRuleByRule;

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while writing a change.</param>
   /// <param name="currentVocab">Finds the vocab being edited, as the collection holds it now.</param>
   /// <param name="sentencesReparser">Reparses the vocab's sentences after every change.</param>
   /// <param name="openTheDefinitionOf">Opens a shared-condition's window, where its definition is edited.</param>
   /// <param name="openTheMatchConditionWindow">Opens the vocab's match-condition window, where its match-condition is edited as a tree.</param>
   public VocabMatchingConfigEditorViewModel(IFatalErrorHandler fatalErrorHandler,
                                             Func<VocabNote> currentVocab,
                                             VocabSentencesReparserViewModel sentencesReparser,
                                             Action<ISharedCondition> openTheDefinitionOf,
                                             Action openTheMatchConditionWindow)
   {
      _fatalErrorHandler = fatalErrorHandler;
      _currentVocab = currentVocab;
      _sentencesReparser = sentencesReparser;
      _openTheMatchConditionWindow = openTheMatchConditionWindow;

      YieldLastToken = RequireForbidRule(VocabEditableFields.YieldLastToken);
      YieldToSurface = TextSetRule(VocabEditableFields.YieldToSurface);
      DictionaryFormStart = RequireForbidRule(VocabEditableFields.DictionaryFormStart);
      MiscMatchingRules =
      [
         RequireForbidRule(VocabEditableFields.Surface),
         RequireForbidRule(VocabEditableFields.SentenceEnd),
         RequireForbidRule(VocabEditableFields.SentenceStart),
         RequireForbidRule(VocabEditableFields.SingleToken)
      ];
      StemMatchingRules =
      [
         RequireForbidRule(VocabEditableFields.GodanImperative),
         RequireForbidRule(VocabEditableFields.GodanImperativeHead),
         RequireForbidRule(VocabEditableFields.GodanPotential),
         RequireForbidRule(VocabEditableFields.IchidanImperative),
         RequireForbidRule(VocabEditableFields.MasuStem),
         RequireForbidRule(VocabEditableFields.Godan),
         RequireForbidRule(VocabEditableFields.Ichidan),
         RequireForbidRule(VocabEditableFields.Irrealis),
         RequireForbidRule(VocabEditableFields.PastTenseStem),
         RequireForbidRule(VocabEditableFields.DictionaryFormHead),
         RequireForbidRule(VocabEditableFields.AdverbHead),
         RequireForbidRule(VocabEditableFields.TeFormStemStart),
         RequireForbidRule(VocabEditableFields.TeFormStemHead)
      ];
      IsFlags =
      [
         Flag(VocabEditableFields.IsInflectingWord),
         Flag(VocabEditableFields.IsPoisonWord)
      ];
      MiscFlags =
      [
         Flag(VocabEditableFields.MatchWithPrecedingVowel),
         Flag(VocabEditableFields.QuestionOverridesForm)
      ];
      TextSetRules =
      [
         TextSetRule(VocabEditableFields.PrefixIsNot),
         TextSetRule(VocabEditableFields.RequiredPrefix),
         TextSetRule(VocabEditableFields.SuffixIsNot),
         TextSetRule(VocabEditableFields.SurfaceIsNot)
      ];
      MatchCondition = new ConditionCodeViewModel(TheMatchConditionsLines(), openTheDefinitionOf);
      ShowWhetherTheMatchConditionCanBeEditedRuleByRule();
   }

   /// <summary>The vocab's match-condition, shown as code, read-only: it's edited in its window (<see cref="OpenTheMatchConditionWindow"/>).</summary>
   public ConditionCodeViewModel MatchCondition { get; }

   /// <summary>How to edit the match-condition, shown under it.</summary>
   public string HowToEdit => HowToEditTheMatchCondition;

   /// <summary>Opens the vocab's match-condition window, where its match-condition is edited as a tree, as a double-click on it or Ctrl+M does.</summary>
   public void OpenTheMatchConditionWindow() => _openTheMatchConditionWindow();

   public RequireForbidFlagEditorViewModel YieldLastToken { get; }
   public TextSetEditorViewModel YieldToSurface { get; }

   /// <summary>Dictionary form (start), a require/forbid rule outside the match-condition: see <see cref="Core.Note.Vocabulary.Matching.VocabMatchingRequireForbidFlags.DictionaryFormStart"/>.</summary>
   public RequireForbidFlagEditorViewModel DictionaryFormStart { get; }

   public IReadOnlyList<RequireForbidFlagEditorViewModel> MiscMatchingRules { get; }
   public IReadOnlyList<RequireForbidFlagEditorViewModel> StemMatchingRules { get; }
   public IReadOnlyList<FlagEditorViewModel> IsFlags { get; }
   public IReadOnlyList<FlagEditorViewModel> MiscFlags { get; }
   public IReadOnlyList<TextSetEditorViewModel> TextSetRules { get; }

   /// <summary>Shows the match-condition, and every rule and flag, as the note has it now, after a change made elsewhere or on this tab.</summary>
   public void Refresh()
   {
      MatchCondition.Show(TheMatchConditionsLines());
      ShowWhetherTheMatchConditionCanBeEditedRuleByRule();
      foreach(var rule in AllRequireForbidRules) rule.Refresh();
      foreach(var flag in AllFlags) flag.Refresh();
      foreach(var rule in AllTextSetRules) rule.Refresh();
   }

   IEnumerable<RequireForbidFlagEditorViewModel> AllRequireForbidRules => [YieldLastToken, DictionaryFormStart, ..MiscMatchingRules, ..StemMatchingRules];

   IEnumerable<FlagEditorViewModel> AllFlags => [..IsFlags, ..MiscFlags];

   IEnumerable<TextSetEditorViewModel> AllTextSetRules => [YieldToSurface, ..TextSetRules];

   void ShowWhetherTheMatchConditionCanBeEditedRuleByRule() => CanBeEditedRuleByRule = _currentVocab().MatchingConfiguration.Rules.IsMadeOfRulesOnly;

   IReadOnlyList<CodeLine> TheMatchConditionsLines() => ConditionCode.LinesOfAMatchCondition(_currentVocab().MatchingConfiguration.MatchCondition);

   /// <summary>
   /// Shows what a change made by a rule's or flag's editor everywhere on the tab, as a rule changed shows in the match-condition, then reparses the<br/>
   /// vocab's sentences.
   /// </summary>
   void ChangeMade()
   {
      Refresh();
      _sentencesReparser.ReparseSentences();
   }

   RequireForbidFlagEditorViewModel RequireForbidRule(EditableRequireForbidField field) =>
      new(_fatalErrorHandler, field.Label, () => field.Read(_currentVocab()), choice => field.Write(_currentVocab(), choice), ChangeMade);

   FlagEditorViewModel Flag(EditableFlagField<VocabNote> field) =>
      new(_fatalErrorHandler, field.Label, () => field.FlagOn(_currentVocab()), ChangeMade);

   TextSetEditorViewModel TextSetRule(EditableTextSetField<VocabNote> field) =>
      new(field.Label,
          () => field.Read(_currentVocab()),
          text => field.Add(_currentVocab(), text),
          text => field.Remove(_currentVocab(), text),
          ChangeMade);
}
