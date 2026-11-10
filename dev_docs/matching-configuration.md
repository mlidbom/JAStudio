# The matching configuration as a tree of conditions

> **Status: a survey, and thinking in progress with Magnus, 2026-11-10. Nothing is decided or built.** Magnus wants the
> matching configuration refactored "to not use hardcoded conditions that only support AND conditions via checkboxes"
> (2026-11-10), into "a flexible tree structure resembling how we implement the note search" (2026-11-08):
> [note-search.md](note-search.md). The goal (Magnus, 2026-11-10): "to be able to craft matching rules for vocab that
> let the parsing produce decent results WITHOUT MANUAL ADJUSTMENT for many more sentences than can be achieved with
> the current limited set of rules that can only do and."
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

Most of the "without manual adjustment" would come from 2 and 3.

### The form that matched

Some vocab are wrong only by one of their forms: ほんの by 本の, ようです by 様です. A condition on the form that matched
lets the rest of a condition apply to that form only, such as: any of (not: the form matched is 本の; the token after is
an amount). The other way is for each form to have a match-condition of its own. *Settled: a form that needs different
matching gets a vocab of its own (see [Settled with Magnus](#settled-with-magnus-2026-11-10)).*

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
  to test. "Not: the token before: a godan verb" is true.
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
   sentences it has now (`LocalNoteUpdater.ReparseSentencesForVocab`). A full rebuild takes about 8 seconds.

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
  rule-for-many-vocab changes many vocab at once, so its edit needs a full rebuild, which takes about 8 seconds.
- **The parsing cases need to carry them too.** The cases run against a stripped-down corpus of the vocab their
  sentences look up (`LookupRecording`). It must carry the shared-conditions and rules-for-many-vocab too.
- **Caching:** a match caches its validity, and only the yield check is computed afresh each time
  (`CustomForbidsNoCache`). A condition on validity must therefore depend only on what doesn't change during the
  analysis.
- **The cost:**
  - a full rebuild takes about 8 seconds, for 49,162 sentences;
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
  false.

### What's stored

A vocab's `matching` becomes:

```json
"matching": {
  "condition": { "allOf": [
    { "tokenBefore": { "fact": "irrealis" } },
    { "not": { "tokenBefore": { "fact": "ichidanVerb" } } },
    { "tokenBefore": { "surface": { "endsWithOneOf": ["ん"] } } }
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

The migration can ship before the tree editor exists:
- **A checkbox view of the tree.** While a vocab's match-condition has today's shape, an "all of" of the nodes today's
  rules translate to, the tab shows it as today's checkboxes and edits it the same way.
- **The tree, read-only.** A vocab whose match-condition has any other shape shows its tree read-only, until the tree
  editor exists.

The MCP server gets a tool to set a vocab's match-condition as JSON.

### Increments

1. **The model:** the three levels, the condition-subject nodes, JSON and descriptions, with specs. Nothing uses it yet.
2. **Testing matches with it, from today's storage:**
   - the match-condition built in memory from today's rules;
   - `VocabMatch` testing it;
   - the side-by-side check, until it's clean.
3. **The migration:**
   - the storage changed, and the corpus migrated;
   - the Matching tab's checkbox view;
   - the MCP tool;
   - today's rule types, and the state tests that read them, deleted.

   The full rebuild must change nothing.
4. **The tree editor** on the Matching tab.
5. **Shared-conditions.**

After the core: the yardstick, rules-for-many-vocab, conditions on display, and vocab given conditions that use the new
power.

## Open questions

1. **Dictionary form (start), until rules-for-many-vocab exist:** a flag on the vocab, "starts with a dictionary-form
   verb ending", which lifts the default, requires the fact, and expands the placeholder forms. A detail to settle when
   building.
2. **Priority** between vocab that match the same word. For the migration there are two ways to keep each vocab's
   weight exactly:
   - (a) Store it on the vocab as a number, the weight its rules give today. It's simple, exact and visible. But adding
     a condition no longer raises a vocab's priority, as setting a rule does today.
   - (b) Compute it from the tree, each node weighing what its rule weighs today.

   What decides priority once conditions can say more is open either way.
3. **Display:** whether a second tree per vocab, "displayed when", takes over the yield rules, and what rule guarantees
   that conditions on display settle.
4. **A split piece's tokenizer facts:** whether a test of a split piece's part of speech, inflection type or inflected
   form reads the token it was split from, or isn't offered on it.
5. **Rules for every match,** dictionary matches included, when the universal-requirements move out of code.

