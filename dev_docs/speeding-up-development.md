# Speeding up development: the next big pieces

**Status: all four pieces of work agreed (Magnus, 2026-11-09: "We will do them both", and the other two the same day).
1, 3 and 4 are scoped; 2 not yet.** Each starts by scoping it: what it takes, and whether it's feasible, before any code.

These are the changes the collaboration rule "Build the tools the work will need again now, not later — and propose the
big ones" asks for: changes to how JAStudio is developed, rather than to what it does.

- **Ending the remaining round trips through Magnus (1 and 2).** JAStudio's MCP server (`jastudio-mcp.md`) took the
  corpus loop off Magnus: editing notes, rebuilding and analyzing now run in seconds, in the headless JAStudio, without
  him. Two round trips remain, both for the code that runs inside Anki: seeing what JAStudio shows, and getting a new
  build into the live JAStudio. Each is a step only Magnus can do now.
- **Parsing specifications true to the corpus, and exact (3 and 4).** The specifications of how sentences parse run
  against copies of vocab made by hand, which drift from the real ones, and check only a small part of each result.

## 1. Seeing JAStudio's windows and cards with real data, without Anki

**Now:** checking a change to a window or a card takes Anki restarted on the new build, and Magnus opening the window or
card and describing what he sees, or sending a screenshot.

**Goal:** a tool of the headless JAStudio that renders a window, or a note's card, on the real corpus, to an image Claude
reads: the loop of changing the UI and looking at it, without Magnus.

**Scoped (2026-11-09): feasible, both halves, from mechanisms JAStudio already has.**

### Windows

- **Rendering works.** `JAStudio.UI.Views.Specifications` opens JAStudio's windows on Avalonia's headless platform and
  saves each as `RenderedWindows/<name>.png`: laid out and drawn with Skia as on a screen, with the system's fonts. But
  on the test environment's notes.
- **The headless JAStudio can run them on the real corpus, in its own process.** Inside Anki, Avalonia already runs on a
  thread of its own (`JAStudioAnkiAppRoot` starts `AvaloniaUIThread` with the platform's own windowing). The headless
  JAStudio would start the same application on a thread of its own with the headless platform instead, as the window
  specifications do, and reference `JAStudio.UI`.
- **The tool:** `render_window` opens a window by name, as JAStudio opens it, renders it, and gives Claude the image.
  The windows: the workspace window showing a note, in its viewer or its editor (`WorkspaceWindow.OpenNote` and
  `EditNote`), and the dialogs the main menu opens, such as the options, the readings mappings and the import profiles.
  Each needs a way to be opened by name, a small list in the MCP server.
- **Live too, later:** in the live JAStudio, the same tool could render the windows Magnus has open, as he sees them, with
  Avalonia's `RenderTargetBitmap` (`CaptureRenderedFrame` is the headless platform's).
- **Driving the windows, later:** the headless platform simulates the keyboard and the mouse, so a tool could also type
  in a search or press a button: reproducing a bug in the UI on the real corpus, and checking the fix.

### Cards

- **Rendering works.** A note's cards are Blazor pages served by `CardServer` (`JAStudio.Web`), such as
  `/card/vocab/{Side}?NoteId=…&CardType=…` (`web-rendering-architecture.md`), shown in Anki in an iframe. Checked
  2026-11-09: headless Edge, run from the command line, made an image of 撒く's vocab card from the `CardServer` running
  inside Anki in 0.8 seconds, looking as it does in Anki.
- **The headless JAStudio would start a `CardServer` of its own,** the one Anki starts, with a note link opener that opens
  nothing.
- **The tool:** `render_card` renders a note's card, by card type and side, and gives Claude the image; perhaps also the
  page's text, which is cheaper when only the content matters.
- **The browser:** Playwright for .NET, which brings its own Chromium on every platform and can wait for a page to settle
  and click in it; or the installed Edge or Chrome, run from the command line as in the check, which needs finding on
  each platform.

### Open questions

- **How the tools give Claude the image:** as an image in the tool's result, which MCP supports, or as a file Claude
  reads.
- **Fidelity, to check on the first images:** Japanese in the windows rendered headless, since the window specifications'
  images so far show only Latin text; each window's size, from where it was last (`WindowPlaces`), for a screen of
  1920×1080 at least; and Anki's own styles around a card's iframe.

### Progress

1. **Done (2026-11-09): `render_window`, for the main menu's windows that need no web view:** the options, the readings
   mappings and the import profiles. Each renders on the real corpus and the add-on's configuration, at the size Magnus
   last left it. Japanese renders, in the default font and in JAStudio's Japanese font. What it took:
   - The headless JAStudio got user files of its own, a copy of the add-on's made as it starts: the add-on's are the live
     JAStudio's, and closing a window, or the workspace saving its state, would otherwise have moved Magnus's windows and
     changed his workspace.
   - It reads the note types for raw notes from the Anki collection, as JAStudio inside Anki does, for the import
     profiles window.
   - Not the text-to-speech voices dialog: opening it lists the voices of the services online, with Magnus's keys, and
     opens an audio player.
   - It found that opening the options dialog saved the configuration once for each option, and reloaded every open
     workspace page as many times: setting a configuration value to the value it had counted as a change.
2. **Done (2026-11-09): `render_the_workspace` and `render_card`.** The workspace shows a note's page in a
   `NativeWebView`, WebView2 on Windows, which needs a real window: on the headless platform it failed as it was created,
   and since Magnus's saved workspace opens his tabs, the workspace window with it. So the headless JAStudio has a
   `CardServer` of its own, and a headless browser renders its pages (`PlaywrightWebPageImages`: the installed Edge,
   driven by Playwright, muted). The headless workspace's tabs show their page as that image (`NoteTabImageView`), in
   place of the web view (`NoteTabView`): the workspace window is created with one or the other. Checked on the real
   corpus: the workspace as Magnus left it, his search, his tabs and the editor, and a sentence opened in a new tab; and
   要する's card, as Anki's reviewer shows it. Follow-ups:
   - **The window's size:** the headless workspace opens at 1600×900, not at Magnus's 2374×1696, since his remembered
     place is on a screen the headless platform doesn't have. Narrow tabs clip a page as his wouldn't.
   - **The build output the gateway copies at every start grew to 458 MB:** Playwright's driver is 104 MB, and SkiaSharp's
     native debug symbols, which nothing reads at runtime, 101 MB. Starting took 7.8 seconds rather than 7.
3. **Then** the live versions, then driving the windows.

## 2. New builds reaching the live JAStudio without restarting Anki

**Now:** a new build reaches the JAStudio inside Anki only when Anki restarts. The addon copies the newest build into
`src/runtime_binaries` as Anki starts (`jastudio/dotnet/copy_runtime_binaries.py`), and pythonnet loads it into Anki's
process, where it stays until Anki closes. So every change to code that runs in Anki waits for Magnus to restart Anki,
and anything checked live waits with it.

**Goal:** a new build reaches the live JAStudio without Anki restarting, so that Claude can put one there, and check it,
on its own. The likeliest way is JAStudio's .NET side running in a process of its own, which Claude or Magnus restarts on
a new build while Anki keeps running, with the addon in Anki talking to it. That points the same way as the corpus data
model's goal of using Anki only for reviewing and importing (`corpus-data-model.md`).

**Open questions:**
- Whether it's feasible, and at what cost: how much of what crosses between Anki and JAStudio now, through pythonnet and
  `JAStudio.PythonInterop`, can cross a process boundary instead, such as Anki's hooks, rendering cards, and the windows
  shown over Anki's, and how fast each crossing must be.
- What has to stay in Anki's process.
- Whether a smaller way does enough: loading JAStudio's assemblies into a .NET `AssemblyLoadContext` that can be unloaded,
  so a new build replaces the old inside Anki's process.

## 3. Parsing specifications that run against the vocab of the actual corpus

**What's set (Magnus, 2026-11-09):** the specifications of how sentences parse must match the actual corpus. **Every other
detail below is preliminary,** a starting point for debate rather than a decision.

**Now:** the specifications that state the words a sentence parses into, such as `TextAnalysisWithSelectDataTests`, run
against about 143 vocab copied by hand into the specifications' sample data (`VocabLists.TestSpecialVocab`), with forms,
compound parts and matching configuration of their own. The copies drift from the real vocab, and nothing keeps them
in step: a specification can pass while the corpus parses the same sentence otherwise, or fail over a copy that's out of
date rather than over the code.

**A possible design, two modes (Magnus's idea):**
- **The full mode** loads all the vocab of the corpus, parses each specification's sentences against it, and writes every
  vocab that contributed to parsing any of them into a stripped-down corpus. That corpus is committed as source, along
  with the specifications.
- **The everyday mode,** the one most runs use, parses against the stripped-down corpus: small, fast, and the same on
  every machine, since it's committed. CI can run it too, though CI doesn't check out the corpus repository.
- **Running the full mode is a manual step, but a frequent one,** as the corpus changes. It brings the stripped-down
  corpus up to date, and a specification that fails in it shows a change in the corpus that changed how one of the
  specification sentences parses.

**Points to debate:**
- **Which corpus the full mode reads:** the commit the source commit records as `src/jas_database`, so that a run can be
  repeated, or the corpus checkout as it is.
- **Which vocab count as contributing to a sentence's parsing:** Magnus's guess, 2026-11-09, is every vocab the parsing
  machinery loads at all, tracked by a hook in it. That errs on the safe side: a vocab never looked at can't have changed
  the result, while one matched only to be rejected, or shadowed, still can. The full mode could check its own answer:
  every sentence must parse the same against the stripped-down corpus as against the whole one.
- **Accepting a change:** when a corpus change changes how a specification sentence parses, and the new parse is right,
  updating the specification's expected words should be cheap.
- **Which specifications move:** those of the form "this sentence parses into these words". The specifications of a single
  matching mechanism, such as a godan potential or a forbidden dictionary-form start, may be better off keeping the few
  vocab they create themselves, as the AI-created ones do: with the real vocab, a change to an unrelated vocab could
  break them, and what they specify would be harder to see.
- **Loading:** the corpus has about 28,000 vocab, and every specification builds a collection of its own. The full mode
  might load them once a run, from the snapshot the corpus keeps for fast loading (`vocab/snapshot.bin`).

## 4. Parsing specifications that expect the exact parsing result

**What's set (Magnus, 2026-11-09):** a parsing specification expects a sentence's exact parsing result, so that any change
to it fails the specification. **Every other detail below is preliminary.**

**Now:** about 174 cases check a small part of the result, in a string format of their own. `TextAnalysisWithSelectDataTests`
checks only the set of forms parsed, without where or to which vocab. `SentenceAnalysisViewModelWithSelectDataTests`
checks the words displayed, written such as `いただける:acceptable`, `る:う` or `ぇ:[MISSING]:emergency`. A change to
anything else, such as a word matched to another vocab with the same form, passes unnoticed.

**A possible design (Magnus's idea):** the cases of one parameterized specification are JSON files in a folder, each file
one case: the sentence, optional comments on why the case was made and what bug it caught, and the whole parsing result,
as JSON. Or each case a folder, of one or more files holding the sentence, the explanation and the result.

**Points to debate:**
- **What "exact" covers:** the parsing result as the corpus stores it, every parsed word with its form, place, surface or
  base form, vocab and whether it's displayed: what Magnus sees, and what `diff_the_changed_notes` compares. Not the
  whole analysis, with its tokens, candidates and the reasons matches fail, which would fail on every change inside the
  parser that changes nothing anyone sees. That could come later as an optional part of a case, which is when a folder
  per case would pay off; until then, a file per case is simpler.
- **What a failure says:** not that two JSON texts differ, but how the words differ, as `diff_the_changed_notes` reports
  them: words added, removed, now displayed, no longer displayed, each with its vocab.
- **Accepting a change must be cheap,** or the specifications become a burden: a mode that writes the actual results over
  the expected ones, after which the git diff shows what changed, for review, and the commit records the change accepted.
  Never in an everyday run.
- **A new case's expected result needs a real review:** recorded, it says what the parser does, not what's right. The
  comment on why the case exists is part of that review.
- **Naming vocab:** an exact result names each vocab by its id, which works with the stripped-down corpus of item 3, where
  vocab keep their real ids, with the question beside it to read. With the vocab copied by hand, ids are new on every run.
  So this item builds on item 3, and they might best be done together.
- **The sentence's own configuration,** its hidden and incorrect matches and highlighted words, affects its parsing: a case
  may need to hold it.
- **Making cases:** a tool of the MCP server could write a case from a sentence of the corpus, or from text, with its
  current result, for review, so a fix for a misparse comes with its case. The existing cases convert by running each once
  and checking the result against their old expectations.
- **The name of a case's file is its name** in the test explorer, so it should say what the case is about.

## 3 and 4, scoped together (2026-11-09): feasible, and best built together

**Why together:** an exact result names each vocab by its id, which only a corpus of the real vocab gives (item 4's point on
naming vocab). And specifications against the real vocab are worth most when they check the whole result.

**What scoping found:**
- **The drift is real.** Two of the existing cases, run against the real corpus in the headless JAStudio: 食べれる parses
  as its case expects. 座っているのはいただけない doesn't: the case expects の, は and いただける, while the corpus parses のは
  and いただけない, for which it has vocab and the copies don't.
- **Parsing reaches the corpus only through the vocab collection's lookups,** so a hook there records exactly "every vocab
  the parsing machinery loads". The analysis is given the vocab collection, the dictionary and the settings
  (`AnalysisServices`), and finds vocab by form, question and so on (`CandidateWordVariant`, `TextLocation`, `WordInfo`,
  `IchidanGodanPotentialOrImperativeHybridSplitter`, `DictLookup.IsDictionaryOrCollectionWord`), all of it answered by
  `VocabCache`. Recording every vocab a lookup there returns covers them all, and any lookup added later.
- **So the stripped-down corpus parses each case exactly as the whole corpus does, by construction:** every lookup returns
  the same vocab, since all it returned is in the stripped-down corpus. The full mode's self-check, every case parsing the
  same against both, then only catches a way of reaching vocab that bypasses the lookups.
- **The stored parsing result doesn't depend on the options.** The two settings parsing reads, hiding all compounds and
  hiding transparent compounds, apply only to the sentence breakdown (`TextAnalysis.ForUI`). So a case's exact result
  holds whatever Magnus's options are. The breakdown's displayed words do depend on them: a case about those would hold
  the settings it assumes.
- **The dictionary is the code's, not the corpus's:** parsing looks words with no vocab up in JMdict, the same on every
  machine.
- **Loading needs nothing new.** `FileSystemNoteRepository` loads notes from any folder in the corpus's layout, so the
  stripped-down corpus can be a folder of the corpus's own vocab files, loaded by the same code.
- **One collection for all the cases, loaded once per run.** Now every specification builds a collection of its own, from
  the 143 copies. A case can instead be parsed with `TextAnalysis` directly, from its text and configuration, without
  creating a sentence note, so all the cases share one collection that nothing changes.
- **Size:** each sentence sampled loads 20 to 30 vocab, many of them shared (particles, endings). The about 174 cases might
  load 1,000 to 2,000 vocab: 5 to 10 MB of the corpus's JSON, at about 5 KB a vocab. A guess, for the first run to measure.
- **CI** checks out only the stub generator's submodule, not the corpus. The committed stripped-down corpus lets the
  everyday mode run there.
- **Both repositories are public,** so committing vocab to the source repository publishes nothing new.

**Recommended: build it as tooling first, in the code, and the corpus-itself variant (below) later, on top of it.** The
case files, the stripped-down corpus, the full mode and accepting are what the corpus variant needs too, as its export to
the test suite. The corpus variant adds data to sentence notes and a way to review in the UI: a change to the domain model,
better designed once the tooling shows how specifications like these work in practice.

**The first increment:**
1. The recording hook in `VocabCache`.
2. A case: a JSON file holding the text, the sentence's configuration if it has one, why the case exists, and the exact
   parsing result as the corpus stores it: every valid match, with its form, place, surface or base form, whether it's
   displayed, and its vocab's id, with the vocab's question beside it to read, not compared.
3. A parameterized specification over the case files, against the stripped-down corpus. A failure says how the words
   differ, as `diff_the_changed_notes` does.
4. The full mode: the same specification, run with a switch, against the corpus checkout. It writes the stripped-down
   corpus, checks it, and reports the cases whose result differs. And accepting: another switch, which writes the actual
   results over the expected ones, for review in git's diff.
5. A handful of the existing cases converted, to prove the loop. Then the rest, reviewing each difference with Magnus: the
   corpus right, the code wrong, or the old expectation an artifact of the copies.

**Calls made, open to change:**
- The full mode reads the corpus checkout as it is (`src/jas_database`), not the commit the source records. That's what
  the specifications should match, and the committed stripped-down corpus records what a run used.
- The full mode is a mode of the specifications, run from the IDE or the command line, so Magnus can run it as easily as
  Claude. A tool of the MCP server that makes a case from a corpus sentence comes later.

## 3 and 4 in the corpus itself: sentences that specify how they parse (Magnus's idea, 2026-11-09)

**Preliminary, like the rest of 3 and 4.** It could replace their designs, or feed them.

**The idea:** the sentences that specify parsing are actual sentences of the corpus, with their configuration, marked as
specifications. JAStudio itself shows a specification that fails, when an edit to a vocab or a change to the code changes
how the sentence parses: Magnus in its UI, Claude through the MCP server, both on the real collection as loaded. A
change that improves the parse is accepted; one that doesn't means the vocab edit or the code change is fixed instead.

**What it could look like:**
- **A specification is data of the sentence note,** like the rest of its data, not a tag: why it exists, and what bug it
  caught; and its accepted parsing result, kept beside the current one the sentence already stores.
- **It fails when the current result differs from the accepted one.** The difference is what `diff_the_changed_notes`
  already shows for a sentence: the words added, removed, now displayed and no longer displayed, each with its vocab.
- **Where failures show:** for Magnus, perhaps as a search condition, such as the parse differing from the accepted one,
  so the workspace's search lists them, each with how its words differ and a way to accept the new result. For Claude,
  tools that list the failing specifications, show how each differs, and accept a result.
- **Accepting** makes the current result the accepted one: a change to the corpus, versioned and committed like any other.

**Why it's attractive:**
- **Vocab edits are checked, not only code changes.** Magnus's edits are likely the most frequent cause of a sentence
  parsing differently, and today nothing flags them.
- **Specifications are made where the sentences are:** a misparse found while studying gets fixed, and the sentence is
  marked, with its configuration as it really is.
- **The suite grows on its own:** any sentence whose parse has been checked can be marked, with why.

**Open points:**
- **The test suite and CI don't see the corpus.** Item 3's two modes fit as an export: the marked sentences, and every vocab
  their parsing loads, written to a stripped-down corpus committed with the source, so that a code change fails in an
  everyday test run too. Or the headless JAStudio's check, run through the MCP server after a rebuild, is enough.
- **A broad vocab edit could fail many specifications at once:** reviewing and accepting them needs to work for many at a
  time, without making acceptance careless.
- **The specifications of a single matching mechanism** stay in the code, with the few vocab they create themselves.
- **It changes the domain model and the UI:** new data on sentence notes, and a way to review and accept. Normal work, to
  design with Magnus, beyond tooling.

## Order of work

Not decided. Each starts with scoping, and the scoping decides the order.

**Claude's proposal (2026-11-09), asked what it needs most:**
1. **3 and 4 together.** Changes to parsing are the most common and the riskiest work, and today the only reliable check
   of one is a full rebuild and a review of its diff. Exact specifications against the real vocab would make most of
   that check a test run, in CI too.
2. **Driving the headless windows** (item 1's step 3): small, and the server is Claude's to change. It makes a UI bug
   reproducible, and its fix checkable, without Magnus.
3. **Scoping 2.** Its biggest gain, Claude seeing the UI without Anki, item 1 now gives. What remains is a new build
   reaching Magnus without a restart, and checking code that only runs inside Anki: worth it, but the biggest change of
   the four.
