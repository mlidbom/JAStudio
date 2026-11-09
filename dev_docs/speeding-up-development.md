# Speeding up development: the next big pieces

**Status: all four pieces of work agreed (Magnus, 2026-11-09: "We will do them both", and the other two the same day). The
first is scoped; the others not yet.** Each starts by scoping it: what it takes, and whether it's feasible, before
any code.

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
2. **Next: the workspace window, with `render_card`.** The workspace shows a note's page in a `NativeWebView`, WebView2 on
   Windows, which needs a real window: on the headless platform it fails as it's created. Since Magnus's saved workspace
   opens his tabs, even the workspace window as it opens needs the page drawn another way. The proposal: a `CardServer`
   of the headless JAStudio's own, and a headless browser rendering its pages, which `render_card` needs anyway, with the
   note tab showing that image in the web view's place on the headless platform. That's a seam in `NoteTabView`, which
   Magnus doesn't see.
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
