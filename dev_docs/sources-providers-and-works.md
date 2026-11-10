# Sources: the provider and the work

> **Status: partly decided (Magnus, 2026-11-10), nothing built.** This is slice 4c of `corpus-data-model.md`: "untangle
> the provider from the work in the data". What's decided is marked *Decided*; the rest is Claude's recommendation, for
> Magnus to confirm. The counts are from the corpus at 83f0b2b734, surveyed with the MCP server's `get_the_tag_tree`.

## The two concepts (decided)

- **The provider** is where we got a note from: an organization, website, service, dataset or tool, such as WaniKani,
  Core 2000, JLab, Immersion Kit, Tatoeba, JMdict or Yomitan. For Yomitan the dictionaries behind it would be the real
  provider, but which they were isn't recorded. What we made ourselves, Magnus or Claude, has the provider `jastudio`.
- **The work** is which work a note's content is from: an anime, a drama, a book, a song, with its parts below it, such
  as a season and an episode, a volume, an OVA or its songs.

They're independent, which is why `source::` tangled them: one work arrives through several providers (Natsume through
Magnus's subs2srs decks and through JLab; Shirokuma Cafe, Clannad After Story and Fruits Basket through both JLab and
Immersion Kit), and one provider brings hundreds of works (JLab, Immersion Kit).

## Tags, with three rules (recommended)

Structure pays only where code has to act on the data. Nothing in JAStudio's logic reads the source today, and
organizing by provider and work, browsing, searching and renaming are what tags already do. What would need structure,
and when:

- **Deciding what may be shared or shown**, once sharing the corpus is on the table (`media-storage-and-import.md`,
  "access gating" and "source-revealing tags"). That depends on the provider, and for media on each file's own origin,
  not on the work. It would be a small list of providers with each one's licence, keyed by the provider tag.
- **Syncing with a provider** needs the provider's id for the note: already a field, `sourceData.externalId`.
- **Story order, or replaying a scene,** needs the time within the episode: already in the external ids and audio
  file names, such as `A_VioletEvergarden_07_0.19.34.670-…`, to parse per provider when a feature wants it.
- **Linking works to AniList or AniDB** would be a mapping from work tag to their ids, which also absorbs how
  differently they classify seasons, OVAs and specials. The tags stay our own judgment, renamed whenever it suits.
- **Priority (slice 5)** could be rules on tag branches, such as "`work::natsume::s4` is high", which searches already
  express. Slice 5 decides.

The rules that keep structure possible later, mechanically:

1. Two branches: `provider::` and `work::`.
2. The level right below `provider::` names the provider, one name each. Anything goes below it: the provider's own
   structure, such as WaniKani's levels or JLab's decks, and `removed`.
3. The level right below `work::` names the overarching work, such as `natsume`. Anything goes below it: seasons,
   episodes, OVAs, songs. No medium level: `anime::` describes the work, not each note, and JLab also has dramas.

## The providers

| Provider | Now | After | Notes |
|---|---|---|---|
| JMdict: vocab JAStudio created from the dictionary (*decided*) | `source::jamdict` | `provider::jmdict` | 12,775 vocab |
| WaniKani, by level | `source::wani::level::NN` | `provider::wanikani::level::NN` | 6,629 vocab, 2,076 kanji |
| WaniKani's context sentences | `source::wani` and `source::wani::current` | `provider::wanikani` | 14,567 sentences. `current` says nothing the absence of `removed` doesn't |
| …no longer on WaniKani (*decided: kept*) | `source::wani::removed` | `provider::wanikani::removed` | 30 sentences. Likely WaniKani found something wrong with them |
| Yomitan (*decided*) | `source::yomitan` | `provider::yomitan` | 5,567 vocab, 396 kanji |
| Core 2000, by step | `source::core2000::stepNN` | `provider::core2000::stepNN` | 584 vocab, 1,996 sentences |
| JLab, by deck | `source::jlab::beginner`, `::inter`, `::booster::1`/`::2`, `::num` | `provider::jlab::beginner` and so on | 3,011 sentences. All 447 booster sentences are in the intermediate deck too |
| Immersion Kit | `source::immersion_kit` | `provider::immersion-kit` | 781 sentences |
| Magnus's subs2srs decks | `source::anime::…`, with the work | `provider::subs2srs` | 28,693 sentences. A tool, as Yomitan is |
| Tatoeba (*decided*) | `source::tatoeba` | `provider::tatoeba` | 3 sentences |
| What we made ourselves (*decided: `jastudio`*) | no source tag | `provider::jastudio` | 2,449 vocab, most likely all our own: JAStudio creates a vocab without a source tag when the dictionary has no entry for it (`VocabNoteFactory.CreateWithDictionary`) or when it's cloned from another vocab (`VocabCloner`), as the many derived forms, such as あせちゃう and あせてもらう, suggest. Those code paths would tag `provider::jastudio`. The 82 kanji with no source tag are radicals and components made by hand, such as ⺌ and ナ, many with WaniKani's radical names, such as triceratops and narwhal: `jastudio` too |
| Unknown (*decided*) | `source::misc`, `source::free::misc`, or no source tag | no provider tag | 7, 39 and 2 sentences. `free` was the licence regime, not a source |

## The works

| Work | Now | After | Notes |
|---|---|---|---|
| 夏目友人帳 | `source::anime::natsume::sN::EE`, `::ova1`, `::ova2`, `::nyhnme1` | `work::natsume::s4::11` and so on | 20,028 sentences. `nyhnme1` is a subs2srs deck of 258 lines that the data doesn't identify |
| Violet Evergarden | `source::anime::violet::EE` | `work::violet::EE` | 3,127 sentences |
| 一週間フレンズ (One Week Friends) | `source::anime::one-week::EE`, `::songs::op`, `::songs::ed`; `sourceName` `Isshuukan_Friends_(Test)_EE` | `work::one-week::EE`, `work::one-week::songs::op` | 2,780 sentences; the `sourceName` is the subs2srs deck's name, and goes |
| 少女終末旅行 (Girls' Last Tour) | `source::anime::girls-last-tour::EE`, `::songs::op`, `::songs::ed`, `::songs::final-ending` | `work::girls-last-tour::EE`, `work::girls-last-tour::songs::op` | 2,758 sentences |
| 未来のひとへ, a song | `source::lyrics::未来のひとへ` | `work::未来のひとへ` | 50 vocab and 33 sentences. *Decided:* the vocab keep it, the kanji don't (removed: 83f0b2b734) |
| JLab's works: about 500 names | the sentence's `sourceName`, such as `Shirokuma Cafe` or `Clannad After Story (eng jp)` | `work::shirokuma-cafe` | 3,011 sentences |
| Immersion Kit's works: about 227 | only in the external id, such as `anime_shirokuma_cafe_000012345` | `work::shirokuma-cafe` | 781 sentences |

- **JLab's and Immersion Kit's works get the overarching work only** (*decided for JLab's episode ranges*). JLab's names
  give ranges, such as "Naruto Season 2 (26-52)" and "Gintama (202-265)", never an episode. Claude recommends the
  overarching work even where a name gives a season, such as "Fruits Basket Season 1": JLab averages about six sentences
  per work name, so a season level would hold almost nothing.
- **Naming them is the only part that isn't mechanical.** JLab's names come in variants, such as
  `Kanon 2006 (JP+Eng)` and `Kanon (2006) (Eng + Jp)`: they're the names of the subs2srs decks JLab took its cards from,
  with notes on their subtitles and sound. Claude would turn each into a work's name, matching Immersion Kit's ids and
  the works already in the corpus, such as `natsumeyuujincho1-6` into `natsume`, and list for Magnus every name that
  becomes each work, to correct before the migration.
- **Vocab and kanji have no work** (*decided*), other than the 未来のひとへ vocab. Which works a word appears in, the
  sentences already say.

## Not a provider or a work

- **`source::grammar`** (42 vocab, 7 kanji) is the vocabulary of grammar: 品詞, 他動詞, 連用形, Kindaichi's verb classes,
  and below it `_inflectable_form`, the names of the inflected forms, such as 未然形 and 終止形. *Decided:* another tag,
  if any, one about the words' meaning, such as `-::grammar-term`, alongside `-::counting`, `-::directional` and
  `-::formal-noun`. Their provider is whatever else they have (26 Yomitan, 10 WaniKani), or `jastudio` for the 6 with
  nothing else.
- **The provider's id and content** (`sourceData`): the external id, such as JLab's Anki note ids, Core 2000's iKnow ids
  (`sentence:250715`) or a subs2srs line's episode and time, and the question, answer and mnemonics as the provider gave
  them. It stays where it is: it's the provider's data.
- **The media's own origin** is a third axis, per media file: a WaniKani sentence's text is WaniKani's but its audio is
  TTS (`_tts_audio`, on 14,607 sentences); the dictionary vocab have TTS audio; some Yomitan vocab have audio clipped
  from One Week Friends. It belongs to the media (slice 9), as the dropped sidecars recorded it.
- **The priority tags** are set per part of a work (`-::priority::high::011_Natsume_01`), per provider
  (`110_core_2000`, the JLab decks) and per media origin (`120_tts`): all three axes matter for slice 5.

## Intellectual property (Magnus's view, which Claude shares; neither is a lawyer's)

- **Vocab and kanji raise no IP question themselves:** a word, its reading, its part of speech and its meaning are facts
  about the language, and the definitions are our own, written to fit how JAStudio displays and matches them.
- **What is the providers' own** is their mnemonics (WaniKani's, in `sourceData` on about 6,700 vocab and 2,000 kanji:
  never shown, but in the corpus files), their audio and their images. JMdict's text is CC BY-SA: sharing it requires
  attribution and the same licence.
- **One uncertainty:** the EU's database right protects against extracting a substantial part of a database, its
  selection and arrangement included. WaniKani's vocab with their levels, or Core 2000's steps, might count. If the
  corpus is shared, the provider's levels are what to drop: one more reason they sit under the provider's tag.
- **So the question is about sentences:** their text, translations, audio and screenshots.

## What the change would do to the code

- `JPNote.SourceTag`, read only for the search results' "Source" column, where it shows the shortest source tag, would
  become the work, or the provider for a note without one. `SourceTag` lives in `Storage.Media`, a leftover of the
  sidecars, though only `JPNote` uses it.
- `SentenceNote.SourceName`, its editor field and the import profile's `sourceName` mapping would go; an import
  profile's `tagsToAdd` would hold the provider's tag.
- `Tags.Source.Jamdict` would become the provider tag, `Tags.Source.ImmersionKit`, which nothing uses, would go, and so
  would `SourceTagJsonConverter`, a leftover of the sidecars.
- `VocabNoteFactory.CreateWithDictionary`, when the dictionary has no entry, and `VocabCloner` would tag
  `provider::jastudio`.
- The migration: rewrite the tags as in the tables, turn every `sourceName` into a work tag through the reviewed names,
  and give Immersion Kit's sentences their works from their ids.

## Fixed along the way

- The 233 Violet Evergarden sentences tagged with both episode 07 and 08 are episode 07's, by their external ids and
  audio, and no longer have the `::08` (f70ad52b5f).

## Still open

1. Tags with the three rules, as recommended?
2. `provider::subs2srs` for Magnus's own decks, as Yomitan is a provider?
3. No medium level in the works, and `current` dropped from WaniKani's sentences?
4. The tag for the grammar vocab, if any.
