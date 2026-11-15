using System;
using System.Collections.Generic;
using System.IO;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Storage.Dto;
using System.Linq;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// A parsing case as the specifications run it, from its file: a sentence with the exact result it's expected to parse to<br/>
/// (<see cref="ParsingCaseResult"/>): its parsing result as the corpus stores it, and<br/>
/// its sentence breakdown under every combination of the options that change it. Any change to either fails the case (<see cref="Each_parsing_case"/>).<br/>
/// One JSON file of <see cref="ParsingCasesFolder"/>, named for what the case is about and its sentence, which is its name in the test explorer.
/// </summary>
/// <remarks>
/// A case file is generated from a sentence of the corpus that is a parsing case (<see cref="SentenceNote.ParsingCase"/>), by the full mode<br/>
/// (<see cref="In_the_full_mode"/>): the corpus is the master, where a case is made, judged and accepted, and the file is how the specifications<br/>
/// run it without the corpus.
/// </remarks>
public sealed partial class ParsingCaseFile
{
   readonly FileInfo _file;
   readonly Content _content;

   ParsingCaseFile(FileInfo file, Content content)
   {
      _file = file;
      _content = content;
   }

   public static ParsingCaseFile Read(FileInfo file) => new(file, Content.ReadFrom(file));

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

   /// <summary>
   /// Writes the file of the case that <paramref name="sentence"/> is, a parsing case of the corpus, into <paramref name="casesFolder"/>, and returns the<br/>
   /// case: the sentence's question and configuration, why it's a parsing case, and what it's expected to parse to, naming each vocab by its question in<br/>
   /// <paramref name="collection"/>.
   /// </summary>
   public static ParsingCaseFile WriteTheFileOf(SentenceNote sentence, DirectoryInfo casesFolder, JPCollection collection)
   {
      var parsingCaseOfTheCorpus = TheParsingCaseOf(sentence);
      var expected = parsingCaseOfTheCorpus.Expected.NamingTheVocabIn(collection);
      var configuration = ((SentenceDto)sentence.ToDto()).Configuration;
      var content = new Content
                    {
                       Sentence = sentence.Question.WithInvisibleSpace().Replace(StringExtensions.InvisibleSpace.ToString(), SentenceQuestionField.WordBreakTag),
                       Why = parsingCaseOfTheCorpus.Why,
                       Configuration = configuration.HighlightedWords.Count + configuration.IncorrectMatches.Count + configuration.HiddenMatches.Count > 0 ? configuration : null,
                       ParsedWords = [.. expected.ParsedWords],
                       Breakdowns = [.. expected.Breakdowns]
                    };
      var file = new FileInfo(Path.Combine(casesFolder.FullName, $"{NameOfTheCaseOf(sentence)}.json"));
      content.WriteTo(file);
      return new ParsingCaseFile(file, content);
   }

   /// <summary>
   /// The name of the case that <paramref name="sentence"/> is, a parsing case of the corpus: what its parsing case is about, then the sentence, as in<br/>
   /// <c>ichidan imperative - 捕まえよ</c>. What follows a <c> - </c> in the parsing case's name comes after the sentence, as in<br/>
   /// <c>hidden exclusions - いらっしゃいません - hidden matches</c>. The sentence is without what a file's name can't have.
   /// </summary>
   public static string NameOfTheCaseOf(SentenceNote sentence)
   {
      var whatTheCaseIsAbout = TheParsingCaseOf(sentence).Name.Split(" - ", 2);
      var theSentenceAsAFileNameCanHaveIt = new string([.. sentence.GetQuestion().Where(it => !CharactersAFileNameCannotHave.Contains(it) && !char.IsControl(it))]).Trim();
      return string.Join(" - ", new[] { whatTheCaseIsAbout[0], theSentenceAsAFileNameCanHaveIt }.Concat(whatTheCaseIsAbout.Skip(1)).Where(it => it.Length > 0));
   }

   const string CharactersAFileNameCannotHave = "<>:\"/\\|?*";

   static ParsingCase TheParsingCaseOf(SentenceNote sentence) =>
      sentence.ParsingCase ?? throw new InvalidOperationException($"The sentence {sentence.GetQuestion()} isn't a parsing case.");

   public override string ToString() => Name;
}
