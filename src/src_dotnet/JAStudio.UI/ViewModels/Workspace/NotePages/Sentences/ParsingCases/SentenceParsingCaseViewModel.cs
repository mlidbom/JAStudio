using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Compze.Teventive.TaggregateStores;
using JAStudio.Core;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Core.TaskRunners;

namespace JAStudio.UI.ViewModels.Workspace.NotePages.Sentences.ParsingCases;

/// <summary>
/// The parsing case section of a sentence's page, for a sentence that is a parsing case (<see cref="ParsingCase"/>): what the case is about and why<br/>
/// it exists, a person's verdict on the vocab it expects the breakdown to display, given with a click, and how the sentence parses now compared<br/>
/// with what the case expects. When it parses otherwise: its differences, a line each, accepting what it parses to, and whether the page shows<br/>
/// the expected breakdown beside the sentence's own.
/// </summary>
/// <remarks>
/// It shows the case as it was when the page was made, as the page does its sentence. Each of its actions changes the sentence or a setting, and the<br/>
/// tab then makes the page anew.
/// </remarks>
sealed partial class SentenceParsingCaseViewModel
{
   readonly ITaggregateReference<SentenceNote> _sentence;
   readonly JapaneseConfig _config;
   readonly IFatalErrorHandler _fatalErrorHandler;

   /// <summary>The sentence as it is now.</summary>
   SentenceNote Sentence => _sentence.Current;

   /// <param name="sentence">The sentence, a parsing case.</param>
   /// <param name="comparison">How <paramref name="sentence"/> parses now, compared with what its parsing case expects.</param>
   /// <param name="services">The application's services.</param>
   public SentenceParsingCaseViewModel(SentenceNote sentence, ParsingCaseComparison comparison, TemporaryServiceCollection services)
   {
      _sentence = sentence.Reference();
      _config = services.CoreApp.Config;
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
      var parsingCase = comparison.Case;

      Name = parsingCase.Name.Length > 0 ? parsingCase.Name : null;
      Why = parsingCase.Why.Length > 0 ? parsingCase.Why : null;
      Verdicts = [.. new[] { Verdict.Verified, Verdict.Unverified, Verdict.Broken }.Select(it => new VerdictChoiceViewModel(it, sentence, _fatalErrorHandler))];
      Outcome = comparison.Outcome;
      Differences = comparison.Differences;
   }

   /// <summary>What the case is about. Null until a person names it, in the sentence's editor.</summary>
   public string? Name { get; }

   public bool IsNamed => Name != null;

   /// <summary>Why the case exists, or what is wrong with how it parses. Null until a person says, in the sentence's editor.</summary>
   public string? Why { get; }

   /// <summary>The three verdicts on the vocab the case expects the breakdown to display, the one given chosen. A click gives another.</summary>
   public IReadOnlyList<VerdictChoiceViewModel> Verdicts { get; }

   /// <summary>How the sentence parses now, compared with what the case expects.</summary>
   public ParsingCaseOutcome Outcome { get; }

   public bool Fails => Outcome == ParsingCaseOutcome.Fails;
   public bool IsToReview => Outcome == ParsingCaseOutcome.IsToReview;
   public bool ParsesAsExpected => Outcome == ParsingCaseOutcome.ParsesAsExpected;

   /// <summary>The <see cref="Outcome"/>, said in a sentence.</summary>
   public string OutcomeText => Outcome switch
   {
      ParsingCaseOutcome.Fails            => "Fails: the breakdown displays other vocab than the ones verified.",
      ParsingCaseOutcome.IsToReview       => "To review: it parses otherwise than expected, in what nobody has verified.",
      ParsingCaseOutcome.ParsesAsExpected => "Parses as expected.",
      _                                   => throw new ArgumentOutOfRangeException(nameof(Outcome), Outcome, "A parsing case has no such outcome.")
   };

   /// <summary>How what the sentence parses to differs from what the case expects, a line per difference. Empty when it parses as expected.</summary>
   public IReadOnlyList<string> Differences { get; }

   /// <summary>
   /// Whether the page shows the breakdown the case expects beside the sentence's own, when the two differ in any way<br/>
   /// (<see cref="JapaneseConfig.ShowWhatAParsingCaseExpectsBesideTheBreakdown"/>). Bound to its check box, for every sentence's page at once.
   /// </summary>
   public bool ShowsWhatItExpectsBesideTheBreakdown
   {
      get => _config.ShowWhatAParsingCaseExpectsBesideTheBreakdown.Value;
      set => _fatalErrorHandler.HandleExceptionsFrom(() => _config.ShowWhatAParsingCaseExpectsBesideTheBreakdown.Value = value);
   }

   /// <summary>Makes what the sentence parses to now what its parsing case expects. The verdict stays.</summary>
   [RelayCommand]
   void AcceptWhatItParsesTo() => SentenceCommands.AcceptWhatItParsesToAsAParsingCase.Run(Sentence);
}
