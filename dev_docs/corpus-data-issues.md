# Corpus data issues — TODO

> **Status: a list to work through, started 2026-11-09.** Problems found in the corpus's data, mostly while migrating it
> (`corpus-data-model.md`), that aren't being fixed when they're found (Magnus, 2026-11-09). Each is fixed either by
> Magnus in JAStudio's editors, or by Claude through JAStudio's MCP server once Claude has agreed the fix with Magnus
> (Magnus, 2026-11-09). Remove an entry once it's fixed.
>
> The checks JAStudio could run to find such problems itself are in `corpus-validity.md`.

## Found while fixing the related vocab that were sentences (2026-11-10)

- **21 vocab related to themselves, 22 times.** A vocab is never related to itself (`corpus-data-model.md`, slice 8a):
  adding its own name to a relationship does nothing, but these were stored before that rule. Each should most likely be
  removed:
  - synonyms: しか, のような, あれない, れる, れない, られない, えそう:imperative, させていただく, させていただきます, 綺麗, きれい,
    特徴 and なんだかんだ言っても;
  - perfect synonyms: えそう:imperative;
  - see also: なきゃ, なくちゃ, なくては, なければ, ないと, せねば and ぶっ;
  - derived from: 出会う.
