# Expanding the sentence corpus, intelligently

> **Status: a to-do of high importance (Magnus, 2026-11-11). Not started.** In his words: "We want our sentence corpus to
> be about as big and diverse in type of sentences as we can make it, so expanding on it, and doing so intelligently,
> bringing in high value sentences, is a super important TODO item."

## Why

The goal of matching is sentences that parse right without corrections on the sentence (Magnus, 2026-11-10/11). Vocab
are adjusted by hand as a matter of course, through their match-conditions; sentences shouldn't have to be. The vocab
are finite and the sentences out there unlimited, so every condition that describes the language correctly pays off in
every sentence JAStudio's users will ever bring.

Everything that measures and steers that work runs over the sentence corpus (see
[matching-configuration.md](../matching-configuration.md), the yardstick):
- **The yardstick** counts the sentence corrections a rule change makes unnecessary.
- **The generality measure** counts, for each condition in a vocab's match-condition, the matches it decides across
  the corpus. It tells a rule about the language from a patch for one sentence. It also tells a condition worth
  sharing among vocab from one that shouldn't exist at all, because a correction on its one sentence would do.
- **The full rebuild's diff** shows what a change does to every sentence.

Each is only as good as the corpus it runs over (see also [improving-parsing.md](../improving-parsing.md)). A rule tested on sentences of one kind can't be told apart from a patch
for that kind. The corpus today has 49,162 sentences, and on 2026-11-10 two of today's rules, godan and godan imperative
(head), were set only on vocab that never match in it.

## What "intelligently" could mean (ideas, to work out with Magnus)

- **Diversity of kind.** The corpus is mostly a few anime series and course decks (Core 2000, WaniKani, JLab, Immersion
  Kit). Other registers and genres would test the rules where they're weakest: conversation, narration, news, writing.
- **Sentences that exercise what the corpus doesn't:** grammar patterns that are rare in it, such as sequences of parts
  of speech and inflection forms; vocab with few sentences; rules and conditions that decide few matches.
- **Sentences where parsing is contested:** several vocab competing for a word, words only the dictionary or nothing
  matches. These are where conditions are tested hardest.
- **A measure of a sentence's value:** what it adds to the corpus's coverage, so that candidates from a large source
  can be ranked, and the best brought in first.

## Questions for when the work starts

- **Do sentences brought in for coverage become study notes, or live in the corpus without cards?** Which is right
  depends on how Magnus wants to use them.
- **Sources, and their licenses:** the corpus is meant to serve other users too, so a source's license must allow that.
  Candidates to check include Tatoeba and public-domain literature.
- **Who checks how a new sentence parses?** Its parse is unverified until someone looks. The measures above say where
  to look first.
