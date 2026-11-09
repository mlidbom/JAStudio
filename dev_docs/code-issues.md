# Code issues — TODO

> **Status: a list to work through, started 2026-11-09.** Bugs found in JAStudio's code while working on something else,
> and questions about how it behaves, that aren't settled when they're found, so that they do get settled (Magnus,
> 2026-11-09). Remove an entry once it's settled.
>
> The corpus's data has a list of its own: `corpus-data-issues.md`.

## Hiding all compounds takes apart words that aren't compounds

Found 2026-11-09, inspecting the parsing cases (`parsing-cases-to-inspect.md`). The breakdown's option to hide all
compounds hides every match that spans several tokens, so a word cut into several tokens, by the tokenizer or by
JAStudio's splitting of inflections, shows as junk parts: いえ (the vocab いいえ) as 言う and え, こわっ as こ and わっ,
うるせえ as うる, う, せ and え. Under the default options each shows as the one word.

**The question (Magnus: "an excellent question"):** should hiding all compounds keep a word that isn't made of other
words? Not to change as part of the refactoring of the data model (Magnus, 2026-11-09). The parsing cases record the
current behavior, so a change shows in their breakdowns with the option on.

## A compositionally transparent compound should lose to any compound overlapping it

Found 2026-11-09, inspecting the parsing cases: 会いに行ったんだ displays に and 行った, not に行く and た. Both vocab have
"Yield to overlapping following compound" required, so に行く yields 行っ to 行った. 行った is now flagged compositionally
transparent, which fixes the case only while the option "Hide compositionally transparent compounds" is on: it hides
行った, and に行く has nothing to yield to (the case's breakdown with the option on: 会う | に行く | た | んだ). The option is
on by default, but off in Magnus's configuration, so he still sees 会う | に | 行った | んだ. The design below fixes it
with the option off too: に行く wouldn't yield to 行った (rule 1), and would shadow it.

The flag affects display only, never matching, and only through that option: `ForbidsCompositionallyTransparentCompound`
hides a flagged compound outright when each of its tokens has a valid single-token match. It plays no part in yielding.
It should stay out of matching: matching decides which sentences a vocab is linked to, and 会いに行った is a sentence of
行った.

**The design (Magnus's idea, refined, 2026-11-09):** transparency ranks compounds when two overlap, whatever the hide
option. A compound that isn't flagged compositionally transparent is called opaque here.

1. **An opaque compound never yields to a transparent one,** even with "Yield to overlapping following compound"
   required: `ForbidsHasDisplayedOverlappingFollowingCompound` skips transparent display words.
2. **A transparent compound yields its last token to any overlapping following compound,** as suru-verb, passive and
   causative compounds do under their automatic yield options. "Forbidden" on its yield setting still opts a vocab out.
3. **Between two transparent compounds, today's rules decide.** Without this, each would yield to the other. The yield
   check reads the display words it helps decide, which the code already has a todo about.

A rule for yielding to a compound *in front* isn't needed: an earlier displayed compound already shadows the locations
it covers, and only failed to here because the earlier compound yielded, which rule 1 stops. No new option either:
there's no case known where a transparent compound should beat an opaque one.

Why: the flag is a fact about one vocab, judged by looking at it alone ("does it mean what its parts mean?"), while
"Yield to overlapping following compound" is a guess about which other vocab might overlap it. And a transparent
compound losing costs nothing, its parts saying the same, while an opaque compound losing loses its meaning. The three
automatic yield options guess at transparency from the parts of speech: 反応する, 騙される and 従わせる are all flagged
transparent.

It changes what sentences display as, which the corpus stores (each parsed word's `isDisplayed`): a parsing change, with
a full rebuild and its analysis, and the parsing cases' breakdowns change. Not to change as part of the refactoring of
the data model (Magnus, 2026-11-09).
