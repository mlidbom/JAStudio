using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
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
   readonly Dictionary<ParsingCase, IReadOnlyList<ParsedWordReported>> _parsedWordsAgainstTheCorpusCheckout;

   public FullModeRun()
   {
      using(var corpusCheckout = new JAStudioWithTheVocabOf(SourceCheckout.CorpusCheckout))
      {
         var (parsedWords, vocabLookedUp) = ParseEveryCaseRecordingTheVocabLookedUp(corpusCheckout);
         ParsingCasesFolder.StrippedDownCorpus.ReplaceTheVocabWith(vocabLookedUp, corpusCheckout.Serializer);
         _parsedWordsAgainstTheCorpusCheckout = ReportedIn(corpusCheckout, parsedWords);
         HowTheCasesDifferFromTheExpected = HowEachCaseDiffersFromTheExpected(parsedWords, corpusCheckout);
      }

      using var strippedDownCorpus = new JAStudioWithTheVocabOf(ParsingCasesFolder.StrippedDownCorpus);
      CasesParsingOtherwiseAgainstTheStrippedDownCorpus = CasesParsingOtherwiseThanAgainstTheCorpusCheckoutIn(strippedDownCorpus);
   }

   /// <summary>How each case's parsed words against the corpus checkout differ from the expected ones: a line per word that differs, after the case's name.</summary>
   public IReadOnlyList<string> HowTheCasesDifferFromTheExpected { get; }

   /// <summary>
   /// The cases that parse otherwise against the refreshed stripped-down corpus than against the corpus checkout. None, unless parsing reaches vocab<br/>
   /// some way other than the vocab collection's lookups, which the recording doesn't see.
   /// </summary>
   public IReadOnlyList<string> CasesParsingOtherwiseAgainstTheStrippedDownCorpus { get; }

   /// <summary>Makes what each case parses to against the corpus checkout the parsed words it expects, in its file.</summary>
   public void AcceptWhatEachCaseParsesTo()
   {
      foreach(var theCase in _cases) theCase.Accept(_parsedWordsAgainstTheCorpusCheckout[theCase]);
   }

   (Dictionary<ParsingCase, IReadOnlyList<ParsedMatch>> ParsedWords, IReadOnlyCollection<VocabNote> VocabLookedUp) ParseEveryCaseRecordingTheVocabLookedUp(JAStudioWithTheVocabOf corpusCheckout)
   {
      var recording = corpusCheckout.Collection.Vocab.StartRecordingTheVocabLookedUp();
      Dictionary<ParsingCase, IReadOnlyList<ParsedMatch>> parsedWords;
      using(recording) parsedWords = _cases.ToDictionary(it => it, corpusCheckout.Parse);

      return (parsedWords, recording.NotesLookedUp);
   }

   static Dictionary<ParsingCase, IReadOnlyList<ParsedWordReported>> ReportedIn(JAStudioWithTheVocabOf jastudio, Dictionary<ParsingCase, IReadOnlyList<ParsedMatch>> parsedWords) =>
      parsedWords.ToDictionary(it => it.Key, it => jastudio.Reported(it.Value));

   IReadOnlyList<string> HowEachCaseDiffersFromTheExpected(Dictionary<ParsingCase, IReadOnlyList<ParsedMatch>> parsedWords, JAStudioWithTheVocabOf jastudio) =>
      [.. _cases.SelectMany(theCase => theCase.HowTheParsedWordsDifferFromTheExpected(parsedWords[theCase], jastudio.Collection)
                                              .Select(difference => $"{theCase.Name}: {difference}"))];

   IReadOnlyList<string> CasesParsingOtherwiseThanAgainstTheCorpusCheckoutIn(JAStudioWithTheVocabOf jastudio) =>
      [.. _cases.Where(theCase => !jastudio.Reported(jastudio.Parse(theCase)).SequenceEqual(_parsedWordsAgainstTheCorpusCheckout[theCase]))
                .Select(it => it.Name)];
}
