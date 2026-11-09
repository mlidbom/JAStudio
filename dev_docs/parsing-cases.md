# Parsing cases: specifications of how sentences parse, against the real corpus, expecting the exact result

**Status: first increment built (2026-11-09).** Items 3 and 4 of `speeding-up-development.md`, which has the plan and its
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

It's parsed as a sentence note of the corpus is (`SentenceNote.CreateAnalysis`), so the stored parsing result, not the
sentence breakdown: see the open point below.

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
mode checks it anyway, which would catch a way of reaching vocab that bypasses the lookups.

The parsing result doesn't depend on the options. The five parsing reads apply only to the sentence breakdown
(`TextAnalysis.ForUI`): hiding all compounds, hiding compositionally transparent compounds, and automatically yielding
the last part of a suru verb, passive verb or causative verb compound to an overlapping compound.

## Where it stands

- **7 cases,** converted from `SentenceAnalysisViewModelWithSelectDataTests` and `TextAnalysisWithSelectDataTests`, each
  with the old expectation in its `why`. Their stripped-down corpus is 67 vocab, 276 KB.
- **4 parse as their old cases expected** (食べれる, 何て言うか<wbr>さ, 私が頼んだの, 書きなさい). **3 don't, because the real
  corpus has vocab the copies made by hand lack:** いらっしゃいません, with いらっしゃいませ an incorrect match, displays the
  vocab いらっしゃいません rather than いらっしゃいます and ん; 食べるな displays 食べる and うな rather than 食べる, る as う,
  and な; 座っているのはいただけない displays のは and いただけない rather than の, は, いただける and ない.
- **The old cases stay, for now,** since most check something else: the sentence breakdown's displayed words
  (`SentenceViewModel.DisplayedMatches`), not the stored parsing result. The breakdown also shows words displayed in an
  emergency, which a stored result doesn't hold, and applies the compound-hiding settings and yielding to an overlapping
  compound. Whether a case holds the breakdown too is open (`speeding-up-development.md`).
