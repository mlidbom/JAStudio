# Corpus data issues — TODO

> **Status: a list to work through, started 2026-11-09.** Problems found in the corpus's data, mostly while migrating it
> (`corpus-data-model.md`), that aren't being fixed when they're found (Magnus, 2026-11-09). Each is fixed by hand in
> JAStudio's editors unless it says otherwise. Remove an entry once it's fixed.
>
> The checks JAStudio could run to find such problems itself are in `corpus-validity.md`.

## Vocab

- **Suru verbs stored as godan verbs** (found 2026-11-08, slice 7b). 要する, こうする, 伍する, 化する and 賞する have
  `godan verb` among their parts of speech, and します has `godan verb` with `transitive` and `intransitive`. The
  dictionary says the five are suru verbs of the special class. Being a godan verb decides how a vocab inflects, so this
  changes how they match.

## Kanji

- **Readings that look wrong** (found 2026-11-08, slice 7a). 行 has や as a primary on reading, and 眩 has げん, an on
  reading, among its kun readings.
