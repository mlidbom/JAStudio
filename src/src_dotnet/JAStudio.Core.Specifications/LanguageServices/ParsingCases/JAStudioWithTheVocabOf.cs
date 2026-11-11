using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Configuration;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Specifications.Fixtures;
using JAStudio.Core.Storage;
using JAStudio.Core.Specifications.LanguageServices.ParsingCases.Breakdowns;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.UI.Web.Sentence;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// A JAStudio of the specifications whose collection holds the vocab of a <see cref="CorpusFolder"/>, and no other notes: what the parsing<br/>
/// cases (<see cref="ParsingCase"/>) are parsed against. Each case is parsed as a sentence note of the corpus is, from its question and<br/>
/// configuration (<see cref="SentenceNote.CreateAnalysis"/>).
/// </summary>
public class JAStudioWithTheVocabOf : IDisposable
{
   readonly CollectionFactory.AppScope _app = CollectionFactory.InjectCollectionWithSelectData(DataNeeded.None);

   public JAStudioWithTheVocabOf(CorpusFolder corpus)
   {
      foreach(var vocab in corpus.ReadTheVocab(Serializer)) Collection.Vocab.Add(vocab);
   }

   public JPCollection Collection => _app.CoreApp.Services.Resolve<JPCollection>();

   public NoteSerializer Serializer => _app.CoreApp.Services.Resolve<NoteSerializer>();

   /// <summary>
   /// What <paramref name="parsingCase"/>'s sentence parses to: its parsing result as the corpus stores it, and its sentence breakdown under each<br/>
   /// combination of the options that change it. Which options change it is found by making the breakdown under every combination of all of them.
   /// </summary>
   public ParsingCaseResult Parse(ParsingCase parsingCase)
   {
      var sentence = SentenceNoteOf(parsingCase);
      return new ParsingCaseResult([.. sentence.CreateAnalysis().ValidMatches.Select(it => ParsedWordReported.Of(ParsedMatch.FromMatch(it), Collection))],
                                   BreakdownsUnderEachCombinationOfTheOptionsChangingThem(sentence));
   }

   /// <summary>The analysis <paramref name="parsingCase"/>'s sentence gets as a sentence note of the corpus, with the case's configuration: what its parsing result is made from.</summary>
   public TextAnalysis AnalysisOf(ParsingCase parsingCase) => SentenceNoteOf(parsingCase).CreateAnalysis();

   /// <summary>A sentence note of <paramref name="parsingCase"/>'s sentence and configuration, as the corpus would have it.</summary>
   SentenceNote SentenceNoteOf(ParsingCase parsingCase) =>
      new(_app.CoreApp.Services.NoteServices,
          new SentenceDto
          {
             Id = Guid.NewGuid(),
             SourceData = new SentenceSourceDataDto { Question = parsingCase.Sentence },
             Configuration = parsingCase.Configuration
          });

   IReadOnlyList<BreakdownUnderOptions> BreakdownsUnderEachCombinationOfTheOptionsChangingThem(SentenceNote sentence)
   {
      var breakdownUnder = BreakdownParsingOption.EveryCombinationOf(BreakdownParsingOption.All).ToDictionary(it => it, it => BreakdownOf(sentence, it));
      var optionsChangingTheBreakdown = BreakdownParsingOption.All.Where(ChangesTheBreakdown).ToList();

      return [.. BreakdownParsingOption.EveryCombinationOf(optionsChangingTheBreakdown)
                                       .Select(options => new BreakdownUnderOptions(optionsChangingTheBreakdown.ToDictionary(it => it.Name, it => it.IsOnIn(options)), breakdownUnder[options]))];

      bool ChangesTheBreakdown(BreakdownParsingOption option) => breakdownUnder.Keys.Any(options => !breakdownUnder[options].SequenceEqual(breakdownUnder[option.SwitchedIn(options)]));
   }

   IReadOnlyList<BreakdownWord> BreakdownOf(SentenceNote sentence, BreakdownParsingOptions options) =>
      [.. new TextAnalysisViewModel(sentence.CreateBreakdownAnalysis(options), Resolve<Settings>(), Collection.Vocab).DisplayedMatches.Select(BreakdownWord.Of)];

   TService Resolve<TService>() where TService : class => _app.CoreApp.Services.Resolve<TService>();

   public void Dispose()
   {
      _app.Dispose();
      GC.SuppressFinalize(this);
   }
}
