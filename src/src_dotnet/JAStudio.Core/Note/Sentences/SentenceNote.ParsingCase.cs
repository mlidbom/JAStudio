using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Core.Note.Sentences.ParsingCases.Breakdowns;
using JAStudio.Core.UI.Web.Sentence;

namespace JAStudio.Core.Note.Sentences;

public partial class SentenceNote
{
   /// <summary>
   /// What makes the sentence a parsing case: the result it's expected to parse to, and a person's verdict on it. Null when the sentence isn't a<br/>
   /// parsing case, as most aren't.
   /// </summary>
   public ParsingCase? ParsingCase { get; private set; }

   /// <summary>
   /// What the sentence parses to now, as a parsing case records it: its parsing result as the corpus stores it, and its breakdown under each<br/>
   /// combination of the options that change it.
   /// </summary>
   public ParsingCaseResult CreateParsingCaseResult()
   {
      return new ParsingCaseResult([.. CreateAnalysis().ValidMatches.Select(it => ParsedWordReported.Of(ParsedMatch.FromMatch(it), Services.Collection))],
                                   BreakdownUnderOptions.UnderEachCombinationOfTheOptionsChangingIt(BreakdownUnder));

      IReadOnlyList<BreakdownWord> BreakdownUnder(BreakdownParsingOptions options) =>
         [.. new TextAnalysisViewModel(CreateBreakdownAnalysis(options), Services.Settings, Services.Collection.Vocab).DisplayedMatches.Select(BreakdownWord.Of)];
   }

   /// <summary>Makes the sentence a parsing case that expects what it parses to now, which nobody has named or judged yet.</summary>
   public void MarkAsAParsingCase()
   {
      Contract.State.Assert(ParsingCase == null, () => $"The sentence {GetQuestion()} is a parsing case already.");
      Guard.Update(() => ParsingCase = ParsingCase.Expecting(CreateParsingCaseResult()));
   }

   /// <summary>Makes the sentence an ordinary sentence again: what it was expected to parse to, and the verdict on it, are gone.</summary>
   public void StopBeingAParsingCase()
   {
      AssertIsAParsingCase();
      Guard.Update(() => ParsingCase = null);
   }

   /// <summary>Makes what the sentence parses to now what its parsing case expects. The verdict stays: accepting claims nothing about the result's being right.</summary>
   public void AcceptWhatItParsesToAsAParsingCase() => ChangeTheParsingCase(it => it.Accepting(CreateParsingCaseResult()));

   /// <summary>Says what a person has found about the vocab the parsing case expects the breakdown to display.</summary>
   public void SetTheVerdictOnTheDisplayedVocab(Verdict verdict) => ChangeTheParsingCase(it => it with { VerdictOnTheDisplayedVocab = verdict });

   /// <summary>Names the parsing case for what it's about.</summary>
   public void NameTheParsingCase(string name) => ChangeTheParsingCase(it => it with { Name = name });

   /// <summary>Says why the parsing case exists, or what is wrong with how it parses.</summary>
   public void SayWhyItIsAParsingCase(string why) => ChangeTheParsingCase(it => it with { Why = why });

   /// <summary>How what the sentence parses to now compares with what its parsing case expects.</summary>
   public ParsingCaseComparison CompareWithWhatTheParsingCaseExpects()
   {
      AssertIsAParsingCase();
      return ParsingCase!.ComparedWith(CreateParsingCaseResult(), Services.Collection);
   }

   void ChangeTheParsingCase(System.Func<ParsingCase, ParsingCase> change)
   {
      AssertIsAParsingCase();
      Guard.Update(() => ParsingCase = change(ParsingCase!));
   }

   void AssertIsAParsingCase() => Contract.State.Assert(ParsingCase != null, () => $"The sentence {GetQuestion()} isn't a parsing case.");
}
