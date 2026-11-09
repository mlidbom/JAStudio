# Tooling for AI agents: ending the remaining round trips through Magnus

**Status: both pieces of work agreed, not yet scoped (Magnus, 2026-11-09: "We will do them both").** Each starts by
scoping it: what it takes, and whether it's feasible, before any code.

JAStudio's MCP server (`jastudio-mcp.md`) took the corpus loop off Magnus: editing notes, rebuilding and analyzing now run
in seconds, in the headless JAStudio, without him. Two round trips remain, both for the code that runs inside Anki:
seeing what JAStudio shows, and getting a new build into the live JAStudio. Each is a step only Magnus can do now, which
is what the collaboration rule "Build the tools the work will need again now, not later — and propose the big ones" says
to look for.

## 1. Seeing JAStudio's windows and cards with real data, without Anki

**Now:** checking a change to a window or a card takes Anki restarted on the new build, and Magnus opening the window or
card and describing what he sees, or sending a screenshot.

**Goal:** a tool of the headless JAStudio that renders a window, or a note's card, on the real corpus, to an image Claude
reads: the loop of changing the UI and looking at it, without Magnus.

**What exists:**
- `JAStudio.UI.Views.Specifications` opens windows on Avalonia's headless platform and saves each as
  `RenderedWindows/<name>.png`, but on the test environment's notes, and in a process of its own, since the headless
  platform takes over the UI dispatcher.
- A note's cards are Blazor pages served by `CardServer` (`JAStudio.Web`), such as
  `/card/vocab/{Side}?NoteId=…&CardType=…` (`web-rendering-architecture.md`). They're HTML: an image of one takes a browser
  engine.

**Open questions:**
- Whether Avalonia's headless platform can run in the headless JAStudio's process, or needs a process of its own that
  loads the real corpus.
- What renders a card to an image: a headless browser, such as Playwright's Chromium, or something closer to the web view
  Anki shows cards in.
- How close to what Magnus sees the images must be: fonts, theme, and the screen size JAStudio is made for, 1920×1080 at
  least.

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

## Order of work

Not decided. Both start with scoping, and the scoping decides the order.
