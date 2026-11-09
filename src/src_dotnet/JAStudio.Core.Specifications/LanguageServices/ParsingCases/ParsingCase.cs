using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// A sentence with the exact parsing result it's expected to have: every valid match of its parsing, as a sentence of the corpus stores it,<br/>
/// with whether it's displayed and the vocab it's matched to. Any change to the result fails the case (<see cref="Each_parsing_case"/>).<br/>
/// One JSON file of <see cref="ParsingCasesFolder"/>, named for what the case is about, which is its name in the test explorer.
/// </summary>
/// <remarks>
/// A case's expected result is recorded by accepting the result it parses to (<see cref="In_the_full_mode"/>), which says what the parser<br/>
/// does, not what's right: it's reviewed in git's diff before it's committed. <see cref="Why"/> is part of that review.
/// </remarks>
public sealed partial class ParsingCase
{
   readonly FileInfo _file;
   readonly CaseFileContent _content;

   ParsingCase(FileInfo file, CaseFileContent content)
   {
      _file = file;
      _content = content;
   }

   public static ParsingCase Read(FileInfo file) => new(file, CaseFileContent.ReadFrom(file));

   /// <summary>What the case is about: the name of its file, without the extension.</summary>
   public string Name => Path.GetFileNameWithoutExtension(_file.Name);

   /// <summary>The sentence, as a sentence note's question has it, such as with <c>&lt;wbr&gt;</c> marking a word break.</summary>
   public string Sentence => _content.Sentence;

   /// <summary>Why the case exists, such as the misparse it caught.</summary>
   public string Why => _content.Why;

   /// <summary>The sentence's configuration, its highlighted words and its incorrect and hidden matches, as the corpus stores a sentence's.</summary>
   public SentenceConfigDto Configuration => _content.Configuration ?? new SentenceConfigDto();

   /// <summary>The parsed words the sentence is expected to parse to.</summary>
   public IReadOnlyList<ParsedMatch> ExpectedParsedWords => [.. _content.ParsedWords.Select(it => it.ToParsedMatch())];

   /// <summary>How <paramref name="parsedWords"/> differ from the <see cref="ExpectedParsedWords"/>, a line per word that differs. Empty when they don't.</summary>
   public IReadOnlyList<string> HowTheParsedWordsDifferFromTheExpected(IReadOnlyList<ParsedMatch> parsedWords, JPCollection collection)
   {
      var change = ParsedWordsChange.Between(ExpectedParsedWords, parsedWords, collection);
      return [.. change.Added.Select(it => $"parsed, but not expected: {it}"),
              .. change.Removed.Select(it => $"expected, but not parsed: {it}"),
              .. change.NowDisplayed.Select(it => $"displayed, but expected not displayed: {it}"),
              .. change.NoLongerDisplayed.Select(it => $"not displayed, but expected displayed: {it}")];
   }

   /// <summary>Makes <paramref name="parsedWords"/> the parsed words the sentence is expected to parse to, in the case's file.</summary>
   public void Accept(IReadOnlyList<ParsedWordReported> parsedWords) => (_content with { ParsedWords = [.. parsedWords] }).WriteTo(_file);

   public override string ToString() => Name;
}
