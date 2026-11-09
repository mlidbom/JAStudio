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
