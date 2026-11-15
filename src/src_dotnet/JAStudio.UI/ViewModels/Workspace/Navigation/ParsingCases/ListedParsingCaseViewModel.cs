using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Compze.Contracts;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Core.SysUtils;
using JAStudio.UI.ViewModels.Workspace.ParsingCases;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.ParsingCases;

/// <summary>
/// A parsing case as a row of the navigation pane's list of them shows it (<see cref="ParsingCasesViewModel"/>): how its sentence parses compared<br/>
/// with what the case expects, a person's verdict on it, its name and its sentence. The row selected also shows its why and its differences below<br/>
/// the list.
/// </summary>
/// <remarks>
/// A row shows its case as it was last compared (<see cref="Show"/>), and stays the same row through every comparison, so that the list keeps it<br/>
/// selected.
/// </remarks>
sealed class ListedParsingCaseViewModel(ComparedParsingCase compared) : ObservableObject
{
   ComparedParsingCase _compared = compared;

   /// <summary>The sentence that is the parsing case, which opening the row shows.</summary>
   public SentenceNote Sentence => _compared.Sentence;

   /// <summary>How the sentence parses compared with what the case expects. The list sorts by it, those that fail first.</summary>
   public ParsingCaseOutcome Outcome => _compared.Comparison.Outcome;

   /// <summary>The <see cref="Outcome"/> in words, such as "to review".</summary>
   public string OutcomeText => Outcome.InWords();

   public bool Fails => _compared.Comparison.Fails;
   public bool IsToReview => _compared.Comparison.IsToReview;

   /// <summary>What a person has found about the vocab the case expects the breakdown to display.</summary>
   public Verdict Verdict => _compared.Case.VerdictOnTheDisplayedVocab;

   /// <summary>The <see cref="Verdict"/> in words, such as "verified".</summary>
   public string VerdictText => Verdict.InWords();

   public bool IsBroken => Verdict == Verdict.Broken;

   /// <summary>What the case is about.</summary>
   public string Name => _compared.Case.Name;

   /// <summary>The sentence's question, without markup.</summary>
   public string Question => Sentence.GetQuestion().StripHtmlAndBracketMarkupAndNoiseCharacters();

   /// <summary>Why the case exists, or what is wrong with how it parses.</summary>
   public string Why => _compared.Case.Why;

   public bool HasAWhy => Why.Length > 0;

   /// <summary>How what the sentence parses to differs from what the case expects, a line per difference.</summary>
   public IReadOnlyList<string> Differences => _compared.Comparison.Differences;

   public bool HasDifferences => Differences.Count > 0;

   /// <summary>Makes the row show <paramref name="compared"/>: the same case, compared again.</summary>
   public void Show(ComparedParsingCase compared)
   {
      Contract.Argument.Assert(compared.Sentence.GetId() == Sentence.GetId(),
                               () => $"A row shows one parsing case, {Sentence.GetId()}, and can't be made to show the parsing case {compared.Sentence.GetId()}.");
      _compared = compared;
      TellTheListThatEveryValueChanged();
   }

   /// <summary>An empty property name says that every property may have changed, so the list reads every value of the row again.</summary>
   void TellTheListThatEveryValueChanged() => OnPropertyChanged(string.Empty);
}
