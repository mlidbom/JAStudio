using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// One run of the parsing cases' full mode (<see cref="In_the_full_mode"/>): every case parsed against the corpus checkout<br/>
/// (<see cref="SourceCheckout.CorpusCheckout"/>), recording every vocab the parsing looks up (<see cref="LookupRecording{TNote}"/>); the stripped-down<br/>
/// corpus (<see cref="ParsingCasesFolder.StrippedDownCorpus"/>) replaced with those vocab; and every case parsed again against it, to check that it<br/>
/// parses each case exactly as the corpus checkout does.
/// </summary>
sealed class FullModeRun
{
   readonly IReadOnlyList<ParsingCase> _cases = ParsingCasesFolder.AllCases();
   readonly Dictionary<ParsingCase, ParsingCaseResult> _resultAgainstTheCorpusCheckout;

   public FullModeRun()
   {
      using(var corpusCheckout = new JAStudioWithTheVocabOf(SourceCheckout.CorpusCheckout))
      {
         var (results, vocabLookedUp) = ParseEveryCaseRecordingTheVocabLookedUp(corpusCheckout);
         ParsingCasesFolder.StrippedDownCorpus.ReplaceTheVocabWith(vocabLookedUp, corpusCheckout.Serializer, SourceCheckout.CorpusCheckout);
         _resultAgainstTheCorpusCheckout = results;
         HowTheCasesDifferFromTheExpected = HowEachCaseDiffersFromTheExpected(corpusCheckout);
      }

      using var strippedDownCorpus = new JAStudioWithTheVocabOf(ParsingCasesFolder.StrippedDownCorpus);
      CasesParsingOtherwiseAgainstTheStrippedDownCorpus = CasesParsingOtherwiseThanAgainstTheCorpusCheckoutIn(strippedDownCorpus);
   }

   /// <summary>How each case's result against the corpus checkout differs from the expected one: a line per difference, after the case's name.</summary>
   public IReadOnlyList<string> HowTheCasesDifferFromTheExpected { get; }

   /// <summary>
   /// The cases that parse otherwise against the refreshed stripped-down corpus than against the corpus checkout. None, unless parsing reaches vocab<br/>
   /// some way other than the vocab collection's lookups, which the recording doesn't see.
   /// </summary>
   public IReadOnlyList<string> CasesParsingOtherwiseAgainstTheStrippedDownCorpus { get; }

   /// <summary>Makes what each case parses to against the corpus checkout the result it expects, in its file.</summary>
   public void AcceptWhatEachCaseParsesTo()
   {
      foreach(var theCase in _cases) theCase.Accept(_resultAgainstTheCorpusCheckout[theCase]);
   }

   (Dictionary<ParsingCase, ParsingCaseResult> Results, IReadOnlyCollection<VocabNote> VocabLookedUp) ParseEveryCaseRecordingTheVocabLookedUp(JAStudioWithTheVocabOf corpusCheckout)
   {
      var recording = corpusCheckout.Collection.Vocab.StartRecordingTheVocabLookedUp();
      Dictionary<ParsingCase, ParsingCaseResult> results;
      using(recording) results = _cases.ToDictionary(it => it, corpusCheckout.Parse);

      return (results, recording.NotesLookedUp);
   }

   IReadOnlyList<string> HowEachCaseDiffersFromTheExpected(JAStudioWithTheVocabOf corpusCheckout) =>
      [.. _cases.SelectMany(theCase => theCase.HowTheResultDiffersFromTheExpected(_resultAgainstTheCorpusCheckout[theCase], corpusCheckout.Collection)
                                              .Select(difference => $"{theCase.Name}: {difference}"))];

   IReadOnlyList<string> CasesParsingOtherwiseThanAgainstTheCorpusCheckoutIn(JAStudioWithTheVocabOf jastudio) =>
      [.. _cases.Where(theCase => !jastudio.Parse(theCase).IsTheSameAs(_resultAgainstTheCorpusCheckout[theCase])).Select(it => it.Name)];
}
