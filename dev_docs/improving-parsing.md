# Improving how sentences parse

> **Status: started 2026-11-11, with Magnus.** How the vocab corpus and matching get better, how we know they did, and
> what we've found so far. The matching configuration's redesign is in [matching-configuration.md](matching-configuration.md);
> growing the sentence corpus is a to-do of its own, [expanding-the-sentence-corpus.md](todo/expanding-the-sentence-corpus.md).

## The goal

Sentences parse right with no corrections on the sentence, above all sentences nobody has looked at yet. Adjusting vocab
by hand is expected; adjusting sentences shouldn't be. In Magnus's words (2026-11-11): "The more sentences that we have
never seen before that parse well without any manual adjustments, the more correct and valuable our vocabulary corpus
is." There's a limited amount of vocab and a virtually unlimited number of sentences, so everything that makes vocab
match well pays off in every sentence JAStudio's users will ever bring.

And improving parsing should get faster and safer as it goes, not slower and riskier: every fact we gain about how a
sentence should parse makes the next change safer to make.

## The ways to fix how a sentence parses

Roughly from the most leverage to the least. Judgment picks among them, not only counts (Magnus: "Apply judgment ...
not pure numbers").

1. **A vocab for a short, clearly recognizable phrase.** Quite possibly the most important (Magnus, 2026-11-11). A phrase
   is longer than a word, so its match shadows the words inside it, and it parses reliably even where its parts aren't
   parsed right. Conditions on top make it more reliable still. For example, からね at the end of
   "夕飯から一つ減らしますからね" displays as one word, whatever its から and ね would have parsed to. It works because the
   display shows the longest valid match at each place, and a phrase is far less ambiguous than its parts.
   - **What to watch:** a phrase can match where it isn't meant, across the boundary of two words that happen to spell
     it. A condition on the phrase fixes that.
2. **Conditions on a vocab,** its match-condition: when a word matches it and when not, such as "the preceding token is
   a masu stem".
3. **Shared-conditions:** a condition defined once and used by many vocab, such as "after a person's name". Improving
   it improves every vocab that uses it. (Built after the core: increment 5 of the matching redesign.)
4. **Rules for many vocab:** a condition applied to every vocab of a kind, chosen by part of speech or flags, such as "a
   suffix needs a word before it". The most leverage, and the most risk: it changes many sentences at once.
5. **A correction on the sentence,** the last resort: a match marked incorrect, hidden, or highlighted. It has no side
   effects, but teaches nothing to the next sentence. A condition that only ever decides one sentence may belong here
   instead (see the generality measure, in [the yardstick](matching-configuration.md#the-yardstick-measuring-what-the-rules-achieve)).

Below all of these is tokenizing (MeCab, and JAStudio's splitters and corrections of its output). Conditions run after
it and can't change its tokens, so a misparse that starts there can only be patched by rejecting matches, or by a phrase
vocab that spans it.

## Knowing what's right: facts about how sentences should parse

What we know now:
- **The parsing cases** (230, `dev_docs/parsing-cases.md`): sentences in the source, each with its exact expected
  result, run with every test run.
- **The corrections on sentences** (2026-11-10): 442 matches marked incorrect and 116 hidden.
- **The highlighted words:** 23,383, mostly the word a course sentence is for (Core 2000, WaniKani), not corrections.
  Parsed without the highlight, each must still find its word: tens of thousands of test samples (Magnus's idea, in
  [the yardstick](matching-configuration.md#the-yardstick-measuring-what-the-rules-achieve)).

**Proposed: confirming how a sentence parses (Magnus, 2026-11-11).** A way for Magnus or me to go through sentences and
mark what's known to be right, so that each acts as a regression test from then on:
- **What can be confirmed:** a match in the sentence; the sentence's parse as a whole; or what the breakdown displays,
  with the default options or given ones.
- **A confirmation changes nothing in the parsing,** unlike a correction: it's an expectation. A highlight forces its
  word to be valid; a confirmed word is only expected to be. The course sentences' highlights are really expectations
  of this kind.
- **A change that breaks one is a regression the full rebuild reports,** as the diff of the parsing results already shows
  what changed. So the corpus itself checks every change, and the more is confirmed, the more certain each change.
- **Feedback within a second of an edit** (Magnus, 2026-11-11): an edit to a vocab reparses the confirmed sentences it
  can affect, and says at once what it broke and what it fixed. See the working set, in
  [expanding-the-sentence-corpus.md](todo/expanding-the-sentence-corpus.md).
- **Growing them fast is the point:** an MCP tool for me to confirm a sentence's parse once I've checked it, and an action
  in the sentence's breakdown for Magnus.
- **A confirmed match could say which sense it has, or that the sense is unusual** (Magnus, 2026-11-12), and matches
  confirmed right or wrong come first when a change's results are analyzed. To be designed:
  [todo/annotating-sentences.md](todo/annotating-sentences.md).
- **Related to, not the same as, making the parsing cases part of the corpus:** confirmed sentences live in the corpus,
  with their sentences; they aren't exported as cases (Magnus).
- **The name:** a sentence's parse, or a match in it, is "confirmed" (agreed with Magnus, 2026-11-11). "Reviewed" and
  "assessed" didn't fit.

## Measuring progress

- **The full rebuild's diff:** what a change did to every sentence (`diff_the_changed_notes`, in the MCP server).
- **The highlighted words, parsed without their highlight:** the share that still find their word.
- **The confirmed sentences:** none may break.
- **The yardstick:** the sentence corrections a change makes unnecessary, and the generality measure, how many matches
  each condition decides ([matching-configuration.md](matching-configuration.md#the-yardstick-measuring-what-the-rules-achieve)).

Each is only as good as the sentences it runs over: see [expanding-the-sentence-corpus.md](todo/expanding-the-sentence-corpus.md).

## Findings so far

From the matching redesign's study of the engine and its checks (2026-11-10 and 11). The details are in
[matching-configuration.md](matching-configuration.md).

- **The corpus:** 49,162 sentences and 28,004 vocab. 801 vocab have some matching configuration, and 422 a
  match-condition.
- **Conditions can only take matches away.** A vocab matches only a text that is one of its forms. When a vocab's match is
  rejected, the dictionary can match the text instead, unless a vocab owns the form: that's what a poison word is for.
- **The display shows the longest valid match at each place** (shadowing). That's why phrase vocab work.
- **Priority is only between vocab of the same text.** Overlaps between different spans are settled by the display.
- **Tokenizing depends on the vocab:** a splitter looks vocab up, so editing a vocab can change how other sentences are
  tokenized.
- **Tokenizer quirks seen:**
  - The stem JAStudio splits off the contracted く of 寄ってく has an empty surface, and a rule from before match-conditions
    took it as the sentence's start.
  - The だ of だーめ is read as the past-tense marker (特殊・タ).
  - うるせぇ comes out as う, る, せ and ぇよ.
- **Rules no vocab exercises:** godan and godan imperative (head) were set only on vocab that never match in the corpus.
- **The tools:** a full rebuild takes about 2 seconds ([parsing-speed.md](parsing-speed.md)). `analyze_sentence` shows every step of one sentence's analysis.
  `diff_the_changed_notes` shows what a change did, word by word across the corpus.
