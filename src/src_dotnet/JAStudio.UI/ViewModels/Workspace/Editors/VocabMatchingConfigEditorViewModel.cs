using System;
using System.Collections.Generic;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// The "Matching" tab of a vocab's <see cref="VocabEditorSectionViewModel"/> (<see cref="VocabMatchingConfigEditor"/>):<br/>
/// the vocab's matching config, everything that decides how sentences are parsed into words and which words match this vocab.<br/>
/// Every change is written to the note at once, and the vocab's sentences are then reparsed to reflect it.
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

   public RequireForbidFlagEditorViewModel YieldLastToken { get; }
   public IReadOnlyList<RequireForbidFlagEditorViewModel> MiscMatchingRules { get; }
   public IReadOnlyList<RequireForbidFlagEditorViewModel> StemMatchingRules { get; }
   public IReadOnlyList<TagFlagEditorViewModel> IsFlags { get; }
   public IReadOnlyList<TagFlagEditorViewModel> MiscFlags { get; }
   public IReadOnlyList<StringSetEditorViewModel> StringRules { get; }

   /// <summary>Shows every rule as the note has it now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      foreach(var rule in AllRequireForbidRules) rule.Refresh();
      foreach(var flag in AllFlags) flag.Refresh();
      foreach(var rule in StringRules) rule.Refresh();
   }

   IEnumerable<RequireForbidFlagEditorViewModel> AllRequireForbidRules => [YieldLastToken, ..MiscMatchingRules, ..StemMatchingRules];

   IEnumerable<TagFlagEditorViewModel> AllFlags => [..IsFlags, ..MiscFlags];

   RequireForbidFlagEditorViewModel RequireForbidRule(string label, Func<VocabMatchingRulesConfigurationRequiresForbidsFlags, RequireForbidFlagField> rule) =>
      new(_fatalErrorHandler, label, () => rule(_currentVocab().MatchingConfiguration.RequiresForbids), _sentencesReparser.ReparseSentences);

   TagFlagEditorViewModel Flag(string label, Func<VocabMatchingRulesConfigurationBoolFlags, TagFlagField> flag) =>
      new(_fatalErrorHandler, label, () => flag(_currentVocab().MatchingConfiguration.BoolFlags), _sentencesReparser.ReparseSentences);

   StringSetEditorViewModel StringRule(string title, Func<VocabNoteMatchingRules, HashSet<string>> rule) =>
      new(title,
          () => rule(CurrentRules()),
          value => ChangeAndSave(rules => rule(rules).Add(value)),
          value => ChangeAndSave(rules => rule(rules).Remove(value)),
          _sentencesReparser.ReparseSentences);

   VocabNoteMatchingRules CurrentRules() => _currentVocab().MatchingConfiguration.ConfigurableRules;

   /// <summary>Changes the vocab's configurable rules, then saves the note: changing a rule's set itself saves nothing.</summary>
   void ChangeAndSave(Action<VocabNoteMatchingRules> change)
   {
      var rules = CurrentRules();
      change(rules);
      rules.Save();
   }
}
