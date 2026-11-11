using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.EditableFields;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.ViewModels.Workspace.Editors.MatchConditions;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// The "Matching" tab of a vocab's <see cref="VocabEditorSectionViewModel"/> (<see cref="VocabMatchingConfigEditor"/>):<br/>
/// the vocab's matching config, everything that decides how sentences are parsed into words and which words match this vocab: its match-condition,<br/>
/// edited as a tree (<see cref="MatchConditionEditor"/>), or through the matching rules it's made of, and the rules and flags outside it.<br/>
/// Every change is written to the note at once, and the vocab's sentences are then reparsed to reflect it. Each rule and flag is one of the<br/>
/// <see cref="VocabEditableFields"/>.
/// </summary>
partial class VocabMatchingConfigEditorViewModel : ObservableObject
{
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Func<VocabNote> _currentVocab;
   readonly VocabSentencesReparserViewModel _sentencesReparser;

   /// <summary>
   /// Whether the match-condition is made of matching rules only, so that the rules' editors can edit it. One with other conditions can only be<br/>
   /// edited as a tree.
   /// </summary>
   [ObservableProperty] bool _canBeEditedRuleByRule;

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while writing a change.</param>
   /// <param name="currentVocab">Finds the vocab being edited, as the collection holds it now.</param>
   /// <param name="sentencesReparser">Reparses the vocab's sentences after every change.</param>
   public VocabMatchingConfigEditorViewModel(IFatalErrorHandler fatalErrorHandler, Func<VocabNote> currentVocab, VocabSentencesReparserViewModel sentencesReparser)
   {
      _fatalErrorHandler = fatalErrorHandler;
      _currentVocab = currentVocab;
      _sentencesReparser = sentencesReparser;

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
      MatchConditionEditor = new MatchConditionEditorViewModel(fatalErrorHandler, currentVocab, ChangeMade);
      ShowWhetherTheMatchConditionCanBeEditedRuleByRule();
   }

   /// <summary>The vocab's match-condition, edited as a tree.</summary>
   public MatchConditionEditorViewModel MatchConditionEditor { get; }

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
      MatchConditionEditor.Refresh();
      ShowWhetherTheMatchConditionCanBeEditedRuleByRule();
      foreach(var rule in AllRequireForbidRules) rule.Refresh();
      foreach(var flag in AllFlags) flag.Refresh();
      foreach(var rule in AllTextSetRules) rule.Refresh();
   }

   IEnumerable<RequireForbidFlagEditorViewModel> AllRequireForbidRules => [YieldLastToken, DictionaryFormStart, ..MiscMatchingRules, ..StemMatchingRules];

   IEnumerable<FlagEditorViewModel> AllFlags => [..IsFlags, ..MiscFlags];

   IEnumerable<TextSetEditorViewModel> AllTextSetRules => [YieldToSurface, ..TextSetRules];

   void ShowWhetherTheMatchConditionCanBeEditedRuleByRule() => CanBeEditedRuleByRule = _currentVocab().MatchingConfiguration.Rules.IsMadeOfRulesOnly;

   /// <summary>
   /// Shows what a change made, in the tree or by a rule's or flag's editor, everywhere on the tab, as a rule changed in the tree shows in its editor,<br/>
   /// then reparses the vocab's sentences.
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
