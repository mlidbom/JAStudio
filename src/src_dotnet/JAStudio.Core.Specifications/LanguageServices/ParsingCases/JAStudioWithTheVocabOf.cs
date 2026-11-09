using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Specifications.Fixtures;
using JAStudio.Core.Storage;
using JAStudio.Core.Storage.Dto;

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

   /// <summary>The parsed words <paramref name="parsingCase"/>'s sentence parses to: its valid matches, as the corpus stores a sentence's parsing result.</summary>
   public IReadOnlyList<ParsedMatch> Parse(ParsingCase parsingCase)
   {
      var sentence = new SentenceNote(_app.CoreApp.Services.NoteServices,
                                      new SentenceDto
                                      {
                                         Id = Guid.NewGuid(),
                                         SourceData = new SentenceSourceDataDto { Question = parsingCase.Sentence },
                                         Configuration = parsingCase.Configuration
                                      });
      return [.. sentence.CreateAnalysis().ValidMatches.Select(ParsedMatch.FromMatch)];
   }

   /// <summary><paramref name="parsedWords"/> written out to read, each with the question of its vocab in this collection.</summary>
   public IReadOnlyList<ParsedWordReported> Reported(IReadOnlyList<ParsedMatch> parsedWords) => [.. parsedWords.Select(it => ParsedWordReported.Of(it, Collection))];

   public void Dispose()
   {
      _app.Dispose();
      GC.SuppressFinalize(this);
   }
}
