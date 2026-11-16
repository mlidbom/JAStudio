using System;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Specifications.Fixtures;
using JAStudio.Core.Storage;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// A JAStudio of the specifications whose collection holds the vocab of a <see cref="CorpusFolder"/>, and no other notes: what the parsing<br/>
/// cases (<see cref="ParsingCaseFile"/>) are parsed against. Each case is parsed as a sentence note of the corpus is, from its question and<br/>
/// configuration (<see cref="SentenceNote.CreateAnalysis"/>).
/// </summary>
public class JAStudioWithTheVocabOf : IDisposable
{
   readonly CollectionFactory.AppScope _app = CollectionFactory.InjectCollectionWithSelectData(DataNeeded.None);

   public JAStudioWithTheVocabOf(CorpusFolder corpus)
   {
      corpus.AddTheVocabTo(_app.CoreApp);
   }

   public JPCollection Collection => _app.CoreApp.Services.Resolve<JPCollection>();

   public NoteSerializer Serializer => _app.CoreApp.Services.Resolve<NoteSerializer>();

   /// <summary>
   /// What <paramref name="parsingCase"/>'s sentence parses to: its parsing result as the corpus stores it, and its sentence breakdown under each<br/>
   /// combination of the options that change it. Which options change it is found by making the breakdown under every combination of all of them.
   /// </summary>
   public ParsingCaseResult Parse(ParsingCaseFile parsingCase) => SentenceNoteOf(parsingCase).CreateParsingCaseResult();

   /// <summary>The analysis <paramref name="parsingCase"/>'s sentence gets as a sentence note of the corpus, with the case's configuration: what its parsing result is made from.</summary>
   public TextAnalysis AnalysisOf(ParsingCaseFile parsingCase) => SentenceNoteOf(parsingCase).CreateAnalysis();

   /// <summary>A sentence note of <paramref name="parsingCase"/>'s sentence and configuration, as the corpus would have it.</summary>
   SentenceNote SentenceNoteOf(ParsingCaseFile parsingCase) =>
      new(_app.CoreApp.Services.NoteServices,
          new SentenceDto
          {
             Id = parsingCase.SentenceId,
             SourceData = new SentenceSourceDataDto { Question = parsingCase.Sentence },
             Configuration = parsingCase.Configuration
          });

   public void Dispose()
   {
      _app.Dispose();
      GC.SuppressFinalize(this);
   }
}
