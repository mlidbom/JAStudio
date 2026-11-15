using System.Collections.Generic;
using System.IO;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// A sentence with the exact result it's expected to parse to (<see cref="ParsingCaseResult"/>): its parsing result as the corpus stores it, and<br/>
/// its sentence breakdown under every combination of the options that change it. Any change to either fails the case (<see cref="Each_parsing_case"/>).<br/>
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

   /// <summary>The result the sentence is expected to parse to.</summary>
   public ParsingCaseResult Expected => new(_content.ParsedWords, _content.Breakdowns);

   /// <summary>How <paramref name="actual"/> differs from the <see cref="Expected"/> result, a line per difference. Empty when it doesn't.</summary>
   public IReadOnlyList<string> HowTheResultDiffersFromTheExpected(ParsingCaseResult actual, JPCollection collection) =>
      Expected.HowItDiffersFromThisExpectedResult(actual, collection);

   /// <summary>Makes <paramref name="result"/> the result the sentence is expected to parse to, in the case's file.</summary>
   public void Accept(ParsingCaseResult result) => (_content with { ParsedWords = [.. result.ParsedWords], Breakdowns = [.. result.Breakdowns] }).WriteTo(_file);

   public override string ToString() => Name;
}
