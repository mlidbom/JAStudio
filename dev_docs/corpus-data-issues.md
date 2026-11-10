# Corpus data issues — TODO

> **Status: a list to work through, started 2026-11-09.** Problems found in the corpus's data, mostly while migrating it
> (`corpus-data-model.md`), that aren't being fixed when they're found (Magnus, 2026-11-09). Each is fixed either by
> Magnus in JAStudio's editors, or by Claude through JAStudio's MCP server once Claude has agreed the fix with Magnus
> (Magnus, 2026-11-09). Remove an entry once it's fixed.
>
> The checks JAStudio could run to find such problems itself are in `corpus-validity.md`.

## Found in slice 8 (2026-11-09)

- **Related vocab that are sentences, or several words in one, rather than a word.** Each should most likely be removed, or
  replaced by the word meant:
  - synonyms: `〜ておる |&nbsp;〜とる` in ていました, ています, てました and てん; `<div>なんなんだ</div>` in 何なんだ and
    何なんです; a sentence in ところを.
  - antonyms: sentences in 下って (9 of them), 降って, ほんの and ちょっと:int.
  - confused with: sentences in いつの間に, 何時の間に, なるべく, 前向き, 動物 (2), 北西, 印象, 取材, 客室, 自然と and 自然に.
  - derived from: sentences in 指導 and 潜在意識.
