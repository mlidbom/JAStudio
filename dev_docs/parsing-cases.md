# Parsing cases: specifications of how sentences parse, against the real corpus, expecting the exact result

**Status: built, and the old parsing specifications converted (2026-11-09).** Items 3 and 4 of `speeding-up-development.md`, which has the plan and its
open points. The code is in `src/src_dotnet/JAStudio.Core.Specifications/LanguageServices/ParsingCases/`.

## A case

A **parsing case** is one JSON file in `ParsingCases/Cases/`, named for what it's about: the name is the case's name in
the test explorer. It holds:
- `sentence`: as a sentence note's question has it, with `<wbr>` for a word break.
- `why`: why the case exists, such as the misparse it caught.
- `configuration`, if the sentence has one: its highlighted words and incorrect and hidden matches, as the corpus stores a
  sentence's.
- `parsedWords`: the exact parsing result expected, as the corpus stores a sentence's: every valid match, one per line,
  with its form, where it starts, whether it's matched by its surface or its base form, whether it's displayed, and its
  vocab's id. The vocab's question is beside the id, to read; it isn't compared.
- `breakdowns`: the exact sentence breakdown expected, the words JAStudio shows for the sentence, under every combination
  of the options that change it (below). Each breakdown names the options it's under, then lists its displayed words, one
  per line, in order: each with its form, where it starts, surface or base, the form shown beside it if any (such as う
  for る, or `[MISSING]`), whether it's displayed in an emergency, and its vocab.

A case is parsed as a sentence note of the corpus is: its stored parsing result as `SentenceNote.CreateAnalysis` makes
it, and its breakdowns as the sentence breakdown shows them (`SentenceNote.CreateBreakdownAnalysis`,
`TextAnalysisViewModel.DisplayedMatches`).

## The breakdown and its options

Five options change how a sentence parses for its breakdown, and only there (`BreakdownParsingOptions`): hiding all
compounds, hiding compositionally transparent compounds, and automatically yielding the last part of a suru verb,
passive verb or causative verb compound to an overlapping compound. The stored parsing result is the same under any of
them.

A case covers every one of their 32 combinations, but stores only what differs: the options that change its breakdown,
and the breakdown under each combination of those. An option it doesn't name changes nothing for its sentence. Most
sentences have one breakdown, under any options; 食べるな has two, since hiding all compounds hides its うな. Which options
change a breakdown is found by making it under all 32 combinations, which costs next to nothing. A case also fails when
an option starts or stops changing its breakdown.

Not covered: the breakdown in edit mode, which shows every match, valid or not, to edit a sentence's configuration with.
That's the whole analysis, which a case doesn't hold, since it changes with changes inside the parser that change
nothing anyone sees.

## The two modes

- **The everyday mode, `Each_parsing_case`,** runs with every test run, CI's included. It parses each case against the
  **stripped-down corpus** (`ParsingCases/StrippedDownCorpus/`): the vocab of the corpus checkout that parsing the cases
  looked up, as of the last run of the full mode, committed with the cases. A case fails on any difference from its
  expected result, and lists the words that differ: parsed but not expected, expected but not parsed, and displayed or not
  otherwise than expected, each with its vocab.
- **The full mode, `In_the_full_mode`,** is run by hand, and often: it's explicit, so it runs only when run by name. In
  Rider, run it from the test explorer; on the command line, run the specifications' executable,
  `JAStudio.Core.Specifications.exe -explicit only -class "JAStudio.Core.Specifications.LanguageServices.ParsingCases.In_the_full_mode+<the nested class>"`.
  It loads every vocab of the corpus checkout, `src/jas_database`, reading its files and changing nothing there. It parses
  every case, recording every vocab the parsing looks up, replaces the stripped-down corpus with those vocab, and checks
  that every case parses against it exactly as against the corpus checkout. Then:
  - `refreshing_the_stripped_down_corpus_from_the_corpus_checkout` checks that every case parses as it expects. A case
    that fails shows a change to the corpus, or to the code, that changed how its sentence parses.
  - `accepting_what_each_case_parses_to_against_the_corpus_checkout` writes what each case parses to into its file, as
    the result it expects. Review what changed in git's diff before committing it: a recorded result says what the parser
    does, not what's right.

  A run takes about 6 seconds, 4.6 of them loading the corpus's 28,003 vocab.

## Making a case

Write its file with `sentence` and `why`, and `configuration` if the sentence needs one, run accepting, and review the
result it recorded. The vocab its parsing looks up join the stripped-down corpus in the same run.

## Why the stripped-down corpus parses each case as the corpus does

Parsing reaches the corpus only through the vocab collection's lookups, by form, question and so on, all answered by
`VocabCache`. While a `LookupRecording` runs (`VocabCollection.StartRecordingTheVocabLookedUp`), every vocab a lookup
returns is recorded: "every vocab the parsing machinery loads". A corpus holding all of those answers every lookup the
parsing of the cases makes with the same vocab as the whole corpus, so each case parses the same against both. The full
mode checks it anyway, which would catch a way of reaching vocab that bypasses the lookups. The recording runs through
the whole of a case's parsing, its breakdowns included, so the stripped-down corpus also holds the vocab the breakdown
looks up, such as those of a compound's parts.

## Where it stands

- **169 cases,** every case of `SentenceAnalysisViewModelWithSelectDataTests` and `TextAnalysisWithSelectDataTests`,
  converted (2026-11-09) by a script from their `InlineData`, each with its old expectation in its `why`. The old classes
  are gone, but for one case, `AllWordsEqual`, which checks every match of the analysis, valid or not.
- **Each records what the corpus parses its sentence to now,** as a regression test, even where its old case expected
  otherwise (Magnus, 2026-11-09). 102 comparisons with old expectations differ: `parsing-cases-to-inspect.md` lists them,
  for Claude and Magnus to inspect.
- **The everyday mode runs the 169 in 4.6 seconds,** each under all 32 combinations of the breakdown's options, and the
  full mode in 11. Their stripped-down corpus is 811 vocab, 3.9 MB.
- **Still against vocab copied by hand:** `TextAnalysisWithPerTestDataTests`, and the specifications of single matching
  mechanisms, which create the few vocab they need and are meant to.
