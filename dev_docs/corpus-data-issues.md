# Corpus data issues — TODO

> **Status: a list to work through, started 2026-11-09.** Problems found in the corpus's data, mostly while migrating it
> (`corpus-data-model.md`), that aren't being fixed when they're found (Magnus, 2026-11-09). Each is fixed either by
> Magnus in JAStudio's editors, or by Claude through JAStudio's MCP server once Claude has agreed the fix with Magnus
> (Magnus, 2026-11-09). Remove an entry once it's fixed.
>
> The checks JAStudio could run to find such problems itself are in `corpus-validity.md`.

## Found in slice 8 (2026-11-09)

- **Five godan verbs whose compound parts are nonsense: かする (か + する), ぎする (ぎ + する), すする (す + する), 揺する (揺 + する)
  and さする (さ + する).** Generating a vocab's data gave every vocab ending in する without compound parts the parts X + する,
  godan verbs included; it no longer does for godan verbs. Their parts should most likely be emptied. こする had been kept
  from getting them by an empty alternative compound part, which slice 8's migration dropped.
- **Related vocab that are sentences, or several words in one, rather than a word.** Each should most likely be removed, or
  replaced by the word meant:
  - synonyms: `〜ておる |&nbsp;〜とる` in ていました, ています, てました and てん; `<div>なんなんだ</div>` in 何なんだ and
    何なんです; a sentence in ところを.
  - antonyms: sentences in 下って (9 of them), 降って, ほんの and ちょっと:int.
  - confused with: sentences in いつの間に, 何時の間に, なるべく, 前向き, 動物 (2), 北西, 印象, 取材, 客室, 自然と and 自然に.
  - derived from: sentences in 指導 and 潜在意識.

## Found cleaning up the kanji's lists (2026-11-10)

- **Words where a kanji should be, each holding the kanji itself:** the similar meaning `奇跡` of 跡, `露顕` of 露 and `各〜` of 各,
  and the confused-with `蒸気` of 蒸 and `尋問` of 尋. Each is one of the kanji's vocab, so most likely meant as one of its primary
  vocab, the vocab its card shows first: moved there, or removed. The cleanup split only the values that ran several other kanji
  together, such as 佐's confused-with `祐怖` (`dev_scripts/corpus_migrations/clean_up_the_kanji_lists.py`).
