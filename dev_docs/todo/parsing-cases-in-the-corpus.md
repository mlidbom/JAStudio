# The parsing cases move into the corpus

> **Status: agreed with Magnus 2026-11-15, on the branch `parsing-cases-in-the-corpus`.** The groundwork is done (below).
> The data type is proposed, for Magnus to confirm before it's built. It changes [the parsing cases](../parsing-cases.md),
> whose case files become generated from the corpus.

## What it's for (Magnus, 2026-11-15)

It came up while planning how to judge what switching the tokenizer dictionary to UniDic changes in parsing (on the
branch `unidic`: `dev_docs/todo/judging-what-unidic-changes-in-parsing.md`). Before any tooling for that, Magnus wanted
"some vital basics in place first":

- **The parsing cases are looked at in JAStudio.** Their sentences are sentence notes of the corpus, so the sentence
  page and its Parsing tab show them, in every JAStudio that runs.
- **The real sentence is the case.** "I want the real sentences we work with, chosen ones of them, to be the cases. Not
  hidden archived copies of them with different matching config." A copy apart from the sentence Magnus studies ends up
  "a moldy thing forgotten in the cellar": "almost certainly the real one is correct and the case wrong".
- **A case is marked while studying.** "I want to hit a sentence that I'm about to study, mark it as a WhatIsItCalled?
  and mark it as incorrect, a bug, and then be able to come back to all such and fix them...."
- **The corpus is the master.** "From here on in, the corpus is the master of the test suite. These sentences are what
  is extrected to the test suite."

## Decided (Magnus, 2026-11-15)

- **The word is the project's own: a parsing case** (`ParsingCase`). A sentence is marked as a parsing case. "Good
  ParsingCase it is."
- **A parsing case has a state: verified, broken, or unverified.** Only the verified ones are a gold standard, the
  language-processing term for sentences whose analysis a person has verified.
- **It's a data type on the sentence, not tags.** "It does not have to be tags, but it does need to be navigatable easily
  [...] it absolutely can, and likely should, be another data type, maybe a nullable object graph on the sentence that
  is filled in with all this and the parsing results for different settings when a sentence is marked as a test-case".
- **The stored result is the result as it was verified.** A reparse leaves it alone and compares with it. A case that
  no longer parses that way is findable in JAStudio itself, and the sentence can show what it was and what it is now.
  Accepting a new result is an action.
- **The parsing cases aren't left out of the vocab's sentences.** Claude proposed it, so that the word forms among the
  cases wouldn't show there. Magnus: "I'd skip this. I'm not sure we want it in the end. For one thing that would, once
  more, make our test sentences separate from what is actually used in the corpus."
- **The code comes before the import** (Magnus: "If we are doing both anyway I'd lean towards code first"). The import
  then writes the final shape once, with no tag in between.
- **The import is exact, and done once.** "This is a one time thing that needs manual massaging of data to get it in,
  then the files will be just generated from the corpus. [...] You will do it once, with jas_database fully up to date.
  We will commit the results there and that will be the corpus going forward. No changes are happening on the corpus
  elsewhere at the moment."
  - **A case goes to a sentence note that's already in the corpus only when that note parses identically:** the same
    text and configuration, and the same parsed words. Otherwise a new sentence note is made from the case. "For now we
    want it reproducible and exact. If no exactly identical sentence, in terms of parsing, exists, create a new on from
    the parsing case."
  - **Every case must parse in the corpus exactly as its case file expects.** "All the tests should pass on main, if not
    something is fubar. And getting the sentences correctly into the corpus means the same thing, all should parse
    identically in the corpus."
  - **The 16 cases that share 6 sentences get a sentence note each.** They mark different matches as hidden or as
    incorrect on the same text, such as いらっしゃいません with いらっしゃいませ hidden, and again with it marked incorrect, and
    one note holds one configuration.
- **The work starts in the worktree's own corpus checkout.** Committing to the corpus's own line comes on Magnus's
  word: "You'll get it when it's time."

## What the cases are today (measured 2026-11-15)

- **230 cases, of 220 different sentences.** 15 cases have a sentence configuration, and 6 sentences are shared by 16
  cases.
- **56 of the sentences are already sentence notes of the corpus.** One of those notes has marked matches of its own
  that its case file lacks, so its case gets a new note. 6 of the texts are in several notes.
- **164 aren't in the corpus,** and become new sentence notes, shown like any other sentence. 90 of them are word
  forms of five characters or fewer, such as 泳げず, 泳げた, くる and のに.
- **A sentence note already stores most of what a case file does:** its configuration, in the same shape, and every
  parsed word with whether it's displayed. It lacks the breakdowns under each combination of the options that change
  them, the case's name and its `why`.

## The groundwork, done

- **The branch** `parsing-cases-in-the-corpus`, from `main` at `7b7427b9f`, in the worktree `worktree_1`. Its corpus
  checkout is at `37f3f7fede7`, which is also where Magnus's corpus checkout is, clean and pushed.
- **The baseline holds.** The full mode, on this branch's code against that corpus checkout: every case parses against
  the corpus checkout exactly as its file expects, and against the stripped-down corpus exactly as against the corpus
  checkout.
- **The stripped-down corpus was behind the corpus, and is refreshed:** 58 of its vocab still had the definitions
  written out that the corpus has named as shared-conditions since increment 6 of the matching configuration. The
  harness couldn't read those names: it loads shared-conditions now (brought over from the branch `unidic`).

## The data type (Claude's proposal, for Magnus to confirm)

The specifications already have the model, and its words are kept: a `ParsingCase` with a `Name`, a `Why` and an
`Expected` result (`ParsingCaseResult`: the parsed words, and the breakdown under each combination of the options that
change it), which `Accept` replaces. It moves into Core, onto the sentence:

- **`SentenceNote.ParsingCase`,** null when the sentence isn't a parsing case. It holds:
  - `Name`: what the case is about, as its file's name says today, such as "ichidan imperative". It's the case's name
    in the test explorer.
  - `Why`: why the case exists, such as the misparse it caught, or what is wrong with how it parses.
  - `State`: verified, broken or unverified.
  - `Expected`: the result the sentence is expected to parse to. For a verified case, a person has found it right. For
    a broken one, a person has found it wrong. It's kept either way, so that a change to it is noticed: a broken case
    that parses otherwise may be fixed.
- **What can be done with it,** as commands of the sentence note, which the note's menu and the MCP server both offer:
  - mark the sentence as a parsing case: `Expected` becomes what it parses to now, and it's unverified;
  - set its state, name and why;
  - accept what it parses to now as `Expected`;
  - stop it being a parsing case.
- **A reparse compares.** Updating the sentence's generated data parses it under every combination of the options, as
  a case does today, and compares with `Expected`.
- **Finding them:** generated tags, as `-::sentence::uses::incorrect-matches` is today, so that the tag tree has them:
  `-::sentence::parsing-case::verified`, `::broken`, `::unverified`, and `::parses-otherwise-than-expected`.

## The steps

1. **The data type,** as above: on the sentence note, stored in the corpus, with its commands, the comparison at a
   reparse and the generated tags. Specified in Core's specifications.
2. **The import, once:** every case file's sentence gets its parsing case in the worktree's corpus checkout, on a note
   already there when one parses identically and on a new note otherwise. `Expected` is what the note parses to, and it
   must equal what the case file expects, for every case. The state is verified: these are the cases Magnus added and
   fixed over months.
3. **Magnus looks at them in JAStudio.** The corpus is committed on his word.
4. **The sentence page shows the parsing case:** its state, its why, and what it was expected to parse to beside what
   it parses to, when they differ.
5. **The case files are generated from the corpus's parsing cases,** with the stripped-down corpus, as the full mode
   refreshes it today. The specifications run on them as now, without the corpus.

After it, on `unidic`: the rebuild with UniDic leaves the parsing cases that parse otherwise than expected findable,
and they are the list to judge.

## Open

- **Whether the cases' check stays as strict as it is,** comparing every parsed word, displayed or not (Claude
  recommends it; put to Magnus on `unidic`). It decides what "parses otherwise than expected" compares.
- **A broken case in the specifications:** whether it's generated as a case that pins what it parses to, or left out
  until it's verified.
