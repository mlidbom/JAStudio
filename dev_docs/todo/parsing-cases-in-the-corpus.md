# The parsing cases move into the corpus

> **Status: agreed with Magnus 2026-11-15, on the branch `parsing-cases-in-the-corpus`.** Steps 1 and 2 are done: the data
> type is built, and the 230 cases are imported into the worktree's corpus checkout, where they wait, uncommitted, for
> Magnus to look at them. It changes [the parsing cases](../parsing-cases.md), whose case files become generated from
> the corpus.

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
- **A parsing case is verified, broken, or unverified.** Only the verified ones are a gold standard, the
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
- **What a person verifies is the displayed vocab, for a start.** "We need more granular specifying of what is verified.
  For starters, what we will verify, if you agree, is the displayed vocab without any consideration for non displayed
  vocab, or correct breakdown of compound parts. Basically What you can see in the results when all options to expand
  or show extra information, or hide compounds etc are all off."
  - **A change outside it is not an error.** "A parsing change that does not change this is NOT an error. Just a suspect
    change needing review. And we review only that. Probably."
  - **Other aspects come later.** "So for different aspects of the sentence, there can be different (Evaluations might
    not feel quite right ?). For now I'm not too confident on defining any except the view that is only the matching
    displayed vocab. Period."
  - **Open, in his words:** "how to model 'suspect' for the other parsing aspects and how to build the UI for making a
    judment about that".
- **A Parsing cases tab in the navigation pane.** "I'd like a Parsing cases tab on the left with filtering and sorting
  by status and text, separate from the search."
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

## The data type (agreed with Magnus, 2026-11-15: "Let't code that and see what happens")

The specifications already have the model, and its words are kept: a `ParsingCase` with a `Name`, a `Why` and an
`Expected` result (`ParsingCaseResult`: the parsed words, and the breakdown under each combination of the options that
change it), which `Accept` replaces. It moves into Core, onto the sentence.

**What a parsing case records, and what a person vouches for, are two things:**

- **`Expected` records everything,** as a case file does today. Nobody vouches for all of it.
- **A person's verdict is on one aspect of it.** There's one aspect for now, the displayed vocab: the words the
  breakdown displays with every option off, each with the vocab it's matched to. Its verdict is verified, broken or
  unverified. Further aspects, such as a compound's parts, get a verdict of their own when it's clear what verifying
  them means. The word is "verdict" (Magnus: ""Verdict" is good").

**`SentenceNote.ParsingCase`,** null when the sentence isn't a parsing case:

- `Name`: what the case is about, as its file's name says today, such as "ichidan imperative".
- `Why`: why the case exists, such as the misparse it caught, or what is wrong with how it parses.
- `Expected`: the result the sentence is expected to parse to.
- `DisplayedVocab`: the verdict on the displayed vocab.

**A reparse compares, and what differs is of two kinds:**

| What differs from `Expected` | The displayed vocab are verified | They aren't |
|---|---|---|
| The displayed vocab | The case fails: an error | Changed: to review |
| Only something else | Suspect: to review | Suspect: to review |

- **"Suspect" isn't stored.** It's a difference from `Expected` in what nobody has verified. What's stored is
  `Expected` and the verdict.
- **The displayed vocab are compared by where a word starts and the vocab it's matched to,** and by its form for a word
  with no vocab. Whether it's matched by its surface or its base form doesn't count. Magnus: "That sounds about right.
  [...] it comes down to what I see in the breakdown with nothing expanded or hidden. Which should be pretty much
  exactly what the tests tested before we started migrating to gold-standard sentences..." What works best is found by
  using it: "As long as we get all the data into the corpus before starting to run the unidic code we can play around
  with it".
- **Judging a suspect change** has two outcomes for now. Accepting it makes what the sentence parses to its
  `Expected`, and claims nothing about its being right. Finding it wrong has no verdict to set yet outside the
  displayed vocab: the case's `Why` says what is wrong, and it stays unaccepted. The first such cases show which
  aspect to define next.

**The commands of the sentence note,** which the note's menu and the MCP server both offer:

- mark the sentence as a parsing case: `Expected` becomes what it parses to now, and its displayed vocab are
  unverified;
- set the verdict on its displayed vocab, its name and its why;
- accept what it parses to now as `Expected`;
- stop it being a parsing case.

**The Parsing cases tab,** in the workspace's navigation pane, beside the search: every parsing case, with its
sentence, its name, its verdict, and what differs (nothing, the displayed vocab, or something else), filtered and
sorted by those and by the text. With it, no generated tags are needed to find the cases. (Magnus: "OK for now. Later I'm apt to
want more :)")

**The sentence page of a case that differs** can show the expected result as a second sentence breakdown, to the right
of the current one (Magnus: "I vas imagining the ability to toggle on seeing the old result, as another sentence
breakdown, to the right of the current one. [...] Side by side comparison. We could bolt on coloring and/or messages
and stuff to that to make it extra clear what happened..."). The toggle is offered for any parsing case whose result
differs in any way. The differences as lines of text, as the cases' failure messages word them today, go where they
fit: on hover, or below. With the commands to accept, and to set the verdict and the why.

## The steps

1. **The data type,** as above: on the sentence note, stored in the corpus, with its commands and the comparison at a
   reparse. Specified in Core's specifications.
2. **The import, once:** every case file's sentence gets its parsing case in the worktree's corpus checkout, on a note
   already there when one parses identically and on a new note otherwise. `Expected` is what the note parses to, and it
   must equal what the case file expects, for every case. The state is verified: these are the cases Magnus added and
   fixed over months.
3. **Magnus looks at them in JAStudio.** The corpus is committed on his word.
4. **The Parsing cases tab, and the sentence page showing the parsing case:** its verdict, its why, and what it was
   expected to parse to beside what it parses to, when they differ.
5. **The case files are generated from the corpus's parsing cases,** with the stripped-down corpus, as the full mode
   refreshes it today. The specifications run on them as now, without the corpus.

After it, on `unidic`: the rebuild with UniDic leaves the parsing cases that parse otherwise than expected findable,
and they are the list to judge.

## Where it stands (2026-11-15)

**Step 1, the data type, is built** as agreed above (`cf1b655ad`), in `JAStudio.Core/Note/Sentences/ParsingCases`, with
`SentenceNote.ParsingCase` in `SentenceNote.ParsingCase.cs`.

- **The corpus stores it in the sentence's file,** as `parsingCase`, only for a sentence that is one. No other note's file
  changes: a full rebuild on the new code changed nothing.
- **JAStudio from before this can't read a sentence that has it:** a note file with a property the code doesn't know
  fails to load. So the corpus's own line gets the parsing cases only together with this code.
- **Nothing is compared at a reparse yet.** The comparison is made when asked for
  (`SentenceNote.CompareWithWhatTheParsingCaseExpects`), which the tab of step 4 will do.
- **The search finds the parsing cases** (`85b3b9c58`): the true-or-false condition "Parsing case", in the workspace's
  search and the MCP server's `search_notes`. It's how they're found until the tab exists.
- **The specifications' types moved into Core:** `ParsingCaseResult` and the breakdown types. The harness uses them as
  before.

**Step 2, the import, is done, in the worktree's corpus checkout, uncommitted.** A script in Claude's scratch folder
ran it through the headless JAStudio's MCP server, with the commands a person has: create a note, edit its
configuration, mark it as a parsing case, name it, say why, verify its displayed vocab.

- **All 230 cases are parsing cases of the corpus, verified,** each on a note of its own.
- **Every one expects exactly what its case file expects:** every parsed word, and every breakdown under every option.
- **55 are on sentence notes that were already in the corpus,** 175 on new ones.
  - **19 of the 55 highlight vocab that their case files don't,** such as これをください。 highlighting これ: the sentences
    Magnus studies them in. They parse identically all the same, so they took their cases, as his rule says:
    "identical, in terms of parsing". Claude first required the same configuration too, as it had said it would, and
    these 19 then got copies. It changed that on its own judgment, for Magnus to confirm.
  - **One sentence already in the corpus got a copy:** 勝つんだ, whose note highlights うんだ besides the case's んだ, and
    parses otherwise for it.
- **One new note's text differs from its case file's by a trailing space:** "noise characters are ignored - 私 among
  noise". JAStudio trims a sentence's question. It parses identically.
- **A case's name is its file's name without the sentence,** such as "ichidan imperative", or "hidden exclusions -
  hidden matches (2)" for a file with a third part.
- **What the import changed in the corpus,** after a full rebuild: 175 sentences added, 55 changed only in their
  `parsingCase`, and 382 vocab changed only in their `sentenceCount`, by the new sentences. No parsed word changed in
  any sentence.

**Next:** Magnus looks at them in JAStudio (the standalone JAStudio on the worktree, with the search condition
"Parsing case"). Then steps 4 and 5. Handing the corpus over to its own line needs this branch's code on `main` first,
and Anki closed while Magnus's corpus checkout takes the commits.

## Open

- **Whether a suspect change should fail the specifications.** It does today: any change to how a case parses fails
  it. Magnus, 2026-11-15: "whether that is good is questionable. I'd not change it right now though unless you have a
  strong argument for it." Claude has none, so it stays.
- **How Claude reads "we review only that":** that a review looks at what changed, not at the whole sentence again.
- **A broken case in the specifications:** whether it's generated as a case that pins what it parses to, or left out
  until it's verified.
