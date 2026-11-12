# Annotating sentences: the sense a match has, and notes for the work

> **Status: to discuss and design (Magnus, 2026-11-12).** Nothing is decided yet beyond what the sections below quote.

## Why: which sense matches, not only which vocab

A sentence's value isn't only in which token or vocab matches, but in which **sense** of it: the same matched word with
different meanings. In Magnus's words: "It is not just about which token or vocab matches, it is which sense matches.
Rare sense matches are valuable. Again, doing this well requires a mind to look at the sentences in most cases."

- **His hand-made highlights often mean this:** where Magnus highlighted a word in a sentence himself, it's often because
  the word is used in an unusual sense there. A highlight (`configuration.highlightedWords`) only forces the word to be
  valid, though: why it was highlighted isn't recorded anywhere.
- **Choosing sentences by their matches alone won't find the senses:** a vocab's matches picked at random give its
  common sense over and over. See [expanding-the-sentence-corpus.md](expanding-the-sentence-corpus.md).

## To discuss and design

1. **Marking that a match in a sentence has an unusual sense, or which sense it has.** What a mark holds (that the sense
   is unusual, a note, the sense itself), who sets it (Magnus in the sentence's breakdown, agents through the MCP server),
   and where it shows.
2. **Which sense matched, as data.** This needs the dictionary's senses as data on the vocab, which they aren't: a vocab's
   answer joins its senses with ` | `, and its parts of speech are the union of every sense's (item 13 of the open
   questions in [corpus-data-model.md](../corpus-data-model.md)). A few answers have a `|` within one sense, such as
   `{a | b}-something`, so the answer text can't simply be split. Magnus (2026-11-12): "In principle we absolutely
   should, since types of speech etc really apply to the sense, not the vocab, but this change would be challenging."
3. **Matches confirmed right or wrong, and if right, their sense.** These are the confirmations proposed in
   [improving-parsing.md](../improving-parsing.md#knowing-whats-right-facts-about-how-sentences-should-parse). When a
   change's results are analyzed, the sentences with such marks come first (Magnus, 2026-11-12): they're what's known.
4. **Free-text notes on a sentence, for the work** (Magnus, 2026-11-12): technical notes and todos, "no matter what they
   are about exactly", as a vocab's `technicalNotes`. A sentence's `comments` are for studying, shown with its card, so
   these would be a field of their own.

## How it serves choosing the working set

- **Varied contexts stand in for senses at first:** different forms, inflections, surrounding words and sources are a
  decent proxy for different senses (Magnus), good enough for an automatic first selection from the reservoir.
- **Then minds:** eventually, agents going over the candidate sentences in loops, judging by hand what to bring in and
  what to leave out (Magnus wants this), and marking the senses they find as they go.
