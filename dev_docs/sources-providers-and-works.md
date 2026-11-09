# Sources: the provider and the work — Proposal

> **Status: Claude's recommendation (2026-11-09), for Magnus to decide.** This is slice 4c of `corpus-data-model.md`:
> "untangle the provider from the work in the data". Magnus asked for it this way (2026-11-09): find out what the data
> looks like, then recommend from it, listing what Claude considers works and what providers, where each is stored now,
> and where it would be stored after the change. Nothing here is built. The counts are from the corpus at 9f1b47011b.

## The recommendation, in short

**Two concepts, two tag branches: `provider::` for where a note was imported from, and `work::` for the creative work its
content comes from. `source::` goes, and so does the sentence's `sourceName` field, whose content becomes `work::` tags.**

- **The provider** is the deck, dataset, service or tool the note came into the corpus from: WaniKani, the Core 2000
  deck, JLab, Immersion Kit, Magnus's subs2srs decks, Yomitan, JAStudio's own dictionary lookups. A provider's own
  grouping, such as a WaniKani level or a Core 2000 step, stays under it: `provider::wanikani::level::19`.
- **The work** is what the content itself comes from: an anime, its season and episode, a song. It's independent of the
  provider: Shirokuma Cafe is the work of 13 Immersion Kit sentences and of more than 40 JLab ones. Most notes have no work: a WaniKani
  vocab, a Core 2000 sentence.

**Why tags rather than typed fields:**
- **Nothing in JAStudio's logic depends on the source.** The code reads it in one place, to show a sentence's source in
  the search results (`JPNote.SourceTag`), and the import adds it as a tag (`import-profiles.json`, `tagsToAdd`). Target
  principle 3, that the code interprets typed fields rather than tags, doesn't come into play until something does.
- **Magnus organizes by them** (decided 2026-11-06: organizing builds on hierarchical tags), such as finding the next
  episode's notes, and tags already have all the tooling: the tag picker, the tag management tree with its renames and
  moves, the search's "tags include" condition, and Anki's browser.
- **The migration is mechanical**, a rewrite of tags, except for naming JLab's and Immersion Kit's works (below).
- **They can still become typed fields later**, once some logic needs them, such as priority per work (slice 5) or
  leaving the works out of a corpus shared without its commercial sources (`media-storage-and-import.md`,
  "Source-Revealing Tags"). Two branches make both easier than one tangled one.

The alternative, typed fields (`"provider": …, "work": …` on every note), would need its own editors, its own search
conditions and its own management UI to be as usable as tags already are, for no logic that needs it yet.

## The providers

| Provider | Now | After | Vocab | Kanji | Sentences |
|---|---|---|---|---|---|
| JAStudio's dictionary lookups (JMdict, through jamdict): vocab JAStudio created | `source::jamdict` | `provider::jmdict` | 12,775 | | |
| WaniKani, by level | `source::wani::level::NN` | `provider::wanikani::level::NN` | 6,629 | 2,076 | |
| WaniKani's context sentences | `source::wani` + `source::wani::current` | `provider::wanikani` | | | 14,567 |
| …no longer on WaniKani | `source::wani::removed` | `provider::wanikani::removed` | | | 30 |
| Yomitan, mined by Magnus | `source::yomitan` | `provider::yomitan` | 5,567 | 396 | |
| The Core 2000 deck, by step | `source::core2000::stepNN` | `provider::core2000::step::NN` | 584 | | 1,996 |
| JLab, by deck | `source::jlab::beginner`, `::inter`, `::booster::1`/`::2`, `::num` | `provider::jlab::beginner` and so on | | | 3,011 |
| Immersion Kit | `source::immersion_kit` | `provider::immersion-kit` | | | 781 |
| Magnus's subs2srs decks | `source::anime::…` (with the work) | `provider::subs2srs` | | | 28,693 |
| Tatoeba | `source::tatoeba` | `provider::tatoeba` | | | 3 |
| Made by hand, or unknown | `source::misc`, `source::free::misc`, or no source tag | `provider::hand-made`, or none | 2,449 without | 82 without | 46, 2 without |

- **The JLab booster sentences are all in the intermediate deck too** (all 447 have both tags), so a note can have two
  provider tags. That's the decks' structure, not a mistake, as `corpus-data-model.md` guessed.
- **`source::grammar`** (42 vocab, 7 kanji) isn't where the notes came from but what they are, grammar constructs made by
  hand. It would become `provider::hand-made` and a label, such as `-::grammar`. *A question for Magnus.*

## The works

| Work | Now | After | Notes |
|---|---|---|---|
| 夏目友人帳, by season and episode, and its OVAs | `source::anime::natsume::sN::EE`, `::nyhnme1` | `work::anime::natsume::sN::EE` | 20,028 sentences |
| Violet Evergarden, by episode | `source::anime::violet::EE` | `work::anime::violet::EE` | 3,127 sentences |
| 一週間フレンズ (One Week Friends), by episode | `source::anime::one-week::EE`, and `sourceName` `Isshuukan_Friends_(Test)_EE` | `work::anime::one-week::EE` | 2,780 sentences; the `sourceName` is the subs2srs deck's name, and goes |
| 少女終末旅行 (Girls' Last Tour), by episode, and its songs | `source::anime::girls-last-tour::EE`, `::songs::…` | `work::anime::girls-last-tour::EE` | 2,758 sentences |
| 未来のひとへ, song lyrics | `source::lyrics::未来のひとへ` | `work::lyrics::未来のひとへ` | 50 vocab, 42 kanji, 33 sentences; the vocab and kanji keep their other provider, such as `yomitan` |
| JLab's works: 287 names in beginner, 203 in inter, 143 in booster, 84 in num | the sentence's `sourceName`, such as `Shirokuma Cafe` or `Clannad After Story (eng jp)` | `work::anime::<title>`, such as `work::anime::shirokuma-cafe` | 3,011 sentences |
| Immersion Kit's works: 308 | only the external id, such as `anime_shirokuma_cafe_000012345` | `work::anime::<title>` | 781 sentences; today the work is nowhere but in the id |

- **Naming JLab's and Immersion Kit's works** is the only part that isn't mechanical. JLab's names come in variants,
  such as `Kanon 2006 (JP+Eng)` and `Kanon (2006) (Eng + Jp)`, and the same works appear in Immersion Kit's ids. Claude
  would turn each into a title in lowercase with hyphens, dropping the variants' "(JP Eng)" parts, and list for Magnus
  every name that becomes each title, to correct before the migration.
- **Not every work is an anime:** JLab has dramas, such as `結婚できない男`. Either the works get a medium level that
  Claude guesses and Magnus corrects, or the medium level goes, as `work::natsume::s4::11`. *A question for Magnus:*
  the subs2srs works have `anime::` today.

## Other data that's part of the question

- **The external id** (`sourceData.externalId`) is the provider's id for the note, such as a subs2srs line's timing
  `ny4-nodup_11_0.15.23.540-0.15.25.620`, a Core 2000 `sentence:250715`, or an Immersion Kit id. It stays where it is:
  it's the provider's data, like the rest of `sourceData`.
- **The vocab's and kanji's `sourceData`** (WaniKani's mnemonics, answers) stays as it is: it's the provider's.
- **The priority tags name providers and works:** `-::priority::high::011_Natsume_01`, `-::priority::low::110_core_2000`,
  `-::priority::medium::15-jlab-beginner`. Priority is slice 5. Once provider and work are separate, a priority per work or
  per provider, rather than per note, becomes possible, but that's for slice 5 to decide.
- **The media layer's folders** are organized by provider and work too (`commercial-02/anime/natsume-yuujinchou`,
  `commercial-01/vocab/wani`). They're files, not notes' data, and stay as they are: that's slice 9.

## Problems the survey found

- **The 233 Violet Evergarden sentences tagged with both episode 07 and 08** are all episode 07, by their external ids
  (`A_VioletEvergarden_S01_E07_…`). The migration would drop their `::08`.
- **A Girls' Last Tour sentence with Natsume's priority tag** (`-::priority::high::011_Natsume_01` on a
  `girls-last-tour::songs::final-ending` sentence): priority tags don't always match the source. For slice 5.
- **2,449 vocab, 82 kanji and 2 sentences have no source tag.** They'd have no provider tag either: unknown.

## What the change would do to the code

- `JPNote.SourceTag`, read only for the search results' "Source" column, would become the work, or the provider for a note
  without one.
- `SentenceNote.SourceName`, its editor field and the import profile's `sourceName` mapping would go; an import profile's
  `tagsToAdd` would hold `provider::…` tags.
- The migration: rewrite the tags as in the tables, turn every `sourceName` into a `work::` tag through the reviewed
  names, and give Immersion Kit's sentences their works from their ids.

## Questions for Magnus

1. Tags, as recommended, or typed fields?
2. A medium level in the works (`work::anime::…`), or none?
3. `source::grammar`: `provider::hand-made` and a `-::grammar` label?
