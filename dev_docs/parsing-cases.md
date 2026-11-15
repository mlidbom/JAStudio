# Parsing cases: specifications of how sentences parse, against the real corpus, expecting the exact result

**Status: built, and the old parsing specifications converted (2026-11-09).** Items 3 and 4 of `speeding-up-development.md`, which has the plan and its
open points. The code is in `src/src_dotnet/JAStudio.Core.Specifications/LanguageServices/ParsingCases/`.

**The corpus is the master of the parsing cases (Magnus, 2026-11-15).** A parsing case is a sentence of the corpus marked
as one (`SentenceNote.ParsingCase`), with what it's expected to parse to and a person's verdict on it. The case files
below are generated from those sentences by the full mode, for the specifications to run without the corpus. The plan,
and how far it has come, is in [parsing cases in the corpus](todo/parsing-cases-in-the-corpus.md).

## A case

A parsing case's file (`ParsingCaseFile`) is one JSON file in `ParsingCases/Cases/`, generated from its sentence in the
corpus. It's named for what the parsing case is about and its sentence, such as `ichidan imperative - 捕まえよ`: the name
is the case's name in the test explorer. It holds:
- `sentenceId`: the id of the sentence of the corpus it's generated from. The name and the sentence's text can change;
  the id ties the file to its sentence for good (Magnus, 2026-11-15).
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
  It loads every vocab of the corpus checkout, `src/jas_database`, and its sentences that are parsing cases, reading its
  files and changing nothing there. It parses each of those sentences, recording every vocab the parsing looks up, replaces
  the case files with one generated from each sentence, and the stripped-down corpus with those vocab. Its one class,
  `generating_the_case_files_and_the_stripped_down_corpus_from_the_corpus_checkout`, checks two things:
  - every parsing case of the corpus checkout parses there exactly as it expects;
  - every case parses against the stripped-down corpus exactly as its sentence does against the corpus checkout.

  Review what it wrote in git's diff before committing it. A corpus checkout without parsing cases fails it, before it
  deletes any case file.

  A run takes about 6 seconds, 4.6 of them loading the corpus's 28,003 vocab.

## Making a case

Mark the sentence as a parsing case in JAStudio, name it and say why, with the sentence's commands (`SentenceCommands`,
which the MCP server runs too). What it parses to then is what it expects. A result that changes is accepted there as
well, on the sentence. Then run the full mode: the case's file is generated, and the vocab its parsing looks up join
the stripped-down corpus in the same run.

## Why the stripped-down corpus parses each case as the corpus does

Parsing reaches the corpus only through the vocab collection's lookups, by form, question and so on, all answered by
`VocabCache`. While a `LookupRecording` runs (`VocabCollection.StartRecordingTheVocabLookedUp`), every vocab a lookup
returns is recorded: "every vocab the parsing machinery loads". A corpus holding all of those answers every lookup the
parsing of the cases makes with the same vocab as the whole corpus, so each case parses the same against both. The full
mode checks it anyway, which would catch a way of reaching vocab that bypasses the lookups. The recording runs through
the whole of a case's parsing, its breakdowns included, so the stripped-down corpus also holds the vocab the breakdown
looks up, such as those of a compound's parts.

## Where it stands

- **230 cases: every specification of how a sentence parses or displays is one now (2026-11-09).** Converted from
  `SentenceAnalysisViewModelWithSelectDataTests`, `TextAnalysisWithSelectDataTests` and `TextAnalysisWithPerTestDataTests`
  by a script from their `InlineData` (169 cases), and from the AI-created specifications of だって, of a vocab entered with
  the dictionary form ending placeholder (う以上), and of ichidan verbs unknown to the tokenizer (61 cases). Each has its old
  expectation in its `why`; a case converted from a specification of a rule says the rule in its name, and the rule held
  in every one of them against the real corpus. The old specifications are gone, `AllWordsEqual`'s check of every match of
  the analysis, valid or not, with them (Magnus agreed). What stays are the specifications of how the tokenizer splits such
  text, which involve no vocab.
- **Each records what the corpus parses its sentence to now,** as a regression test, even where its old case expected
  otherwise (Magnus, 2026-11-09). 102 comparisons with old expectations differ: `parsing-cases-to-inspect.md` lists them,
  for Claude and Magnus to inspect.
- **The everyday mode runs the 230 in 3.5 seconds,** each under all 32 combinations of the breakdown's options, and the
  full mode in about 10. Profiled (2026-11-09): about 2 seconds are the breakdowns, and 0.5 to 0.9 loading JMdict's forms,
  once. The cases run one after another, as the rows of one theory: running them in parallel waits for xUnit v3 4.0
  (`speeding-up-development.md`).
- **The vocab copied by hand are gone (2026-11-09).** The specifications that aren't about parsing but need vocab as sample
  data, such as the card server's and the serializer's, get the stripped-down corpus's (`DataNeeded.Vocabulary`). So a
  run of the full mode changes their sample data too: they need only real vocab, and the web specifications 入れる.
