using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// One run of the parsing cases' full mode (<see cref="In_the_full_mode"/>): the sentences of the corpus checkout that are parsing cases<br/>
/// (<see cref="SentenceNote.ParsingCase"/>) parsed against the corpus checkout (<see cref="SourceCheckout.CorpusCheckout"/>), recording every vocab<br/>
/// the parsing looks up (<see cref="LookupRecording{TNote}"/>); the case files replaced with one generated from each of those sentences; the<br/>
/// parsing cases' corpus (<see cref=\"ParsingCasesFolder.Corpus\"/>) replaced with those vocab; and every case parsed again against it, to<br/>
/// check that it parses each case exactly as the corpus checkout does.
/// </summary>
sealed class FullModeRun
{
   public FullModeRun()
   {
      Dictionary<ParsingCaseFile, ParsingCaseResult> resultAgainstTheCorpusCheckout;
      using(var corpusCheckout = new JAStudioWithTheVocabOf(SourceCheckout.CorpusCheckout))
      {
         var parsingCasesOfTheCorpus = SourceCheckout.CorpusCheckout.ReadTheSentencesThatAreParsingCases(corpusCheckout.Serializer);
         var (results, vocabLookedUp) = ParseEachRecordingTheVocabLookedUp(parsingCasesOfTheCorpus, corpusCheckout);
         HowTheParsingCasesOfTheCorpusCheckoutDifferFromWhatTheyExpect =
         [
            .. parsingCasesOfTheCorpus.SelectMany(sentence => sentence.ParsingCase!.ComparedWith(results[sentence], corpusCheckout.Collection).Differences
                                                                      .Select(difference => $"{ParsingCaseFile.NameOfTheCaseOf(sentence)}: {difference}"))
         ];

         var caseOf = ParsingCasesFolder.ReplaceTheCasesWithThoseOf(parsingCasesOfTheCorpus, corpusCheckout.Collection);
         ParsingCasesFolder.Corpus.ReplaceTheVocabWith(vocabLookedUp, SourceCheckout.CorpusCheckout);
         resultAgainstTheCorpusCheckout = parsingCasesOfTheCorpus.ToDictionary(sentence => caseOf[sentence], sentence => results[sentence]);
      }

      using var parsingCasesCorpus = new JAStudioWithTheVocabOf(ParsingCasesFolder.Corpus);
      CasesParsingOtherwiseAgainstTheParsingCasesCorpus =
         [.. resultAgainstTheCorpusCheckout.Where(it => !parsingCasesCorpus.Parse(it.Key).IsTheSameAs(it.Value)).Select(it => it.Key.Name).Order()];
   }

   /// <summary>
   /// How what each parsing case of the corpus checkout parses to there differs from what it expects: a line per difference, after the case's name.<br/>
   /// None, while every parsing case parses as expected in the corpus.
   /// </summary>
   public IReadOnlyList<string> HowTheParsingCasesOfTheCorpusCheckoutDifferFromWhatTheyExpect { get; }

   /// <summary>
   /// The cases that parse otherwise against the refreshed parsing cases' corpus than their sentences do against the corpus checkout. None, unless<br/>
   /// parsing reaches vocab some way other than the vocab collection's lookups, which the recording doesn't see.
   /// </summary>
   public IReadOnlyList<string> CasesParsingOtherwiseAgainstTheParsingCasesCorpus { get; }

   static (Dictionary<SentenceNote, ParsingCaseResult> Results, IReadOnlyCollection<VocabNote> VocabLookedUp) ParseEachRecordingTheVocabLookedUp(
      IReadOnlyList<SentenceNote> sentences,
      JAStudioWithTheVocabOf corpusCheckout)
   {
      var recording = corpusCheckout.Collection.Vocab.StartRecordingTheVocabLookedUp();
      Dictionary<SentenceNote, ParsingCaseResult> results;
      using(recording) results = sentences.ToDictionary(it => it, it => it.CreateParsingCaseResult());
      return (results, recording.NotesLookedUp);
   }
}
