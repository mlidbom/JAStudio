# Corpus data issues — TODO

> **Status: a list to work through, started 2026-11-09.** Problems found in the corpus's data, mostly while migrating it
> (`corpus-data-model.md`), that aren't being fixed when they're found (Magnus, 2026-11-09). Each is fixed either by
> Magnus in JAStudio's editors, or by Claude through JAStudio's MCP server once Claude has agreed the fix with Magnus
> (Magnus, 2026-11-09). Remove an entry once it's fixed.
>
> The checks JAStudio could run to find such problems itself are in `corpus-validity.md`.

## Ergative twins that aren't ergative twins (found 2026-11-09)

Found when a vocab's ergative twin became a list kept mutual (`corpus-data-model.md`). The migration didn't make these
mutual, so the mistake doesn't spread to a second vocab. Most look like "Set ergative twin" picked from the context menu of
text selected in a sentence.

- 行う's twin is a sentence, 待って 猫ちゃん 行っちゃ嫌だ.
- 名刺's twin is a sentence, あの会社の社長さんから名刺をもらいました。
- 静寂's twin is a sentence, 赤ん坊の泣き声が静寂を破った。
- 疑う's twin is the kanji 疑.
- 含む's twin is 含まれる, its passive, not an intransitive counterpart.
- 釣る's twin is 釣られる, its passive.
- 時間目's twin is 時限目, a noun of much the same meaning: a synonym, if anything.
