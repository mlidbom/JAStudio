using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences.ParsingCases.Breakdowns;

namespace JAStudio.Core.Note.Sentences.ParsingCases;

/// <summary>
/// What a sentence parses to, as a parsing case records it (<see cref="ParsingCase"/>): its parsing result as the corpus stores it, and its sentence<br/>
/// breakdown under every combination of the options that change it. A parsing case expects one exactly.
/// </summary>
/// <param name="ParsedWords">The stored parsing result: every valid match, displayed or not.</param>
/// <param name="Breakdowns">The breakdown under each combination of the options that change it: one, under any options, when none does.</param>
public sealed record ParsingCaseResult(IReadOnlyList<ParsedWordReported> ParsedWords, IReadOnlyList<BreakdownUnderOptions> Breakdowns)
{
   /// <summary>Whether this is exactly the result <paramref name="other"/> is, the questions of the vocab included.</summary>
   public bool IsTheSameAs(ParsingCaseResult other) =>
      ParsedWords.SequenceEqual(other.ParsedWords) &&
      Breakdowns.Count == other.Breakdowns.Count &&
      Breakdowns.Zip(other.Breakdowns).All(it => it.First.OptionsDescription == it.Second.OptionsDescription && it.First.DisplayedWords.SequenceEqual(it.Second.DisplayedWords));

   /// <summary>The names of the options that change the breakdown, in the order of <see cref="BreakdownParsingOption.All"/>.</summary>
   public IReadOnlyList<string> OptionsChangingTheBreakdown =>
      [.. BreakdownParsingOption.All.Select(it => it.Name).Where(name => Breakdowns.Any(breakdown => breakdown.Options.ContainsKey(name)))];

   /// <summary>The breakdown with every option off: what a person sees of the sentence with nothing hidden and nothing yielded.</summary>
   public BreakdownUnderOptions BreakdownWithEveryOptionOff => Breakdowns.Single(it => it.Options.Values.All(isOn => !isOn));

   /// <summary>
   /// The breakdown under <paramref name="options"/>: the one under the same values of the options that change the breakdown. The other options<br/>
   /// change nothing, whatever their values.
   /// </summary>
   public BreakdownUnderOptions BreakdownUnder(BreakdownParsingOptions options) =>
      Breakdowns.Single(breakdown => BreakdownParsingOption.All.Where(it => breakdown.Options.ContainsKey(it.Name)).All(it => breakdown.Options[it.Name] == it.IsOnIn(options)));

   /// <summary>The vocab the breakdown displays with every option off: the part of this result a person gives a <see cref="Verdict"/> on.</summary>
   public DisplayedVocab DisplayedVocab => DisplayedVocab.Of(BreakdownWithEveryOptionOff);

   /// <summary>
   /// How <paramref name="actual"/> differs from this, the result expected, a line per difference, naming each vocab by its question in<br/>
   /// <paramref name="collection"/>. Empty when it doesn't differ. The questions of the vocab are not compared.
   /// </summary>
   public IReadOnlyList<string> HowItDiffersFromThisExpectedResult(ParsingCaseResult actual, JPCollection collection)
   {
      var expected = NamingTheVocabIn(collection);
      var parsed = actual.NamingTheVocabIn(collection);
      return [.. HowTheParsedWordsDiffer(), .. HowTheBreakdownsDiffer()];

      IEnumerable<string> HowTheParsedWordsDiffer()
      {
         var change = ParsedWordsChange.Between([.. expected.ParsedWords.Select(it => it.ToParsedMatch())], [.. parsed.ParsedWords.Select(it => it.ToParsedMatch())], collection);
         return [.. change.Added.Select(it => $"parsed, but not expected: {it}"),
                 .. change.Removed.Select(it => $"expected, but not parsed: {it}"),
                 .. change.NowDisplayed.Select(it => $"displayed, but expected not displayed: {it}"),
                 .. change.NoLongerDisplayed.Select(it => $"not displayed, but expected displayed: {it}")];
      }

      IEnumerable<string> HowTheBreakdownsDiffer()
      {
         if(!parsed.OptionsChangingTheBreakdown.SequenceEqual(expected.OptionsChangingTheBreakdown))
            return [$"the options that change the breakdown are {NamesOf(parsed.OptionsChangingTheBreakdown)}, not the expected {NamesOf(expected.OptionsChangingTheBreakdown)}"];

         return parsed.Breakdowns.SelectMany(breakdown => expected.Breakdowns.SingleOrDefault(it => it.OptionsDescription == breakdown.OptionsDescription) is {} expectedBreakdown
                                                             ? expectedBreakdown.HowTheDisplayedWordsDiffer(breakdown)
                                                             : [$"the breakdown {breakdown.OptionsDescription} isn't in the case: {string.Join("; ", breakdown.DisplayedWords)}"]);
      }

      static string NamesOf(IReadOnlyList<string> options) => options.Count == 0 ? "none" : string.Join(", ", options);
   }

   /// <summary>This result, naming each vocab by the question it has in <paramref name="collection"/>: a result the corpus stores holds only the vocab's ids.</summary>
   public ParsingCaseResult NamingTheVocabIn(JPCollection collection)
   {
      return new ParsingCaseResult([.. ParsedWords.Select(it => ParsedWordReported.Of(it.ToParsedMatch(), collection))],
                                   [.. Breakdowns.Select(breakdown => breakdown with { DisplayedWords = [.. breakdown.DisplayedWords.Select(it => it with { Vocab = QuestionOf(it.VocabId) })] })]);

      string? QuestionOf(string? vocabId) => vocabId != null ? collection.NoteFromNoteId(new VocabId(Guid.Parse(vocabId)))?.GetQuestion() : null;
   }

   /// <summary>This result without the questions of the vocab, as the corpus stores it.</summary>
   public ParsingCaseResult WithoutTheQuestionsOfTheVocab() =>
      new([.. ParsedWords.Select(it => it with { Vocab = null })],
          [.. Breakdowns.Select(breakdown => breakdown with { DisplayedWords = [.. breakdown.DisplayedWords.Select(it => it with { Vocab = null })] })]);
}
