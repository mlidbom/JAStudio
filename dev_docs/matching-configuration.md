# The matching configuration as a tree of conditions

> **Status (2026-11-12): increments 1 to 5 of the core are built, and steps 1 and 2 of increment 6, shared-conditions:
> the model, their storage and display, the first five in the corpus, and the vocab using them by name wherever their
> definitions were, through an operation that stays. Next, naming the conditions vocab share that the new finder found,
> then step 3:** see [Increments](#increments). It started as a survey with Magnus, 2026-11-10. Magnus wants the
> matching configuration refactored "to not use hardcoded conditions that only support AND conditions via checkboxes"
> (2026-11-10), into "a flexible tree structure resembling how we implement the note search" (2026-11-08):
> [note-search.md](note-search.md). The goal (Magnus, 2026-11-10): "to be able to craft matching rules for vocab that
> let the parsing produce decent results WITHOUT MANUAL ADJUSTMENT for many more sentences than can be achieved with
> the current limited set of rules that can only do and."
>
> **Clarified (Magnus, 2026-11-11): without manual adjustment *to sentences*.** Adjusting vocab by hand, their
> match-conditions included, is expected. "The big, big, deal is about getting sentences to parse right without us having
> to manually adjust a large percentage of them. The more sentences that we have never seen before that parse well
> without any manual adjustments, the more correct and valuable our vocabulary corpus is." Vocab are finite and sentences
> unlimited, so every condition that describes the language well pays off in every sentence JAStudio's users will bring.
> Shared-conditions and rules-for-many-vocab make the vocab work cheaper; they serve that goal.
> The wider strategy, phrase vocab and confirmed sentences among it, is in [improving-parsing.md](improving-parsing.md).
>
> A first proposal that only translated today's rules into a tree is superseded: that was "half, if that, of the
> point" (Magnus).
>
> **Sharing code with the note search isn't a given** (Magnus): only if it really fits, not because the structure and
> purpose are alike. See [Sharing code with the note search](#sharing-code-with-the-note-search-to-find-out).
>
> **The order (Magnus, 2026-11-10):** first the core support, with every vocab migrated to it, and the test is that
> nothing changes in the parsing. Then the yardstick, and the new power used to make vocab, and groups of vocab, parse
> well.
>
> An earlier discussion of it, from the summer of 2026 on the `flexible-requires-forbids-rules` branch, is lost. Claude
> Code deletes transcripts after 30 days, and the oldest JAStudio transcript left is from 2026-10-31. No document from
> that discussion turned up in either worktree, in any branch's history, or in the corpus repository.

## Terms used in this document

The names are placeholders, to be chosen when the code is written.

- **match-condition:** the tree of conditions a word must meet to match a vocab. It would replace the require/forbid
  rules and the configurable rules.
- **condition-subject:** what a single condition looks at, such as the token before the word, the whole text after it,
  or the form of the vocab that matched.
- **condition-test:** what a single condition checks about its condition-subject, such as "its part of speech is noun"
  or "it starts with one of 湧, わ".
- **shared-condition:** a condition with a name, defined once and used in the match-condition of many vocab, such as
  "sentence end".
- **rule-for-many-vocab:** a condition every vocab of some kind must meet, such as every vocab whose parts of speech
  make it a suffix.
- **universal-requirements:** what JAStudio's code requires of every match, whatever the vocab. Examples are
  `Match.MatchPrimaryValidityRequirements` and the sentence's incorrect matches. They're rules-for-many-vocab, written in
  code.

## What there is now

### The configuration

A vocab's `VocabNoteMatchingConfiguration`, stored in the vocab's `matching`, has three parts:

- **19 require/forbid rules** (`VocabMatchingRequireForbidFlags`, each a `RequireForbidFlagField`). Each is unset,
  required or forbidden. Stored in `matching.requiredOrForbidden`.
- **5 flags** (`VocabMatchingFlags`). Stored in `matching.flags`.
- **5 configurable rules** (`VocabNoteMatchingRules`), each a set of texts. Stored in `matching.rules`.

The Matching tab shows each rule as a three-way choice, each flag as a checkbox, and each configurable rule as a list of
texts (`VocabMatchingConfigEditorViewModel`).

### How matching uses it

`VocabMatch` checks a fixed list of requirements, each a static function that returns a failure or nothing. A match is
valid when none of them fails (`VocabMatch.IsPrimarilyValid`). That's the only way conditions combine: each rule that's
set adds one more requirement. There's no OR and no grouping, except inside a configurable rule's set, where any one of
the texts is enough.

**Each require/forbid rule tests one fact about the word in its sentence.** When the rule is required, a match fails if
the fact is false. When it's forbidden, a match fails if the fact is true. The facts look at these places:

| Where | Rules (the editor's labels) | The fact tested |
|---|---|---|
| The token before the word | Godan, Ichidan, Irrealis, Masu stem, Adverb (head), て-form stem (head), Dictionary form (head), Godan imperative (head) | The token is a godan verb, an ichidan verb, an irrealis form, a masu stem, an adverb; it has a て-form stem; it's a dictionary-form verb inflection, a godan imperative inflection |
| The token before, or the word's first token | Past tense stem | The token before is a past-tense stem, or the word's first token is a past-tense marker |
| The word's first token | て-form stem (start), Dictionary form (start), Godan potential, Godan imperative, Ichidan imperative | It has a て-form stem; it's a dictionary-form verb inflection; it's a godan potential, godan imperative or ichidan imperative stem or inflection |
| The word | Single token, Surface | It's one token, not a compound; it's matched by its surface form, not its base form |
| The word's place in the sentence | Sentence start, Sentence end | Before the word, the sentence is empty or ends with a sentence-start character; the word ends a statement |

**The configurable rules compare texts.** Prefix is not and required prefix: the token before the word ends with one of
the texts. Suffix is not: the token after starts with one. Surface is not: the word's surface is one of them.

### Not all of it decides whether a word matches

- **Conditions on matching:**
  - 18 of the 19 require/forbid rules, all but yield to overlapping following compound;
  - 4 of the configurable rules: prefix is not, required prefix, suffix is not, surface is not;
  - the poison word flag, which fails every match.
- **Conditions on display,** that is, whether a valid match shows in the sentence breakdown:
  - "yield to overlapping following compound", a require/forbid rule;
  - "yield to surface", a configurable rule;
  - "compositionally transparent compound", a flag that acts through a breakdown option.
- **Facts about the vocab that other code reads:**
  - **Inflecting word** makes the word *before* the vocab count as inflected (`CandidateWord.IsInflectedWord`), so it
    changes how other words match.
  - **Question overrides form** changes the form a match shows.
  - **Match with preceding vowel** is a to-do flag: nothing reads it, and no vocab has it.
- **Side effects of two rules:**
  - Dictionary form (start), when required, also expands the forms' dictionary-form-ending placeholders
    (`VocabNoteForms.ExpandsDictionaryFormEndingPlaceholders`).
  - Surface, when required, narrows the sentences a vocab's reparse looks at (`SentenceCollection.PotentiallyMatchingVocab`).
- **Priority.** Two vocab can match the same word with neither owning the form, or both. Then the one whose
  configuration weighs more wins (`VocabNoteMatchingConfiguration.CustomRequirementsWeight`). Each rule that's set adds its
  weight:
  - a required require/forbid rule: 10 to 50;
  - a forbidden one: 1;
  - a required prefix: 10;
  - each other configurable rule in use: 2.
- **Tags JAStudio maintains:** four of the configurable rules in use tag the vocab, below `Tags.Vocab.Matching.Uses`
  (`LocalNoteUpdater.TagVocabMetadata`).

### Irregularities that a translation must keep

- **Dictionary form (start) is forbidden by default.** Unset fails as forbidden does. Required both allows the fact and
  requires it, and there's no "allowed". So forbidding it differs from leaving it unset only in weight (1) and in the
  failure reason: the 70 vocab that forbid it gain nothing else.
- **Surface tests different facts when required and when forbidden.** Required: the word is matched by its surface
  form, inflected or not. Forbidden: the word is matched by an inflected surface, one that differs from its base form. A
  word whose surface is its base form has no base variant (`CandidateWord.BaseVariant`), so it's matched by its surface.
- **Defaults from parts of speech and options:**
  - A complete な-adjective yields its last token to an overlapping following compound unless that's forbidden.
  - Some parts of speech make a vocab an inflecting word without the flag.
  - Three breakdown options yield the last token of suru-verb, passive and causative compounds.
- **The universal-requirements apply to every match:**
  - 7 on validity, one of them the sentence's incorrect matches;
  - 2 on display, one of them the sentence's hidden matches;
  - shadowing;
  - the sentence's highlighted words, which make a match valid whatever else fails.

### What the corpus uses

Surveyed in the headless JAStudio's corpus worktree, at `1a9ac0e10e5`:

- **801 of 28,004 vocab have any matching configuration,** and 560 of those have a single setting. None has more than
  six (2 vocab), and 42 have four or more.
- **Godan and godan imperative (head) are set on no vocab, and neither is match with preceding vowel.**

| Require/forbid rule | Required | Forbidden |
|---|---|---|
| Yield to overlapping following compound | 152 | 7 |
| Surface | 81 | 1 |
| Single token | 48 | 0 |
| Sentence end | 46 | 4 |
| て-form stem (start) | 46 | 11 |
| Irrealis | 31 | 7 |
| Dictionary form (start) | 23 | 70 |
| Godan potential | 20 | 16 |
| Sentence start | 19 | 19 |
| Masu stem | 16 | 5 |
| Godan imperative | 5 | 11 |
| Past tense stem | 4 | 2 |
| Ichidan | 2 | 21 |
| Ichidan imperative | 2 | 2 |
| Dictionary form (head) | 2 | 0 |
| Adverb (head) | 1 | 3 |
| て-form stem (head) | 0 | 4 |
| Godan, Godan imperative (head) | 0 | 0 |

| Flag | Vocab |
|---|---|
| Inflecting word | 149 |
| Question overrides form | 139 |
| Compositionally transparent compound | 51 |
| Poison word | 43 |

| Configurable rule | Vocab | Texts in all |
|---|---|---|
| Required prefix | 50 | 77 (ん in 37 of them; おう has 15 texts) |
| Prefix is not | 35 | 36 |
| Surface is not | 23 | 31 |
| Suffix is not | 21 | 25 |
| Yield to surface | 20 | 21 |

**The most common combinations** are:
- question overrides form with a required prefix (13 vocab, plus 10 with inflecting word too and 10 with て-form stem
  (start) required too);
- dictionary form (start) required with question overrides form (13);
- godan potential required, sentence start forbidden, inflecting word and question overrides form (10);
- surface required with yield to overlapping following compound (9);
- irrealis required with ichidan forbidden (8).

OR was never expressible, except within a configurable rule's set, so the data can't show where it's wanted.

## Where parsing fails today: the manual corrections

Every manual correction in a sentence's configuration marks a sentence that the rules got wrong, and says what was
right. That makes them labeled data, both for finding what the rules lack and for measuring a change.

- **Incorrect matches:** 442, in 408 sentences, on 189 distinct vocab. The most corrected are す (40), ほんの (28), うる
  (21), 鬱 (18), 降って (18), 様です (15) and 役に立てる (12).
- **Hidden matches:** 116, in 109 sentences, on 104 distinct vocab.
- **Highlighted words:** 23,383, in 19,773 sentences. A highlight also makes its match valid whatever else fails
  (`Match.IsValid`), so some highlights are corrections. Most probably mark the word the sentence was added for. Which
  is which isn't known yet.

Some cases, analyzed without their sentence's configuration (`analyze_sentence`, 2026-11-10):

| Sentence | Matched wrongly | What tells it apart | What a rule would need |
|---|---|---|---|
| この本の作家は有名な学者です。 | ほんの, by its form 本の, which the tokenizer reads as the noun 本 and the particle の | ほんの written 本の comes before an amount, as in 本の少し | The form that matched; the token after |
| 夏目様ですよね | ようです, by its form 様です | The tokenizer tags this 様 as a suffix after a person's name (名詞・接尾・人名) | The part of speech of the word's first token, or of the token before |
| 良い友達がいて私は幸せだ。 | 友達がい, "true friendship" | Its last token is the verb いる in its continuative form, which continues into て | The part of speech or inflected form of the word's last token |
| あの女の子、一人でタブレットを五台もかったんだって。 | かっ, the prefix "very" | The tokenizer reads this かっ as a verb (かる) in its ta-connection form, which continues into た | The same |
| うるせぇよ | うる, the suffix "be able to", at the sentence's start | A suffix needs a word before it | The vocab's own parts of speech, as a rule for every suffix vocab |
| 藤宮さん早退した時　雨降ってたし; 信奉すべきだ | Neither, any more | 降って is a poison word now, and す is shadowed by すべき | Nothing: these corrections are stale |

What these show:
- **Today's rules can't express the first four.** None can test a token's part of speech or inflected form, or the form
  that matched. And none can require a text after the word: only forbid one.
- **友達がい and かっ fail the same way:** the word ends partway through a verb. Many vocab can fail like that, so a
  single rule-for-many-vocab could fix them all, where per-vocab rules would fix one vocab at a time. うる is the same
  kind of case: a rule about suffixes, not about one vocab.
- **Some corrections are stale.** The only way to tell which is to parse each sentence without its correction.

## A condition, built from ingredients (thinking with Magnus, 2026-11-10)

Magnus's draft, in short. There are many boolean tests, and several things they can be tested on. Most of today's tests
are about one token: the first or last in the word, or the one before or after it. Other things could be tested too:
- the matched text as a whole;
- the whole text before the word, or after it.

Each rule combines these dimensions. "Token at position x" says the condition is about a token. A test is then picked
from those that apply to tokens. Last comes the choice of the test as it is, or negated. Today's rules, which combine
such things in code, would each be replaced by one built like this. Those conditions then combine into a tree like the
search's.

So a single condition is a condition-subject and a condition-test. Negation, all of and any of come from the tree.

### Condition-subjects

| Condition-subject | What it is | Today |
|---|---|---|
| A token at a position | The token before the word, the word's first token, its last token, the token after. Perhaps the second before or after, or any or every token of the word | The token before and the first token. The last token only as "ends a statement", which looks at the token after. Otherwise the token after only by its surface |
| A text | The word's surface; its base form; the whole text before the word, back to the sentence's start; the whole text after, to its end; the whole sentence | The surface of the token before and the token after, and the word's surface |
| The match | Its number of tokens; whether it's matched by its surface or its base form; which of the vocab's forms matched | Single token and surface. Not the form |
| Other matches | The valid or displayed matches before, after, or overlapping the word, and their vocab | Only in the display rules and the universal-requirements: shadowing, priority, yielding |
| The vocab itself | Its parts of speech, its forms, its flags | Only as defaults in code, such as a complete な-adjective yielding its last token |

### Condition-tests, by what the condition-subject has

- **A token** has:
  - its part of speech, in IPADIC's four levels, as the tokenizer, MeCab, reports it: 13 at the top (noun, verb, i-adjective,
    adverb, particle, bound auxiliary, conjunction, interjection, prefix, symbol, …), down to such as noun › suffix ›
    person's name;
  - its inflection type, about 70, in families: godan (by row), ichidan, suru, kuru, i-adjective, special (特殊・タ,
    特殊・ダ, 特殊・マス, 特殊・ナイ, …), classical;
  - its inflected form, about 30, in families: dictionary form, irrealis, continuative, ta-connection, te-connection,
    conditional, imperative, classical dictionary form, …;
  - its surface, base form and reading, as texts;
  - JAStudio's own facts about it: masu stem, has a て-form stem, the stem or the inflection of a dictionary-form verb,
    of a godan potential, of a godan or ichidan imperative; past-tense marker; ends a statement; non-word character;
  - whether it's there at all: the sentence's first token has no token before it.
- **A text** can: be one of some texts, start with one of them, end with one of them, contain one, match a regular
  expression, or be empty.
- **The match** has its number of tokens, the variant that matched (surface or base) and the form that matched.
- **The vocab** has its parts of speech and its flags.

### Today's rules, built from these ingredients

| Today's rule | Built from ingredients |
|---|---|
| Godan, ichidan | The token before: its inflection type is godan (する excluded), or ichidan |
| Irrealis | The token before: its inflected form is an irrealis form |
| Masu stem | The token before: it's a verb, and its inflected form is continuative |
| Adverb (head) | Any of: the token before is an adverb; all of: it's an i-adjective, and its surface ends with く |
| て-form stem, dictionary form, godan potential, godan and ichidan imperative, in their head and start variants | The token before, or the first token: JAStudio's fact about it |
| Past tense stem | Any of: the token before's inflected form is ta-connection; the first token's inflection type is 特殊・タ |
| Single token | The match: one token |
| Surface | The match: matched by its surface (required), or by an inflected surface (forbidden) |
| Sentence start | The text before is empty, or ends with a sentence-start character. Today it tests the token before's surface |
| Sentence end | Any of: there's no token after; the token after is a sentence-ending particle, a symbol, or a sentence-end character |
| Prefix is not, required prefix | The token before: its surface ends with one of the texts |
| Suffix is not | The token after: its surface starts with one of the texts. There's no "required suffix" today |
| Surface is not | The word's surface is one of the texts |

Every one of today's rules can be built from the ingredients. Three of them need a small tree: adverb (head), past tense
stem and sentence end. Those would become shared-conditions under today's names, so they stay one pick each.

### How the ingredients can and can't combine

- **The condition-subject decides the condition-tests,** as a note property decides its conditions in the search. A
  token has a part of speech; a text doesn't.
- **A condition-subject can be absent.** At the sentence's start there's no token before. A test on an absent subject
  is false, so "not: the token before is a godan verb" holds at the start. Whether it's there is a test of its own.
  *Superseded (2026-11-12): a missing token is the empty token, on which every condition is evaluated as written. See
  [A missing token: the empty token](#a-missing-token-the-empty-token-agreed-with-magnus-2026-11-12).*
- **Token or text.** "The token before ends with ん" and "the text before ends with ん" mostly agree. But a test on the
  text doesn't depend on where the tokenizer splits, so it's the sturdier of the two wherever it says enough.
- **When a condition can be tested is the hardest constraint.**
  - Parsing runs in stages: tokens, then which matches are valid, then which valid ones display
    (`CandidateWord.RunValidityAnalysis`, then the display passes).
  - Tokens, texts, the match, the vocab and the dictionary are known before any validity is decided. Other matches'
    validity isn't: a condition on whether a word matches can't depend on a match whose own validity depends on it.
  - Priority already works around this, by looking only at the other matches' primary validity.
  - So a condition on validity can use the static condition-subjects, and perhaps other matches' primary validity. A
    condition on display can also use the other matches' validity and display.
  - The editor would offer only the condition-subjects that a tree's stage allows.

### Where the leverage is: rules for many vocab

1. **One vocab's match-condition** fixes that vocab's sentences.
2. **Shared-conditions** are defined once and used by many vocab, such as "sentence end" or "after a person's name".
   Improving one improves every vocab that uses it. Today's rules made of several facts are hard-coded ones.
3. **Rules-for-many-vocab** apply a condition to every vocab of a kind, chosen by its parts of speech or its flags.
   Today's universal-requirements and part-of-speech defaults are hard-coded ones. The cases above suggest two:
   - **A vocab that isn't a verb or an adjective doesn't match a word that ends partway through one,** on a token in a
     connective form that continues into its inflection. This is the 友達がい and かっ case.
   - **A suffix vocab needs a word before it.** This is the うる case.

   A vocab must be able to opt out of such a rule, as requiring dictionary form (start) lifts that default today. And
   a rule-for-many-vocab changes many sentences at once. The full rebuild's diff, the parsing cases, and the yardstick
   below are what make that safe.

2 and 3 make the work on vocab cheaper, and spread each improvement to every vocab it fits. The goal they serve is
sentences that parse right without corrections on the sentence.

### The form that matched

Some vocab are wrong only by one of their forms: ほんの by 本の, ようです by 様です. A condition on the form that matched
lets the rest of a condition apply to that form only, such as: any of (not: the form matched is 本の; the token after is
an amount). The other way is for each form to have a match-condition of its own. *For now: a form that needs different
matching gets a vocab of its own (see [Settled with Magnus](#settled-with-magnus-2026-11-10)). In Magnus's words: "That is my
gut instinct and how I've done it so far, not gospel."*

### Priority, when conditions can say more

Today, owned forms decide first, then the weights. With richer conditions, competing vocab can exclude each other
instead: ようです excludes "after a person's name", さまです requires it. That leaves fewer ties for weights to break.
What decides priority is open: weights computed from the tree, something explicit, or the conditions alone with ties
treated as a problem to fix.

### Display

Whether a valid match displays can use the same language: today's yield rules and the breakdown's hiding options would
be conditions on display. Its condition-subjects include the other displayed matches, so it's the last stage. Whether
it's a second tree per vocab is open.

## The yardstick: measuring what the rules achieve

A tool would parse every corrected sentence without its corrections, and report for each correction whether it still
changes the result. It would be part of JAStudio's MCP server, so mine to build.
- The stale corrections show up, such as 降って's and す's.
- For any change to the rules, the corrections it makes unnecessary can be counted. The full rebuild's diff shows the
  other sentences it changes.
- Further: for a vocab corrected in many sentences, the tool could compare the tokens around its incorrect matches with
  those around its correct ones. It would then list the ingredients that tell them apart, which suggests the rule.

**The highlighted words as test samples (Magnus, 2026-11-11).** Most highlighted words aren't corrections: they're the word
a course sentence is for, one in every Core 2000 sentence and one to three per WaniKani vocab. So for tens of thousands of
sentences we know a word that must be in the parse. Parse each without its highlight, and the share that still finds the
word measures the goal directly: sentences that parse right with no correction on the sentence.
- **It's the largest truth we have.** The corrections number some 560 matches marked incorrect or hidden. The
  highlighted words cover tens of thousands of sentences, and were chosen for the course, not for the parser's mistakes.
- **It tests one word of each sentence,** not the whole parse. A rule that makes the highlighted word parse can still
  break another word of the sentence; the full rebuild's diff shows that.
- **A sentence whose highlighted word doesn't parse without its highlight** is a case to work on, and the ones that fail
  for the same vocab, or the same pattern, suggest the rule.

**The generality measure (thinking with Magnus, 2026-11-11).** For each condition in each vocab's match-condition, count
the matches it decides across the corpus: those it alone rejects, which would be valid without it. Counting every
rejection would credit conditions that only repeat what another rejects. What it tells:
- **A condition that decides many matches is a rule about the language.** Ones that recur across vocab are candidates
  for shared-conditions.
- **A condition that decides only the sentence it was written for is a patch.** It may be worth redoing as a rule.
- **Or it shouldn't exist at all:** a correction on that sentence, marked incorrect or highlighted there, does the same
  with no side effects. A condition is tested on every match of its vocab, and can wrongly reject a correct match in a
  sentence nobody has seen yet. Moving one is checked mechanically: make the corrections, delete the condition, and the
  full rebuild must change nothing.
- **The data can't settle every case:** a literal-text condition that decides one sentence today may protect future
  ones. The tool ranks the candidates; Magnus decides.
- **The reverse:** clusters of similar corrections on one vocab are candidates for a condition, as the comparison
  above suggests.
- **Judgment, not only numbers (Magnus, 2026-11-11).** The counts are evidence, not the verdict. Knowledge of Japanese can
  tell that a rule is worth making though few of our sentences need it yet, because many sentences out there will. And the
  opposite: a condition can fit a large share of our sentences only because they're irregular in the same way, shaped
  by the same work. In his words: "Apply judgment to all of the above, not pure numbers."

So the corrections, the vocab's conditions and the rules for many vocab each hold the knowledge that belongs at their
level. Every measure here is only as good as the sentences it runs over, so a large and diverse sentence corpus matters
as much as the conditions: see [Expanding the sentence corpus](todo/expanding-the-sentence-corpus.md).

## Building a condition in the editor

Magnus's flow:
1. Pick the condition-subject: the token before, the first token, the last token, the token after, the text before,
   the text after, the word, the form that matched, ….
2. Pick a condition-test from those that apply to it, such as "part of speech is", "inflected form is", "surface
   starts with one of" or "is a masu stem".
3. Give its value.
4. Choose whether it's as it is or negated.

It fits the search editor's add menu with single keys: the condition-subject, the condition-test, then a dialog for the
value. A part of speech would be picked from its four-level tree by typing, as a tag is. A shared-condition would be
picked by name.

## Sharing code with the note search: to find out

> Tried in increment 4: see [Increment 4, as built](#increment-4-as-built).

Only if it really fits (Magnus).

**What's alike:**
- the all-of, any-of and not tree, and positions in it;
- editing with undo;
- the tree editor.

**What differs:**
- **The leaves:** a condition-subject with a condition-test, against a property with a comparison.
- **What they're tested against:** a word in its sentence, tested in stages, with absent condition-subjects, and millions
  of times in a full rebuild.
- **Failure reasons:** a search never says why a note didn't match.
- **Shared-conditions,** which refer to other trees.
- **Priority.**
- **Storage:** in the corpus, not in the workspace.

That's to be decided when building, by trying it.

## Settled with Magnus (2026-11-10)

- **Everything about a token is testable:** its part of speech at each of its four levels, its inflection type, its
  inflected form, its surface, base form and reading, and JAStudio's own facts about it. **The conditions on a single
  token are a tree themselves** (Magnus). See [The shape of the tree](#the-shape-of-the-tree).
- **Shared-conditions are named, saved subtrees,** usable in any other tree (Magnus).
- **Both the text before and after and the tokens before and after can be tested,** and each vocab uses what works
  best for it (Magnus).
- **A form that needs different matching gets a vocab of its own,** cloned from the vocab (Magnus's gut, with a grain
  of salt). The word's surface is a condition-subject anyway, so a small difference between forms can still be a
  condition, such as "any of: not: the word's surface is 本の; the token after is an amount".
- **The highlighted words are mostly the word a sentence is for,** not corrections: Core 2000 features a word in every
  sentence, and WaniKani has one to three sentences per vocab that do the same (Magnus).
- **That makes them test samples (Magnus, 2026-11-11):** "For all of these wani and core sentences tens of thousands of
  them, we know that they should parse the same even if the highlighted word is un-highlighted." See
  [the yardstick](#the-yardstick-measuring-what-the-rules-achieve).
- **Condition-subjects:** start with the ones known, and add others when they're missed. The design must make adding
  one cheap.
- **The yardstick comes after the core,** not before (Magnus).

## The shape of the tree

**A condition-subject is a node of the tree, and the conditions under it are about that subject.** Under "the token
before", the add menu offers what a token has: part of speech, inflection type, inflected form, its texts, JAStudio's
facts about it. Under "the text after" it offers the text tests. Adverb (head), for example:

```
The token before:
   Any of:
      Part of speech is adverb › general
      All of:
         Part of speech is i-adjective
         Surface ends with く
```

The inner tree isn't only tidier. Two things need it:
- **An absent token.** At the sentence's start, "the token before: not a godan verb" is false, since there's no token
  to test. "Not: the token before: a godan verb" is true. *Superseded (2026-11-12): a missing token is the empty token, on which every condition is evaluated as written. See
  [A missing token: the empty token](#a-missing-token-the-empty-token-agreed-with-magnus-2026-11-12).*
- **The word's tokens taken together.** "Any token of the word: all of (a verb; in a connective form)" isn't "any token
  is a verb, and any token is in a connective form".

**Today's rules translate one to one or two nodes each.** JAStudio's facts about a token are tests of their own,
computed by the same code as now, so "godan" becomes "the token before: a godan verb". That keeps the migration exact.
Building them from the raw fields instead, such as the inflection type, would change results: on a token split by
JAStudio's preprocessing, the facts don't come from the raw fields. Past tense stem becomes an "any of" over two
condition-subjects. Sentence start becomes a condition on the text before.

**So the migration doesn't need shared-conditions.** I'd still design them in from the start, and build them right
after the migration, before any vocab gets new conditions.

**Shared-conditions have the level they're defined at.** "Adverb-like" is about a token, so it's usable under any token
condition-subject, before the word or first in it. "Sentence end" is about the word in its sentence, so it's usable
wherever such conditions are. A shared-condition can't use itself, through others or directly.

## Rules for many vocab: one way to do it

A rule-for-many-vocab could be three things:
- **Which vocab it applies to: a note search.** Finding notes by their properties is exactly what the search does, so
  here sharing it fits. The search needs a parts-of-speech property, which it was always going to get.
- **The match-condition** those vocab must meet.
- **Exemptions:** a vocab can opt out of a rule. Its editor would show every rule that applies to it, each with the
  opt-out, so these rules are visible, as today's hard-coded ones aren't.

A vocab's matches must meet its own match-condition and every rule that applies to it. Today's universal-requirements
and part-of-speech defaults would become such rules. "Dictionary form (start), forbidden by default" is one: a rule for
every vocab, which the 23 that require it opt out of. Until such rules exist, those vocab keep a flag in its place (open
question 1 below).

## The parsing engine, and what it means for the design (studied 2026-11-10)

### How a sentence is parsed

1. **Tokenizing (`JNTokenizer`).** MeCab, with the IPADIC dictionary, gives each token:
   - its surface, base form and reading;
   - its part of speech at four levels;
   - its inflection type and inflected form.

   JAStudio corrects the tokenizer twice: a quotative だって is split into だ and って (`MisTokenizedQuotativeDatte`), and
   an ichidan verb the dictionary lacks is fixed (`MisTokenizedIchidanVerbUnknownToTheTokenizer`). A `<wbr>` hint
   becomes a splitter that forces a token boundary.
2. **Preprocessing (`PreProcessingStage`)** splits tokens into JAStudio's own pieces, in an order that matters:
   - godan imperatives;
   - godan potentials and imperatives hidden in what the tokenizer took for an ichidan verb;
   - ichidan imperatives;
   - every dictionary-form verb, into its stem and its ending. A godan verb's ending gets the base form う.
3. **Candidate words (`TextAnalysisLocation`).** Every token starts candidate words of 1 to 12 tokens. Each candidate has
   up to two variants:
   - its surface, the tokens' surfaces joined;
   - its base form, the same with the last token's base form, when that differs.
4. **Matches (`CandidateWordVariant`).**
   - **Vocab are found by exact form:** the vocab whose forms include the variant's text (`VocabCollection.WithForm`).
   - **Each vocab match is tested:** the universal-requirements first, then the vocab's own rules, then priority.
   - **If any vocab owns the form,** or if vocab have it but the dictionary doesn't and the word is a compound, the vocab
     decide alone.
   - **Otherwise,** with no valid vocab match, the dictionary's match counts. A single token with neither gets a missing
     match.
   - **Validity is decided in order:** every single-token word first, then the compounds. Within a word, the base variant
     before the surface.
5. **Display.** Three passes: display without shadowing, then shadowing, then shadowing again until nothing changes
   (`TextAnalysis`). A location's longest displayed word shadows the locations it covers. A word that yields its last
   token gives way to an overlapping compound that follows it.
6. **What's stored (`ParsingResult`):** every valid match, with whether it's displayed when no breakdown option is set.
   That links vocab to sentences, and gives a vocab its sentence count. The sentence breakdown analyzes the sentence
   again, under the options.
7. **After a vocab is edited,** the sentences that contain one of its forms or stems are reparsed, along with the
   sentences it has now (`LocalNoteUpdater.ReparseSentencesForVocab`). A full rebuild takes about 2 seconds, since the
   parsing speed work of 2026-11-12 ([parsing-speed.md](parsing-speed.md)).

### What it means

- **Conditions can only take matches away, never add them.** A vocab can match only a text that is one of its forms.
  So a condition can only reject a word that the form lookup found. Making a word match takes a form, or a highlight.
- **Taking a vocab's match away can let the dictionary match instead,** unless a vocab owns the form. That's what a
  poison word is for: it owns its form and never matches, so the dictionary doesn't match the text either. It means
  "this text isn't a word here". In the tree, that's a condition that always fails. So the model needs an explicit
  "never". In the search, an empty group is ignored, which is the opposite of a "never".
- **Two kinds of token, and only one has the tokenizer's facts.**
  - Only a tokenizer token (`JNToken`) has a part of speech, an inflection type and an inflected form.
  - A piece JAStudio split off (`SplitTokenBase`) has only a surface, a base form, and JAStudio's facts, which are
    yes-or-no. Its other facts come from the token it was split from (`IAnalysisToken.SourceToken`).
  - Every dictionary-form verb is split, so "the token before" a word that follows one is the verb's ending, such as る.
  - So a test of a split piece's part of speech needs a definition: the token it came from, or not allowed. Open
    question 4.
- **Tokenizing depends on the vocab.** The hybrid splitter looks up vocab, and the dictionary, by question, by form,
  by parts of speech and by compound parts. So editing a vocab can change how other sentences are tokenized.
- **Conditions can't fix tokenizing.** They run after preprocessing and can't change its tokens. A misparse that starts
  in the tokenizer, such as うるせぇ coming out as う, る, せ and ぇよ, can only be patched by rejecting matches. Rules
  for splitting tokens could one day be configurable too, but they're another layer.
- **Validity can't depend on the breakdown's options.** The stored parsing result is made with no options, and the
  breakdown with them. They must agree on what matches, so only conditions on display may read the options, as
  hiding compositionally transparent compounds does.
- **The order constrains what a condition can read:**
  - a single-token word is decided before any compound, so a compound's conditions could read whether its parts match
    on their own;
  - the base variant is decided before the surface;
  - other vocab of the same text count only through their primary validity, as priority does.
- **Display is decided by repeating until nothing changes.** A display condition that reads what's displayed could
  keep changing, and never settle. The code already notes that the yield check reads the displayed words it helps
  decide. Conditions on display need a rule that guarantees they settle.
- **Priority is only between vocab of the same text.** It compares the vocab matches of the same variant, the same span
  of the same text. Overlaps between different spans are settled by display: the longest shadows, and yielding. So
  conditions that tell two vocab of the same form apart, such as ようです and さまです, settle priority without weights.
- **The universal-requirements apply to every match: vocab, dictionary and missing** (`Match.MatchPrimaryValidityRequirements`).
  A rule-for-many-vocab doesn't reach dictionary matches. Moving these requirements out of code needs rules for every
  match, the dictionary's included.
- **Dictionary form (start) is one concept with two others.** The forms' placeholder う expands to every dictionary-form
  ending. And a godan verb's split ending has the base form う. A vocab such as う限り matches only because of all three.
  That supports making it a fact about the vocab (open question 1).
- **Reparsing after an edit.** A vocab's edit reparses the sentences with its forms. A shared-condition or a
  rule-for-many-vocab changes many vocab at once, so its edit needs a full rebuild, which takes about 2 seconds.
- **The parsing cases need to carry them too.** The cases run against a stripped-down corpus of the vocab their
  sentences look up (`LookupRecording`). It must carry the shared-conditions and rules-for-many-vocab too.
- **Caching:** a match caches its validity, and only the yield check is computed afresh each time
  (`CustomForbidsNoCache`). A condition on validity must therefore depend only on what doesn't change during the
  analysis.
- **The cost:**
  - a full rebuild takes about 2 seconds, for 49,162 sentences ([parsing-speed.md](parsing-speed.md));
  - a vocab's rules are tested only on variants whose text is one of its forms;
  - the dictionary lookups dominate the time.

  So testing trees should add little.
- **Python doesn't touch the matching configuration,** so the redesign stays in .NET.

## The core: the design to build first

> Written 2026-11-10, before the study of the parsing engine above. What that study changes is in its "What it means".
> The names are placeholders.

The core is the match-condition and everything it takes to replace today's require/forbid rules and configurable rules
with it, with nothing changing in the parsing. Shared-conditions are designed in, and built right after.

### The model, in Core

**Three levels of condition, one tree structure.** A condition is tested against one of three things, and each has its
own leaves:

| Level | Tested against | Its leaves |
|---|---|---|
| The word | The candidate word as a vocab's match: its tokens, its variant, its place in the sentence | The condition-subject nodes below; "is a single token"; "is matched by its surface"; "is matched by an inflected surface" |
| A token | One token (`IAnalysisToken`) | Part of speech is (at any of the four levels); inflection type is one of; inflected form is one of; surface, base form or reading (a text condition); each of JAStudio's facts about a token, from `IAnalysisToken` (godan verb, ichidan verb, irrealis, masu stem, adverb, has a て-form stem, the dictionary-form verb's stem or inflection, the godan potential's, the godan and ichidan imperative's, past-tense stem, past-tense marker, ends a statement, non-word character, inflectable word) |
| A text | One string | Is one of; starts with one of; ends with one of; contains; matches a regular expression; is empty |

**All of, any of and not exist at every level,** so the tree structure is generic over what's tested. That much
sharing is needed within matching itself, whatever comes of sharing with the note search.

**The condition-subject nodes connect the levels.** Each is a condition on the word that holds a condition on its
subject:
- **Tokens:** the token before the word, the word's first token, the word's last token, the token after the word.
  Absent, as before the sentence's first token, the node is false. "Any token of the word" and "every token of the word"
  come when a condition needs them.
- **Texts:** the text before the word, back to the sentence's start; the text after it, to the end; the word's surface;
  its base form.

**Shared-conditions** (built after the migration) are a leaf at their own level that refers to the subtree by name. A
name is checked when the tree is read, and a shared-condition can't use itself, directly or through others.

**What else the model has:**
- **Equality:** conditions are values, so specs can compare a migrated tree with one written out.
- **Descriptions:** each condition has its readable lines, indented like the tree, for the editor, the failure reasons
  and a failing spec.
- **JSON,** for storage.

### Testing a match

- **A vocab's match-condition becomes one requirement** among `VocabMatch`'s, replacing the 22 that read the vocab's
  require/forbid rules and configurable rules. The universal-requirements stay as they are.
- **Failure reasons:** when it fails, each failing top-level condition gives its readable line as a failure reason, such
  as "Not: the token before: a godan verb".
- **The match's inspector provides the condition-subjects:** the token before (`MatchInspector.PreviousLocation`), the
  first and last tokens (`StartLocation`, `EndLocation`), the token after (`NextLocation`) and the texts.
- **Absent tokens:** the token before the sentence's first token, or after its last, is absent, and conditions on it are
  false. *Superseded (2026-11-12): a missing token is the empty token, on which every condition is evaluated as written. See
  [A missing token: the empty token](#a-missing-token-the-empty-token-agreed-with-magnus-2026-11-12).*

### What's stored

> As built, the format differs a little: see [Increment 3, as built](#increment-3-as-built).

A vocab's `matching` becomes:

```json
"matching": {
  "condition": { "allOf": [
    { "precedingToken": { "fact": "irrealis" } },
    { "not": { "precedingToken": { "fact": "ichidanVerb" } } },
    { "precedingToken": { "surface": { "endsWithOneOf": ["ん"] } } }
  ] },
  "flags": ["questionOverridesForm"],
  "yieldLastTokenToOverlappingCompound": null,
  "yieldToSurface": []
}
```

- **`condition`** is null for a vocab without one, as 27,203 would be.
- **Each kind of condition is an object** whose key says its kind, as a search's JSON does (`NoteConditionJson`).
- **The flags and the display rules stay as they are.** They aren't conditions on whether a word matches.
- **Shared-conditions** will be corpus data too, stored beside the notes, one file each.

### The migration

> As built: see [Increment 3, as built](#increment-3-as-built).

Every vocab's require/forbid rules and configurable rules become its match-condition: an "all of" with one or two nodes
for each rule that's set.
- **Required becomes the node, and forbidden becomes "not" of it.**
- **The exceptions:**
  - forbidden surface becomes "not: matched by an inflected surface";
  - past tense stem becomes "any of" over two condition-subjects;
  - sentence start becomes a condition on the text before;
  - sentence end becomes the word's last token ending a statement;
  - dictionary form (start) is open question 1.

The migration is a one-time script, as earlier corpus migrations were. It's followed by saving every note, so JAStudio
writes each file in its own format.

### The checks

- **Side by side, before the storage changes.** For a while JAStudio builds the match-condition in memory from today's
  stored rules, and tests both on every candidate match of a full rebuild. Temporary code lists every disagreement, with
  the rule it came from. That points at a wrong translation directly, where a diff of the corpus shows only that some
  sentence changed. The code goes once the list is empty.
- **A full rebuild after the migration changes nothing,** and the parsing cases pass unchanged.
- **Specs for the model:** each kind of condition, absent tokens, the JSON both ways, the descriptions.
- **The full rebuild's time,** before and after.

### What else reads today's rules

- **Priority (`CustomRequirementsWeight`)** sums each rule's weight. Open question 2.
- **`VocabNoteForms.ExpandsDictionaryFormEndingPlaceholders`** reads dictionary form (start). It goes with open question 1.
- **`SentenceCollection.PotentiallyMatchingVocab`** reads "surface required" to narrow the sentences a vocab's reparse
  looks at. I'd drop that: it's only a shortcut, and dropping it makes a reparse look at more sentences. I'd measure the
  cost first.
- **The tags for the configurable rules in use** (`LocalNoteUpdater.TagVocabMetadata`) go.
- **The Matching tab** and **the MCP server's editable fields** (`VocabEditableFields`) edit today's rules.

### The Matching tab, in between

> Done in increment 3, and the read-only tree replaced by the tree editor in increment 4.

The migration can ship before the tree editor exists:
- **A checkbox view of the tree.** While a vocab's match-condition has today's shape, an "all of" of the nodes today's
  rules translate to, the tab shows it as today's checkboxes and edits it the same way.
- **The tree, read-only.** A vocab whose match-condition has any other shape shows its tree read-only, until the tree
  editor exists.

The MCP server gets a tool to set a vocab's match-condition as JSON.

### Increments

1. **The model:** the three levels, the condition-subject nodes, JSON and descriptions, with specs. Nothing uses it yet.
   *Built 2026-11-10, waiting for Magnus's pass over its names and design.* See
   [Increment 1, as built](#increment-1-as-built).
2. **Testing matches with it, from today's storage:** built 2026-11-10, see
   [Increment 2, as built](#increment-2-as-built).
   - the match-condition built in memory from today's rules;
   - `VocabMatch` testing it;
   - the side-by-side check, until it's clean.
3. **The migration:** built 2026-11-11, see [Increment 3, as built](#increment-3-as-built).
   - the storage changed, and the corpus migrated;
   - the Matching tab's checkbox view;
   - the MCP tool;
   - today's rule types, and the state tests that read them, deleted.

   The full rebuild must change nothing.
4. **The tree editor** on the Matching tab: built 2026-11-11, see [Increment 4, as built](#increment-4-as-built).
5. **Conditions shown as code,** with the tokenizer's values in English too, and a missing token as the empty token:
   see [How conditions display: as code](#how-conditions-display-as-code-agreed-with-magnus-2026-11-12). Built
   2026-11-12, see [Increment 5, as built](#increment-5-as-built).
6. **Shared-conditions, and the rule editors go** (agreed with Magnus, 2026-11-12). The five rules that are small trees
   become shared-conditions under their names: past tense stem, godan potential, godan imperative, ichidan imperative
   and sentence start. The other 16 are one line each already, such as `PrecedingToken.IsAMasuStem`. Then the Matching
   tab's rule editors go, with the rule-by-rule editing (`MatchConditionAsRules`) and the MCP server's
   `matching.rules.*` fields. They have two reasons to stay until then:
   - **A rule is one click,** where its condition built in the tree takes several steps, and sentence start means typing
     the characters that start a sentence. A shared-condition is one pick in the add menu.
   - **A condition weighs only when it's exactly a rule's,** and the comparison depends on order (`ConditionGroup`'s
     equality). Godan potential built by hand with its two facts the other way round matches the same words and weighs
     nothing, which can change which of two vocab wins, silently. A shared-condition's weight can belong to its name.

   What still reads the rules then reads the shared-conditions: the priority weights, the four tags below
   `Tags.Vocab.Matching.Uses`, and narrowing a reparse when the surface is required. (Revised in step 2: a
   shared-condition weighs what its definition weighs, so the rules stay the weights until priority is designed.)

   In four steps, each committed with its full rebuild (agreed with Magnus, 2026-11-12, with the five as corpus data, not
   code):
   1. **The model and its storage, their display, and the five in the corpus:** built 2026-11-12, see
      [Increment 6, step 1, as built](#increment-6-step-1-as-built).
   2. **The vocab using the five by name,** built 2026-11-12, see
      [Increment 6, step 2, as built](#increment-6-step-2-as-built). In place of their conditions written out, through an operation that stays:
      using a shared-condition wherever its definition is (revised with Magnus, 2026-11-12, see
      [Step 2: naming a condition never changes what a vocab does](#step-2-naming-a-condition-never-changes-what-a-vocab-does)).
      A change to the corpus's format, so Anki is closed for the fast-forward.
      - **Then, finding the conditions vocab share:** an MCP tool that lists the subtrees, and the combinations of
        conditions in a group, that several vocab hold, for a mind to name and decide on.
   3. **Creating and editing shared-conditions in JAStudio,** and opening one's definition from a vocab's tree (F12 or
      Ctrl+click).
   4. **The rule editors go,** with the rule-by-rule editing and the MCP server's `matching.rules.*` fields.

#### Step 2: naming a condition never changes what a vocab does

Agreed with Magnus, 2026-11-12, after he asked whether replacing the five's conditions by their names would be done by
hand or by JAStudio, and whether it would stay: "when we find that other subtrees are used often we should created
shared-conditions for them, and if those can be auto-inserted in the relevant vocab that would be very nice. Also, since
conditions are values, that should mean that we could mine existing matching configs in the future to find common
subtrees, right? And maybe created shared conditions straight from that?"

**The rule that makes using one everywhere safe: giving a condition a name never changes what a vocab does,** neither
which words it matches nor what it weighs. What it matches can't change, since a shared-condition is met exactly when its
definition is; a full rebuild checks it. Two things broke the rule, and step 2 fixes both:
1. **A group's conditions were compared in order** (`ConditionGroup<TSubject>.Equals`): godan potential with its two facts
   the other way round wasn't the rule's condition, so it weighed nothing, and wouldn't be found to be a shared-condition's
   definition. So conditions are also compared by what they mean: in any order within a group, and with a
   shared-condition taken as its definition. Not as their equality, which the editor relies on: it ignores an edit equal to
   the tree it has (`EditableMatchCondition`), and reordering is a change.
2. **Only a condition that is exactly a rule's weighed,** so a rule's condition replaced by its name lost its weight. The
   plan was to store a weight in each shared-condition's file; instead a shared-condition weighs what its definition
   weighs. No file holds a weight, and a forbidden one weighs the 1 a forbidden rule does. And since a combination of the
   match-condition's conditions can become one shared-condition, weighing takes an "all of" in the match-condition's "all
   of", or a shared-condition defined as one, as the conditions it holds.

**Using a shared-condition wherever its definition is** replaces, in every vocab's match-condition and every other
shared-condition's definition, each condition that means the same as the definition with the shared-condition, by name.
When the definition is a group of two or more conditions, a group of the same kind that holds them all among others
has them replaced by it: `AllOf(A, B, C)` with a definition `AllOf(A, B)` becomes `AllOf(Shared, C)`.

**Making a shared-condition takes a mind; using it everywhere doesn't.** Finding the conditions vocab share proposes,
and someone names each and decides: a shared-condition is mostly its name, and two vocab holding the same condition may
hold it for different reasons, which one shared-condition then ties together. Once decided, using it everywhere is
mechanical. For the five, tying them together is right by construction: each was one rule.

**Counted when deciding, 2026-11-12:** 421 of the 28,004 vocab have a match-condition, 345 distinct subtrees in all.
Sentence start is in 38 vocab (19 forbid it), godan potential in 36 (16), godan imperative in 16 (11), past tense stem in
7 and ichidan imperative in 5. `PrecedingToken.Surface.EndsWithOneOf("ん")` is in 44, more than any of the five. A
combination: following an irrealis form and not an ichidan verb is the whole match-condition of 14 vocab.

After the core: the yardstick, rules-for-many-vocab, conditions on display, the tree checked against a word in a
sentence (decided: see [The tree checked against a word](#decided-on-the-backlog-the-tree-checked-against-a-word-in-a-sentence)),
and vocab given conditions that use the new power.

### Increment 1, as built

The code is in `JAStudio.Core/Note/Vocabulary/Matching/Conditions/`, with specs in
`JAStudio.Core.Specifications/AICreatedSpecs/Matching/`. The specs test the conditions on words of real sentences, as
JAStudio's analysis finds them. The names are mine until Magnus's pass at the end of the work (see
[Naming and design questions: `review:matching:` comments](#naming-and-design-questions-reviewmatching-comments)).

**The model:**
- **One tree for three subjects.** `Condition<TSubject>` is a condition on a word (`CandidateWordVariant`), a token
  (`IAnalysisToken`) or a text (`string`). `ConditionGroup` makes "all of", "any of" and "not" on any of them.
- **A `Fact<TSubject>`** is a yes-or-no condition defined once, such as `TokenConditions.IsAGodanVerb`.
- **A `ConditionSubject<TFrom, TSubject>`** is a subject found from another, such as `WordConditions.PrecedingToken` or
  `TokenConditions.Surface`. Its `Meets` makes a `SubjectCondition`.
- **`ConditionLevel<TSubject>`** knows a level's conditions and reads them back from JSON. `MatchConditionJson` reads and
  writes a whole match-condition.
- **`WordConditions`, `TokenConditions` and `TextConditions`** hold each level's facts, subjects and factory methods. In
  code, a match-condition reads like the tree:

  ```csharp
  ConditionGroup.AllOf(
     PrecedingToken.Meets(TokenConditions.IsInAnIrrealisForm),
     ConditionGroup.Not(PrecedingToken.Meets(TokenConditions.IsAnIchidanVerb)),
     FollowingText.Meets(TextConditions.StartsWithOneOf("湧", "わ")))
  ```

**Decisions made while building it:**
- **A split piece's tokenizer facts are those of the token it was split from:** its part of speech, inflection type,
  inflected form and reading (open question 4). So the ending る of 食べる is a verb, in its dictionary form, read タベル.
  JAStudio's own facts about it, such as being a dictionary-form verb's ending, are its own.
- **An inflection type condition takes a type or a family.** 五段 is met by every godan type, 特殊 by every special one.
  Most families aren't types themselves: of them, only 一段 and ラ変 are.
- **A part of speech condition takes the levels from the top.** 名詞 is met by every noun, and 名詞・接尾 by every suffix
  noun.
- **Text comparisons take a set of texts, and compare ordinally.** The kinds are "is one of", "starts with one of",
  "ends with one of" and "contains one of". A regular expression is case-sensitive.
- **An empty group adds nothing, as in the note search.** So a group just added doesn't change what matches.
- **What can't be made, can't be created.** That covers a part of speech, inflection type or inflected form the
  tokenizer never gives, a comparison with no text or with an empty text, and an invalid regular expression.
- **The texts around a word are as its tokens have them** (`CandidateWord.PrecedingText` and `FollowingText`): without a
  `<wbr>` hint, and with the characters that confuse the tokenizer, such as `!`, as full-width spaces.
- **No code is shared with the note search.** The tree is generic over its subject within matching. Whether the note
  search's tree becomes `Condition<JPNote>` is still to be found out.

**A name the model leaves behind: head and tail.** Today's rules and state tests use these for the token before and after a word, as in "Adverb (head)"
and the `Head` and `Tail` folders. In linguistics a phrase's head is something else entirely. The model says preceding
and following, and the old names go with today's rule types in increment 3.

### Increment 2, as built

**Matching tests the match-condition now.** `VocabMatch` builds each vocab's match-condition from the rules the corpus
stores today (`TodaysMatchingRules.MatchConditionOf`) and tests it in place of the 21 requirements it replaces. The
poison word flag and dictionary form (start) stay requirements of their own. A condition the word doesn't meet is a
failure reason of its own, such as `not_met::Not: The preceding token: is a godan verb`.

**The translation, rule by rule** (`TodaysMatchingRule`): each knows its name in the corpus, the condition it becomes,
and the requirement that tests it today. Required becomes the condition and forbidden its negation, with these
exceptions:
- **Surface** forbidden becomes "not: matched by an inflected surface".
- **Sentence start** becomes a condition on the preceding text: empty, or ending with a character that starts a
  sentence. Today's rule reads the preceding token instead, and an empty one counts as the sentence's start: the stem
  JAStudio splits off the く of 寄ってく is one. That's the only difference, in 25 matches, all of which other rules
  reject anyway. A `review:matching:` comment marks it.
- **Past tense stem** becomes "any of: the preceding token is a past-tense stem; all of: the preceding text isn't
  empty, the first token is a past-tense marker". That's today's rule exactly: at the sentence's start a marker is the
  tokenizer's misreading, as in だーめ.
- **Prefix is not, required prefix and suffix is not** test the preceding or following token's surface, as today, not
  the texts: a prefix longer than the token before would differ.

**The checks, both clean:**
- **The side-by-side check:** the MCP tool `compare_match_conditions_with_todays_rules` tests every rule both ways on
  every vocab match of every sentence: 341,028 matches. Every rule agrees everywhere but sentence start's 25. Godan and
  godan imperative (head) are set on no vocab that matches anything.
- **A full rebuild changes nothing.** It takes as long as before: 5.5 to 7.6 seconds over three runs, before and after.
- **The parsing cases pass unchanged.**

**What increment 3 deletes:** the 21 requirements' state tests, `TodaysMatchingRule` and the MCP tool, once the corpus
stores match-conditions.

### Increment 3, as built

**The corpus stores each vocab's match-condition,** as `matching.condition`: an "all of", absent for a vocab without one.
422 of the 28,004 vocab have one. A vocab's matching configuration in the corpus now looks like this:

```json
"matching": {
  "requiredOrForbidden": { "dictionaryFormStart": "forbidden" },
  "flags": ["isInflectingWord"],
  "condition": { "allOf": [
    { "precedingToken": { "fact": "irrealis" } },
    { "not": { "firstToken": { "anyOf": [{ "fact": "godanPotentialStem" }, { "fact": "godanPotentialInflection" }] } } }
  ] },
  "yieldToSurface": []
}
```

**The migration, in two commits**, so that the code that made it is in the history:
- **Step 1** read both formats and wrote the new one. In the headless JAStudio: a full rebuild on the old corpus changed
  nothing; saving every note rewrote all 28,004 vocab files; read as notes, every vocab was the same as before; a full
  rebuild changed nothing more.
- **Step 2** removed reading the old format. A full rebuild changed nothing, and saving every note again wrote every file
  as it was. A vocab file of the old format now fails to load, as a file of an older format should.
- **The parsing cases** pass unchanged, on a stripped-down corpus refreshed in the new format.

**The rules, now views of the match-condition** (`MatchingRules`, namespace `Conditions.Rules`):
- **Each of the 21 rules a match-condition replaces is a `MatchingRule`:** a `RequireForbidMatchingRule`, with the
  condition it is when required and when forbidden, or a `TextsMatchingRule`, whose condition is made of its texts. Each
  knows what it weighs.
- **`MatchConditionAsRules`** reads a vocab's match-condition as rules, and edits it rule by rule while it's made of
  rules only: each condition a rule's, and no rule twice. Every migrated vocab is. Editing keeps the rules' conditions in
  the order of `MatchingRules.All`, the order the migration wrote them in.
- **The Matching tab** shows the match-condition as text at its top, and edits it through the rules as before. When it
  has other conditions, the rules' sections are locked and a line says to edit it as a whole.
- **The MCP server** edits the rules as `matching.rules.<rule>`, and the whole match-condition as JSON, as
  `matching.condition`, which can hold any condition.

**Decisions made while building it:**
- **Priority is computed from the match-condition** (open question 2, (b)): each condition that is a rule's weighs what
  the rule did, and others weigh nothing. So the migration changed no priority, and requiring a rule on the Matching tab
  still raises the vocab's, as before. What priority should be once conditions can say more is still open.
- **Dictionary form (start) stays a require/forbid rule** (open question 1, for now). Forbidding it still differs from
  leaving it unset in what it weighs, so a flag would have changed priorities. Yielding the last token stays one too,
  a rule on display.
- **Yield to surface** is the vocab's `YieldToSurface`, stored as `matching.yieldToSurface`. The "configurable rules"
  are gone.
- **The tags for the configurable rules in use stay,** read from the rules, and so does narrowing a reparse when the
  surface is required.
- **A match-condition that isn't an "all of", given as JSON, is made the only condition in one.**

**What went:** the 21 rules' requirements (the `StateTests` classes), the rules' fields in `VocabMatchingRequireForbidFlags`,
`VocabNoteMatchingRules`, the side-by-side check's MCP tool, and the inspector helpers only they used.

**Found and fixed on the way:** the parsing cases' full mode made a run per specification, and the two of refreshing ran
side by side, writing and deleting each other's files. Each mode now makes one run.

### Increment 4, as built

Built 2026-11-11, before the yardstick, on Magnus's word: "I want to be able to work on it as I study too. I want to be
able to see the tree :)".

**The Matching tab shows the match-condition as a tree, and edits it as the note search's tree is edited:**
- **The keys are the search's:** `+` opens the add menu, and a key from it, typed on the tree, does what it does there.
  `-` negates the selected line, Del removes it, Enter or a double-click changes its texts, and Ctrl+Z undoes. The
  right-click menu has the rest, including switching a group between "all of" and "any of".
- **A line is a condition, or a group with its conditions under it.** Negations and conditions on a subject are shown in
  front of what they hold, on its line: "Not: The preceding token: Surface: ends with one of ん", or "The following token:
  Any of:" with the conditions on the token under it.
- **The add menu offers what fits where the condition goes.** At the root: a word's facts, groups, and its subjects (the
  preceding, first, last and following token, the preceding and following text, the surface and the base form), each a
  submenu of the conditions on it. Under "The following token: Any of:": a token's facts, groups, its texts, and part of
  speech, inflection type and inflected form. So p, f, g adds "The preceding token: is a godan verb".
- **The menu is built from the conditions' catalogs** (`WordConditions`, `TokenConditions`, `TextConditions`), so a
  condition added to a catalog shows in it at once. Its keys are picked automatically from the items' names
  (`AutomaticMnemonics`): the first letter of a word that says something, if no item before took it.
- **A condition that takes texts opens a small dialog:** "ends with one of" takes the texts separated by spaces,
  "part of speech is" its levels from the top, such as 名詞 接尾, and "matches the regular expression" the whole
  pattern. What can't make a condition, such as a part of speech the tokenizer never gives, shows why, and OK stays
  disabled.
- **The rules' editors and the tree show each other's changes.** Requiring a rule by its radio button adds its line to
  the tree, and the tree's undo takes it back. The rules' sections lock, with a line saying so, while the match-condition
  holds conditions that aren't rules'.
- **Every change is saved and reparses the vocab's sentences at once,** as the rules' editors always did. The keyboard
  stays on the tree's selected line through the reparse.

**In Core:**
- **`Condition`, a non-generic base of `Condition<TSubject>`,** is the tree's shape: each condition's `Children`, the
  condition at a `ConditionPosition`, and the tree with one replaced. A tree holds conditions on several subjects, so the
  editor works with them as `Condition`, through `IConditionGroup`, `INegatedCondition` and `ISubjectCondition`, and
  offers what a subject has through `IConditionLevel` and `IConditionSubject`.
- **`EditableMatchCondition`** is the search's `EditableSearch` for a match-condition: changes by position, and undo. Its
  root is always an "all of" group, never negated. Removing a condition removes the negations and conditions on
  subjects that held only it. `LevelAt` says which conditions there are at a position: a word's at the root, a token's
  inside "The preceding token:".
- **`ConditionTestWithTexts`** is a kind of condition made from what's typed, such as "ends with one of", with what typing
  makes a condition, for changing it. The levels list theirs.

**Shared with the note search, after trying it** (the open question in
[Sharing code with the note search](#sharing-code-with-the-note-search-to-find-out)):
- **Shared:** `ConditionPosition`, moved to `JAStudio.Core.Note.ConditionTrees`, and the add menu's key picking
  (`SpecMenuItem.ItemPickedBy`).
- **Alike, not shared yet,** each marked with a `refactor:` comment: `EditableMatchCondition` and `EditableSearch`, and
  the two views' keys, menus and focus handling. Sharing them needs the search's `NoteCondition` and `Condition` to share
  the tree's shape, which is the question of the search's tree becoming a `Condition<JPNote>`.

**Not built yet:**
- **Changing a condition's subject or its condition-test in place,** such as the preceding token to the following one.
  Remove it and add the new one.
- **Picking a part of speech from its tree,** as a tag is picked. It's typed for now.
- **Negating inside a condition on a subject from the keyboard.** `-` negates a line's whole condition: "Not: The
  preceding token: is a godan verb", which is met at the sentence's start. "The preceding token: Not: is a godan verb",
  which isn't, takes a group under the subject: add "The preceding token: All of:", add the fact in it, and negate that.
  *With the empty token of increment 5, the two mean the same, so this goes.*

**Found on the way:**
- **Why what's typed makes no condition is the editor's own question, not a caught contract assertion.** The dialog
  first caught the conditions' contract assertions to show why, which hid any bug that failed one in the same calls,
  and showed the reason wrapped as "Argument.Assert(...)". The Compze session pointed out the real cause (2026-11-11). Now
  each kind of value says why it makes no condition, such as `PartOfSpeechIs.WhyTypedLevelsAreNoPartOfSpeech`, the
  conditions' constructors assert the same rule, and the editor asks before making one. Only a regular expression's
  parsing is caught, around itself alone, since .NET has no other way to check a pattern.
- **The headless JAStudio passes no keys into a menu opened with a submenu showing,** the note search's as well, so the
  MCP server drives those menus by clicks.

## How conditions display: as code (agreed with Magnus, 2026-11-12)

The sentence-like lines of increment 4, such as "Not: The preceding token: Surface: ends with one of ん", go. Magnus:
"it is a logical boolean condition. I would like it to display very similarly to how such a condition displays in a
programming language."

```
PrecedingToken.IsInAnIrrealisForm
!PrecedingToken.IsAnIchidanVerb
PrecedingToken.Surface.EndsWithOneOf("ん")
!FollowingToken.Surface.StartsWithOneOf("湧", "わ")
AnyOf(
   !Surface.In("本の"),
   FollowingToken.PartOfSpeech.Is(名詞・数))     // noun › numeric
!LastToken.AllOf(
   PartOfSpeech.Is(動詞),                        // verb
   InflectedForm.In(
      連用形,                                    // continuative
      連用タ接続))                                // continuative, ta-connection
```

- **Each top-level line must hold.** The root "all of" isn't shown.
- **The dots go from the word to what's tested:** the preceding token, its surface, the test. A line is about what the
  line it's under is about, and a top-level line is about the word: `Surface` there is the word's.
- **The names are the code's,** and the code is renamed where the display needs another. `In` is "is one of";
  `StartsWithOneOf`, `EndsWithOneOf` and `ContainsOneOf` keep their names (Magnus: "a bit clearer to me. We can polish
  later though").
- **Groups are `AnyOf(...)` and `AllOf(...)`,** with commas and closing parentheses (Magnus: "Keep them").
- **Negation is `!`, as in C#** (Magnus). It's shown in a color that stands out, since a `!` at the start of a line is
  easy to miss. With the empty token (below), a `!` means the same wherever it sits, so one form is enough: in front of
  what it negates.
- **Texts are in quotes.**
- **Values from the tokenizer's fixed lists are unquoted,** in Japanese with their English beside them: parts of
  speech, inflection types and inflected forms. Several values go one to a line (Magnus). The selectors show both, and
  find a value by either. The same goes for anything else the editor shows in Japanese only (Magnus). The English is
  in the code already: `JNPartsOfSpeech`, `InflectionTypes`, `InflectionForms`.
- **The same text wherever a condition shows:** the tree, the failure reasons in the sentence breakdown and in
  `analyze_sentence` (`not_met::!PrecedingToken.IsAGodanVerb`), and the specs' failure messages.
- **The C# that builds a condition reads the same, as far as C# allows** (Magnus: "If that is not hard, by all means do
  it"). C# 14's extension members can give each subject the conditions on it, so `PrecedingToken.Surface.EndsWithOneOf("ん")`
  is C# as it stands. Inside a subject's group the bare names clash in C#, since the word and a token both have a
  `Surface`, so there the C# needs a prefix.

### A missing token: the empty token (agreed with Magnus, 2026-11-12)

**Before a sentence's first word, `PrecedingToken` is the empty token, and after its last, so is `FollowingToken`. The
empty token has no part of speech and no facts, and its texts are empty. Every condition is evaluated on it exactly as
written.** The word's first and last tokens always exist, and a text is never missing, only empty, so the empty token is
all there is to the question of what's missing.

- **Where a `!` sits never matters.** "The preceding token isn't a godan verb" and "it's not so that the preceding token
  is a godan verb" are both met at the sentence's start.
- **Requiring a token is a line of its own,** `PrecedingToken.Exists`, written only when it's meant. The suffix うる, which
  needs a word before it:
  ```
  PrecedingToken.Exists
  !PrecedingToken.IsANonWordCharacter
  ```
- **A name must say what its definition does on the empty token.** `Token.IsAContentWord = !PartOfSpeech.Is(助詞)` is
  true of the empty token, since it isn't a particle. If a content word needs a token, its definition says so:
  `AllOf(Exists, !PartOfSpeech.Is(助詞))`.

**Why not the rule increments 1 to 4 built:** a condition on a missing token was false without looking inside it
(`SubjectCondition.IsMetBy`: the subject found, `&&` the condition met). So a negation inside the token, written there or
in a shared-condition's definition, was never evaluated at the sentence's edges, and "the preceding token isn't a godan
verb" differed from "it's not so that it's a godan verb" only there. Magnus, after a long and confusing discussion: the
outcomes "literally seem utterly insane to me". Under the empty token, nothing is evaluated other than as written.

**The corpus doesn't change:** of the 421 stored match-conditions (2026-11-12), none has a negation, an empty-text test or
a regular expression inside the preceding or following token, the only conditions whose result the switch can change. A
full rebuild checks it when it's built, in increment 5.

### Shared-conditions, as they display

Defined once, with what it's about in front of its name, so it's offered where that is: a token's under any token.

```
Word.FollowsAPastTenseStem =
   AnyOf(
      PrecedingToken.IsAPastTenseStem,
      AllOf(
         !PrecedingText.IsEmpty,
         FirstToken.IsAPastTenseMarker))

Token.IsAdverbLike =
   AnyOf(
      PartOfSpeech.Is(副詞),                     // adverb
      AllOf(
         PartOfSpeech.Is(形容詞),                // i-adjective
         Surface.EndsWithOneOf("く")))
```

Used by name, as a built-in fact is, such as `FollowsAPastTenseStem` or `PrecedingToken.IsAdverbLike`. The name has a
color of its own, as Rider colors the methods you wrote. Hovering over it shows its definition, an arrow expands the
definition in place, greyed, and F12 or Ctrl+click opens it for editing. Magnus asked for hover or expanding; this
proposes both.

### Decided, on the backlog: the tree checked against a word in a sentence

Magnus, 2026-11-12: "Seeing matching failures that way is definitely something I want." First an idea of Claude's
(2026-11-11). When a vocab matches a word it shouldn't, or misses one it should, the question is which of its conditions
decided it. The tree editor takes a sentence and a place in it, such as from the word's line in the sentence's
breakdown, and marks each line of the tree ✓ or ✗ for that word. Editing a condition then shows at once whether the word
matches now, much faster than reading the failure reasons in the breakdown.

### To consider soon: storing a match-condition as its text

An idea of Claude's (2026-11-12). The corpus could store `matching.condition` as the lines the editor shows, a list of
strings in the vocab's JSON, instead of nested JSON objects. A vocab file and its diff in git would read as the editor
does, and the MCP server and the specs would take the same text. Magnus: "Stored data is sacred, would that risk it in
any way?" What it would risk, and how that's guarded:
- **A new parser reads the data,** where JSON's reader is mature. A bug that reads a line as another condition than the
  one written would change parsing without failing. The guards: each condition, as it's written, is parsed back and
  asserted equal to what was written; a line that doesn't parse stops the load, never skipped; and the migration is
  checked as increment 3's was: a full rebuild changes nothing, and every vocab reads back as before.
- **Hand edits become easy,** in a text editor or a merge. A typo that doesn't parse fails loudly. One that parses to
  another valid condition is as possible as in JSON.
- **The cost: the stored names are the displayed names.** Each rename of a condition, including the naming pass still to
  come, rewrites every vocab file that uses it: a migration each time, checked by a rebuild, and churn in git. JSON
  keys let the display's names be polished freely.
- **The English is display only:** the files store the Japanese values.

Claude's recommendation: decide after the naming pass, so that renames aren't migrations. Nothing in the display depends
on it.

### Increment 5, as built

Built 2026-11-12, in five steps, each a commit of its own. A full rebuild after each changed no notes.

1. **The empty token** (`Conditions.Tokens.EmptyToken`). `WordConditions.PrecedingToken` and `FollowingToken` find it where
   there's no token, so every subject is found (`ConditionSubject.FoundIn` never gives null) and every condition is tested
   on it as written. Its part of speech is `JNPartsOfSpeech.None`, which has no levels and isn't among the tokenizer's.
   `TokenConditions.Exists` is the first fact on a token.
2. **Conditions written as code** (`Conditions.Code`). Each condition writes its own lines (`Condition.CodeLines`), each
   tagged with the position in the tree of the line it's on, made of pieces of three kinds: code, negation and comment
   (`CodePiece`). `ConditionCode` writes a vocab's match-condition, a whole condition, or a condition on one line for
   `ToString`, which the failure reasons use: `not_met::!PrecedingToken.IsAGodanVerb`.
   - **The root's line is a comment heading,** `// A word matches the vocab when each of these holds:`, rather than no
     line: the root needs a line for its `+` button and the keyboard.
   - **Names:** facts and subjects are named by the fields that define them (`nameof`). `TextConditions.IsOneOf` became
     `In` and `IsEmptyText` became `IsEmpty`. The conditions are named for their code too: `TextIsIn`, `InflectionTypeIsIn`
     and `InflectedFormIsIn` were `...IsOneOf`, beside `PartOfSpeechIs`. Their keys in the corpus keep the old names, so
     no stored note changed. The tests with texts are named in code, such as `EndsWithOneOf` and
     `PartOfSpeech.Is`, and the add menu picks its keys from the words of a code name (`AutomaticMnemonics`).
   - **Values are in ordinal order,** as a condition's set of texts keeps them, so `InflectedForm.In(連用形, 連用タ接続)`
     shows as `InflectedForm.In(連用タ接続, 連用形)`.
   - **A comment is three spaces after its code,** not aligned in a column: Japanese is wider than a monospace cell.
   - **The families of inflection types have English names** (`InflectionTypes.EnglishOf`), such as "every godan type"
     for 五段.
3. **The tree shows the code in pieces,** in a monospace font (`CodeFont` in `App.axaml`): a `!` in orange and bold, a
   comment dimmed. A condition's lines after its first are indented under it.
4. **The tokenizer's values are picked, not typed.** `PartOfSpeech.Is`, `InflectionType.In` and `InflectedForm.In` offer
   their values (`ConditionTestWithTexts.PickableValues`, each a `TokenizerValue`): every part of speech and every kind
   of one, every inflection type and every family of them, every inflected form, each with its English, the tokenizer's
   unknown `*` left out. A condition's code lists its values as the same `TokenizerValue`s. The dialog filters them by
   Japanese or English, whatever the case. Picking types the values into the typed text the editor already made
   conditions from (`ConditionTestWithTexts.TypedTextPicking`), so making, checking and changing a condition work as
   before. A part of speech is picked alone (`PicksOne`). A dialog's keyboard goes to the first visible input marked
   `initialFocus` (`DialogInitialFocus`), so the dialog marks both its text box and its filter.
5. **C# builds a condition as its code reads.** A subject is a way to what a condition can be on (`IConditionPath`), and
   C# 14 extension members give what leads to a token the token's conditions (`TokenConditionsInCode`) and what leads to
   a text the text's (`TextConditionsInCode`), each forwarding to the condition that defines it. `!` is an operator on
   conditions. Inside a group on a token, its conditions are `TokenConditions`', such as
   `TokenConditions.PartOfSpeech.Is("動詞")`, since a word and a token both have a `Surface`. `MatchingRules` is written
   so, and a specification checks that every fact about a token and every test of a text can be.
   - **One spelling:** the factories from increment 1, `TokenConditions.PartOfSpeechIs(...)`, `InflectionTypeIsOneOf(...)`
     and `InflectedFormIsOneOf(...)`, are gone, so C# writes these conditions only as the tree shows them (Magnus,
     2026-11-12).

**Not built yet:**
- **Moving from the picker's filter to its list takes Tab;** there's no arrow key into the list.
- **A shared-condition's hover and expansion,** which come with shared-conditions (increment 6).

### Increment 6, step 1, as built

Built 2026-11-12. No note changed: a full rebuild before and after the code, and after the five were defined, changed
none.

1. **A shared-condition is a condition, as a fact is** (`Conditions.Sharing.SharedCondition<TSubject>`): a leaf of the
   tree, used by its name, met when its `Definition` is. Every tree that uses one holds the same object, the one the
   collection loaded, so a word is tested against its definition as it is. Two are equal when their names are, as two
   facts are. It's on one kind of subject, and offered wherever a condition on that subject goes: one on a token under
   any token. `ISharedCondition` is one whatever its subject, for the editor, and `Condition.SharedConditionsUsed` says
   which a condition uses, and `Condition.Uses` whether it uses one.
2. **The corpus keeps them, one file each** (`SharedConditionsFolder`), in the corpus checkout's folder
   `shared-conditions`, named by the full name, such as `Word.FollowsAPastTenseStem.json`: the subject, the name, and the
   definition, in the JSON a match-condition is stored in (`StoredSharedCondition`, a file's content, which makes the
   shared-condition it keeps). A match-condition stores one as `{"sharedCondition": "FollowsAPastTenseStem"}`.
3. **`SharedConditions` holds the corpus's,** and the collection loads them before its notes, at every load and reload
   (`JPCollection.LoadFromBackend`), since a vocab's match-condition uses them by name: reading one finds them in it
   (`MatchConditionJson`, which now takes them). Each is made by name first, then defined, so that definitions can use
   one another. Loading fails, loud, when a file isn't a shared-condition's, is named for another, or a definition uses
   one there's none of or uses itself, directly or through others (`ISharedCondition.UsesItself`). Defining one
   (`SharedConditions.Define`) checks its name: a name in code, not one a shared-condition on the subject has, nor one
   of the subject's facts, subjects or tests, since the code shows them alike
   (`IConditionLevel.WhyItCannotNameASharedCondition`). Each level has the name code gives its subject, in front of a
   shared-condition's (`IConditionLevel.SubjectNameInCode`): `Word`, `Token`, `Text`.
4. **As code,** a shared-condition is its name, in a color of its own (`CodePieceKind.SharedConditionName`), and as it's
   defined, its full name, `=`, and its definition under it (`ConditionCode.LinesOfASharedCondition`).
5. **In the tree editor,** the add menu has a submenu "Shared" with those on the subject, one pick each. Hovering over
   one shows its definition, and an arrow beside it expands the definition in place, greyed. On the selected line its
   name takes the selection's color, since the selection's blue hid it.
6. **The first five, in the corpus,** made from the matching rules that are small trees, each its rule's condition exactly,
   as the vocab store it: `Word.FollowsAPastTenseStem`, `Word.StartsInAGodanPotentialForm`, `Word.StartsInAGodanImperative`,
   `Word.StartsInAnIchidanImperative` and `Word.IsAtTheSentencesStart`. "StartsIn": the word's first token is a piece of
   that form, its stem or its inflection. Names for Magnus's naming pass. They were defined through the MCP server's new
   `define_a_shared_condition`.

**Found on the way:** the code wrote a text's tab as a tab, and sentence start's zero-width space as itself, invisible.
`StringLiteral` now writes what shows as nothing as C# escapes it: `"\t"`, and a zero-width space by its code point.

**Not built yet:** opening a shared-condition's definition from the tree (step 3). The hover's tooltip is the one part
the headless JAStudio's tools can't show, so it's unchecked by eye.

### Increment 6, step 2, as built

Built 2026-11-12, as planned in
[Step 2: naming a condition never changes what a vocab does](#step-2-naming-a-condition-never-changes-what-a-vocab-does).

1. **Comparing by meaning** (`Condition.MeansTheSameAs`): two conditions mean the same when they're built of the same
   conditions, but for the order of those in a group, with a group of the same kind in a group taken as the conditions it
   holds (`ConditionGroup<TSubject>.FlattenedConditions`), and with a shared-condition taken as its definition. Equality
   is unchanged, order and names included, since the tree editor relies on it.
2. **Weighing by meaning:** the rules recognize a condition that means the same as theirs (`RequireForbidMatchingRule`,
   `TextsMatchingRule`), and a vocab's priority weighs the match-condition's flattened conditions
   (`MatchConditionAsRules.MatchWeight`). So a shared-condition weighs what its definition weighs, a negated one what the
   forbidden rule does, and a combination put in a shared-condition weighs what its conditions did. No file holds a
   weight. A shared-condition's compared texts are its definition's, so a texts rule's condition given a name is still
   that rule's, for its weight and for the tags below `Tags.Vocab.Matching.Uses`. Godan potential built by hand with its
   two facts the other way round now weighs what the rule does.
3. **Using a shared-condition wherever its definition is** (`Condition.WithSharedConditionUsed`,
   `VocabNoteMatchingConfiguration.UseWhereverItsDefinitionIs`, `SharedConditions.UseWhereverItsDefinitionIs`): in every
   vocab's match-condition and the other shared-conditions' definitions, saving what changes. The MCP server's
   `use_a_shared_condition_wherever_its_definition_is` runs it and reports what it changed.
4. **The rule editors keep working on a vocab that uses one of the five:** its shared-condition reads as the rule's
   condition, so the vocab is still made of rules only. Changing that rule's choice on the Matching tab writes the rule's
   condition out again in place of the name; the rule editors go in step 4.
5. **The five used in the corpus:** 87 vocab. Sentence start in 38, godan potential in 36, godan imperative in 16, past
   tense stem in 6 and ichidan imperative in 4, the forbidden ones negated. No vocab holds any of their definitions written
   out anymore. Only those 87 vocab's match-conditions changed, each a written-out condition replaced by the name in its
   place: a full rebuild after it changed nothing more, and no sentence. Full rebuilds before the code and after it changed
   nothing.

**Then, finding the conditions vocab share** (`Sharing.Candidates.SharedConditionCandidates`, the MCP server's
`find_the_conditions_vocab_share`): every condition in the vocab's trees, and every combination of two or more of the
conditions of a group of three to eight, counted by meaning (`ConditionMeaningComparer`, with a hash of what a condition
means, `Condition.HashCodeOfItsMeaning`). Left out: what's a name already, a fact or a shared-condition, on a subject or
negated; a negation, since naming what it negates names it too, and what it negates counts as held by the vocab holding
the negation; and a condition only ever found within a larger one held by the same vocab. Each candidate comes with the
shared-condition whose definition means the same, if there's one, to be used rather than defined.

**Found on the corpus after the five were used, 2026-11-12:** 19 conditions held by three vocab or more, the first:
- `PrecedingToken.Surface.EndsWithOneOf("ん")`, 44 vocab: 37 require it and 7 forbid it.
- `AllOf(PrecedingToken.IsInAnIrrealisForm, !PrecedingToken.IsAnIchidanVerb)`, 20 vocab, such as あせる.
- `AllOf(PrecedingToken.Surface.EndsWithOneOf("ん"), FirstToken.HasATeFormStem)`, 13 vocab, such as んで and んでしまう.
- `AllOf(!IsAtTheSentencesStart, StartsInAGodanPotentialForm)`, 11 vocab, such as えた and えない.
- `PrecedingToken.Surface.EndsWithOneOf("く")`, 10 vocab: 4 require it, such as くなる, and 6 forbid it.
- `AllOf(IsAtTheSentencesStart, LastToken.EndsAStatement)`, 9 vocab, interjections such as あのな and そうだな.

None is defined yet: they wait for Magnus to name them and decide that each is one concept.

### Naming and design questions: `review:matching:` comments

Magnus polishes the names and design at the end of the work, into a language that's intuitive to him and to everyone
working on the corpus (Magnus, 2026-11-10). Waiting for him at each step would stall the work, so until then:
- **I make the calls.** Where I'm confident a name is clearly better, I rename.
- **Each question for that pass is a `//review:matching:` comment at the code it concerns:** a rename I made, a name I
  doubt, a design call he may not share. The IDE lists them among the `review:` comments, and
  `grep -rn "review:matching:" src` lists exactly this work's.
- **What isn't tied to any code goes in this document,** as the open questions below.

## Open questions

1. **Dictionary form (start), until rules-for-many-vocab exist.** *For now it stays a require/forbid rule outside the
   match-condition (increment 3).* The idea: a flag on the vocab, "starts with a dictionary-form
   verb ending", which lifts the default, requires the fact, and expands the placeholder forms. A detail to settle when
   building.
2. **Priority** between vocab that match the same word. For the migration there are two ways to keep each vocab's
   weight exactly:
   - (a) Store it on the vocab as a number, the weight its rules give today. It's simple, exact and visible. But adding
     a condition no longer raises a vocab's priority, as setting a rule does today.
   - (b) Compute it from the tree, each node weighing what its rule weighs today.

   *Increment 3 took (b): each condition that is a matching rule's weighs what the rule did, and others weigh nothing.*
   What decides priority once conditions can say more is still open.
3. **Display:** whether a second tree per vocab, "displayed when", takes over the yield rules, and what rule guarantees
   that conditions on display settle.
4. **A split piece's tokenizer facts:** whether a test of a split piece's part of speech, inflection type or inflected
   form reads the token it was split from, or isn't offered on it.
5. **Rules for every match,** dictionary matches included, when the universal-requirements move out of code.

