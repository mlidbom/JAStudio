using System;
using System.Collections.Generic;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.EditableFields;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// The "Matching" tab of a vocab's <see cref="VocabEditorSectionViewModel"/> (<see cref="VocabMatchingConfigEditor"/>):<br/>
/// the vocab's matching config, everything that decides how sentences are parsed into words and which words match this vocab.<br/>
/// Every change is written to the note at once, and the vocab's sentences are then reparsed to reflect it. Each rule and flag is one of the<br/>
/// <see cref="VocabEditableFields"/>.
/// </summary>
class VocabMatchingConfigEditorViewModel
{
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Func<VocabNote> _currentVocab;
   readonly VocabSentencesReparserViewModel _sentencesReparser;

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while writing a change.</param>
   /// <param name="currentVocab">Finds the vocab being edited, as the collection holds it now.</param>
   /// <param name="sentencesReparser">Reparses the vocab's sentences after every change.</param>
   public VocabMatchingConfigEditorViewModel(IFatalErrorHandler fatalErrorHandler, Func<VocabNote> currentVocab, VocabSentencesReparserViewModel sentencesReparser)
   {
      _fatalErrorHandler = fatalErrorHandler;
      _currentVocab = currentVocab;
      _sentencesReparser = sentencesReparser;

      YieldLastToken = RequireForbidRule(VocabEditableFields.YieldLastToken);
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
         RequireForbidRule(VocabEditableFields.DictionaryFormStart),
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
         TextSetRule(VocabEditableFields.SurfaceIsNot),
         TextSetRule(VocabEditableFields.YieldToSurface)
      ];
   }

   public RequireForbidFlagEditorViewModel YieldLastToken { get; }
   public IReadOnlyList<RequireForbidFlagEditorViewModel> MiscMatchingRules { get; }
   public IReadOnlyList<RequireForbidFlagEditorViewModel> StemMatchingRules { get; }
   public IReadOnlyList<FlagEditorViewModel> IsFlags { get; }
   public IReadOnlyList<FlagEditorViewModel> MiscFlags { get; }
   public IReadOnlyList<TextSetEditorViewModel> TextSetRules { get; }

   /// <summary>Shows every rule as the note has it now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      foreach(var rule in AllRequireForbidRules) rule.Refresh();
      foreach(var flag in AllFlags) flag.Refresh();
      foreach(var rule in TextSetRules) rule.Refresh();
   }

   IEnumerable<RequireForbidFlagEditorViewModel> AllRequireForbidRules => [YieldLastToken, ..MiscMatchingRules, ..StemMatchingRules];

   IEnumerable<FlagEditorViewModel> AllFlags => [..IsFlags, ..MiscFlags];

   RequireForbidFlagEditorViewModel RequireForbidRule(EditableRequireForbidField field) =>
      new(_fatalErrorHandler, field.Label, () => field.RuleOn(_currentVocab()), _sentencesReparser.ReparseSentences);

   FlagEditorViewModel Flag(EditableFlagField<VocabNote> field) =>
      new(_fatalErrorHandler, field.Label, () => field.FlagOn(_currentVocab()), _sentencesReparser.ReparseSentences);

   TextSetEditorViewModel TextSetRule(EditableTextSetField<VocabNote> field) =>
      new(field.Label,
          () => field.Read(_currentVocab()),
          text => field.Add(_currentVocab(), text),
          text => field.Remove(_currentVocab(), text),
          _sentencesReparser.ReparseSentences);
}
