# Whether to split the source tags into provider and work

> **Status: deferred, and undecided whether to do it at all (Magnus, 2026-11-10).** This was slice 4c of
> `corpus-data-model.md`. Everything learned while working it out is here, for when it comes up again. The counts are from
> the corpus at 83f0b2b734, surveyed with the MCP server's `get_the_tag_tree`.

## Magnus's doubts

In his words (2026-11-10):

- "I think we will defer the move to provider/work for later, potentially. I'm still unsure of whether I like the change
  and it is worth it."
- "Keeping source very vague is actually sort of helpful for me. Splitting it would not necessarily help me even though I
  find it hard to summarize why."
- "I agree on untangling the confusion of source meaning more than one thing.... Potentially at least... But sometimes tags
  that can hold almost anything is a good tool."

So `source::` stays as it is for now: one vague, hierarchical branch that can hold whatever helps Magnus organize. The
changes to the data that don't depend on the split are listed [at the end](#data-changes-that-dont-depend-on-the-split).

## The question

On 2026-11-08 Magnus decided to "untangle the provider from the work in the data". `source::` holds two kinds of thing:

- **where a note came from**, such as `source::wani::level::19` or `source::jlab::beginner`;
- **which work its content is from**, such as `source::anime::natsume::s4::11`.

For Magnus's own anime decks only the work is there. For JLab and Immersion Kit sentences the two differ: the tag names
the deck, and the work is elsewhere. JLab's is in the sentence's `sourceName` field, such as `Shirokuma Cafe`, and
Immersion Kit's only inside its external id, such as `anime_shirokuma_cafe_000012345`.

## What `source::` holds today

| Branch | Vocab | Kanji | Sentences | What it is |
|---|---|---|---|---|
| `source::anime::natsume` | | | 20,028 | Magnus's subs2srs decks: `s1`–`s6`, `ova1`, `ova2`, `nyhnme1` (a deck of 258 lines the data doesn't identify), then episodes |
| `source::anime::violet` | | | 3,127 | the same, by episode |
| `source::anime::one-week` | | | 2,780 | the same, and `songs::op`, `songs::ed` |
| `source::anime::girls-last-tour` | | | 2,758 | the same, and `songs::op`, `songs::ed`, `songs::final-ending` |
| `source::jamdict` | 12,775 | | | vocab JAStudio created from the dictionary, JMdict (`VocabNoteFactory.CreateWithDictionary`) |
| `source::wani::level::NN` | 6,629 | 2,076 | | WaniKani, by level |
| `source::wani` and `source::wani::current` | | | 14,567 | WaniKani's context sentences |
| `source::wani::removed` | | | 30 | sentences no longer on WaniKani: likely WaniKani found something wrong with them |
| `source::yomitan` | 5,567 | 396 | | mined by Magnus with Yomitan |
| `source::core2000::stepNN` | 584 | | 1,996 | the Core 2000 deck, by step |
| `source::jlab::beginner`, `::inter`, `::booster::1`/`::2`, `::num` | | | 3,011 | JLab's decks. All 447 booster sentences are in the intermediate deck too: the decks' structure, not a mistake |
| `source::immersion_kit` | | | 781 | Immersion Kit |
| `source::lyrics::未来のひとへ` | 50 | | 33 | a song |
| `source::grammar`, `::_inflectable_form` | 42 | 7 | | grammar terms: see the data changes below |
| `source::free::misc`, `source::misc` | | | 39, 7 | unknown, added by hand; `free` was the licence regime, not a source |
| `source::tatoeba` | | | 3 | Tatoeba |
| no source tag | 2,449 | 82 | 2 | our own: see the data changes below |

Nothing in JAStudio's logic reads the source. It's only shown: the search results' Source column and a vocab's sentence
list show `JPNote.SourceTag`, the shortest `source::` tag a note has, so a JLab booster sentence shows `jlab::inter`.

## The idea, as discussed

Two concepts, in Magnus's words: **the provider** is where we got it from, "an organization, website, service etc", and
**the work** is "which work it is from", where "season/episode/volume etc can also go".

The data fits that. One work arrives through several providers: Natsume through Magnus's decks and through JLab
(`natsumeyuujincho1-6`), and Shirokuma Cafe, Clannad After Story and Fruits Basket through both JLab and Immersion Kit.
And one provider brings many works: JLab's four decks name works 717 times (beginner 287, inter 203, booster 143, num 84,
overlapping), and Immersion Kit's ids about 227 times.

### What the discussion settled, should the split happen

- **Tags, not structured storage:** two branches, `provider::` and `work::`, with everything now below `source::` moved
  under one of them as it is, levels and names included. Most of that is a few whole-branch renames in the tag
  management.
- **The providers:** WaniKani, Core 2000, JLab, Immersion Kit and Tatoeba. Also:
  - `jmdict`, for the vocab JAStudio created from the dictionary;
  - `yomitan`, though the dictionaries behind it would be the real providers if they had been recorded;
  - `jastudio`, for what we made ourselves, Magnus or Claude.
- **Not a provider:** subs2srs, "a technology, not a provider" (Magnus). For anime the work is what matters, and what
  matters for intellectual property; with no coherent provider, a note simply has none. Unknown is the absence of a
  provider tag.
- **The medium level stays:** `anime::`, `lyrics::`, `songs::`, and `drama::` for JLab's dramas, such as 結婚できない男.
  Magnus finds the grouping helpful, and Claude's suggestion to drop it came from thinking of a work as a thing with
  properties, as structured storage would, which contradicts tags.
- **JLab's and Immersion Kit's works get the overarching work only:** JLab's names give episode ranges, such as
  "Naruto Season 2 (26-52)" and "Gintama (202-265)", never episodes. Naming them is the only part of the move that
  isn't mechanical. JLab's names are those of the subs2srs decks it took its cards from, in variants such as
  `Kanon 2006 (JP+Eng)` and `Kanon (2006) (Eng + Jp)`. Claude would turn each into a work's name, matching Immersion
  Kit's ids and the works already in the corpus, and list for Magnus every name that becomes each work, to correct.
- **Vocab and kanji have no work,** "in terms of anime seasons etc" (Magnus), apart from the 未来のひとへ vocab.
  Which works a word appears in, the sentences already say. A dictionary could be a vocab's work, with its publisher
  the provider, but with our definitions rewritten anyway, nothing would use it.
- **WaniKani's `removed` stays a tag.**

### What it would achieve

1. **Every sentence findable, and prioritizable, by its work, whichever deck it came through.** For 3,792 sentences
   the work is in no tag today: JLab's 3,011 and Immersion Kit's 781.
2. **One place for the work.** `sourceName` holds a work's name for JLab, a subs2srs deck's name for One Week Friends
   (`Isshuukan_Friends_(Test)_EE`), and nothing for every other sentence.
3. **Each branch meaning one thing,** for code that reads it: the Source column could show the work, slice 5's leaning
   is a priority table keyed by tag branches, and sharing the corpus would need a licence per provider. With
   `source::` holding both, code would have to know which top-level names are works.

### What it would cost

- The renames: mechanical.
- The code: `SentenceNote.SourceName`, its editor field and the import profile's `sourceName` mapping go; the Source
  column shows the work; an import profile's `tagsToAdd` holds the provider's tag; vocab created in JAStudio get
  `provider::jastudio`.
- Naming JLab's and Immersion Kit's works, as above.

### A minimal alternative, without the split

Gains 1 and 2 without touching the rest: give JLab's and Immersion Kit's sentences their works as tags in the existing
branch, such as `source::anime::shirokuma-cafe`, and drop `sourceName`. A JLab sentence would then have two `source::`
tags, its deck's and its work's, so code reading the source would need to know which is which.

## Tags or structured storage

Structure pays only where code acts on the data, and nothing needs more than tags now:

- **Sharing the corpus or gating access** (`media-storage-and-import.md`) would need each provider's licence: a small
  list keyed by the provider's tag. Notes keep their tags.
- **Syncing with a provider** needs the provider's id for the note, which is already a field, `sourceData.externalId`.
- **Story order, or replaying a scene,** needs the time within the episode, which is already in the external ids and
  audio file names, such as `A_VioletEvergarden_07_0.19.34.670-…`.
- **Linking works to AniList, AniDB or MyAnimeList** would be a mapping from work tag to their ids, which also absorbs
  how differently they classify seasons, OVAs and specials. As Claude understands them, AniList and MyAnimeList give
  each season an entry of its own. The tags stay Magnus's judgment.
- **Priority (slice 5)** could be rules keyed by tag branches, which searches already express.

Target principle 3 of `corpus-data-model.md`, "tags hold only labels the domain does not interpret", was aimed at
settings and micro-syntax hidden in tags, as the matching flags were. Code asking which group a note is in, such as "is
it in this work's branch?", is what tags are for. The cost is that code depends on a few path names staying put.
Should the split happen, the principle would be reworded to say so, and open question 9 closed the same way.

## Other aspects of where content comes from

The discussion found more than provider and work in and around `source::`:

- **The provider's own structure:** WaniKani's levels, Core 2000's steps and JLab's decks. A curriculum position,
  not a work: a JLab sentence has both a deck and a work. It sits below the provider.
- **The time within an episode:** in the external ids, too fine for a tag.
- **The provider's id and content** (`sourceData`): JLab's Anki note ids, Core 2000's iKnow ids (`sentence:250715`),
  a subs2srs line's episode and time, and the question, answer and mnemonics as the provider gave them.
- **The licence regime:** `free::` in `source::free::misc`, and the media layer's `free`, `commercial-01` and
  `commercial-02`. It follows the origin of each piece of content, not the note.
- **The media's own origin,** a separate axis, per media file:
  - A WaniKani sentence's text is WaniKani's, but its audio is TTS (`_tts_audio`, on 14,607 sentences).
  - The dictionary vocab have TTS audio.
  - Some Yomitan vocab have audio clipped from One Week Friends.

  It belongs to the media, slice 9, as the dropped sidecars recorded it.
- **The priority tags** are set along all three axes: per part of a work (`-::priority::high::011_Natsume_01`), per
  provider (`110_core_2000`, the JLab decks) and per media origin (`120_tts`).

## Intellectual property

Magnus's view, which Claude shares; neither is a lawyer's:

- **Vocab and kanji raise no question themselves:** a word, its reading, its part of speech and its meaning are facts
  about the language, and the definitions are our own, written to fit how JAStudio displays and matches them.
- **What is the providers' own** is their mnemonics (WaniKani's, in `sourceData` on about 6,700 vocab and 2,000 kanji:
  never shown, but in the corpus files), their audio and their images. JMdict's text is CC BY-SA: sharing it requires
  attribution and the same licence.
- **One uncertainty:** the EU's database right protects against extracting a substantial part of a database,
  including its selection and arrangement. WaniKani's vocab with their levels, or Core 2000's steps, might count. It
  matters only if the corpus is shared.
- **So the question is about sentences:** their text, translations, audio and screenshots.

## Code noticed along the way

- `SourceTag` lives in `Storage.Media`, a leftover of the dropped media sidecars, though only `JPNote` uses it.
- `SourceTagJsonConverter` is another leftover of the sidecars, used by nothing.
- *Removed in b319cbb63:* `Tags.Source.ImmersionKit`, used by nothing.
- *Fixed in b319cbb63:* `VocabNoteFactory.CreateWithDictionary`, when the dictionary has no entry, and `VocabCloner`
  created vocab with no source tag. That is most likely where the 2,449 untagged vocab came from: their many derived
  forms, such as あせちゃう and あせてもらう, suggest the cloner's derived versions.

## Data changes that don't depend on the split

**Done (2026-11-10)**, each a corpus commit of its own, after a full rebuild that changed nothing:
- The 233 Violet Evergarden sentences tagged with both episode 07 and 08 are episode 07's, by their external ids and
  audio, and no longer have the `::08` (f70ad52b5f).
- The kanji no longer have `source::lyrics::未来のひとへ`; its 50 vocab and 33 sentences keep it (83f0b2b734).
- The kanji of the grammar terms, 基, 形, 格, 然, 般, 詞 and 類, no longer have `source::grammar` (9ef6a36bfd).
- The 42 grammar terms' vocab have `-::grammar-term` instead of `source::grammar`, and the 6 names of the inflected forms
  `-::grammar-term::_inflectable_form` instead of `source::grammar::_inflectable_form` (6354a4cab2). Magnus: "These
  should be given another tag, if any. This is more about the meaning of the words". Each has a Yomitan or WaniKani
  source tag too.
- Our own content has `source::jastudio` (Magnus: "sounds good"): the 2,449 vocab and 82 kanji that had no source tag
  (88c94920a8). The 82 kanji are radicals and components made by hand, such as ⺌ and ナ, many with WaniKani's radical
  names, such as triceratops and narwhal.
- Girls' Last Tour's 2,758 sentences have the priority tag `-::priority::high::011_Girls_Last_Tour` instead of Natsume's
  season 1's, `011_Natsume_01`: the same priority, named for the right work (c18697b4d6).

**Done in the code (b319cbb63):** what JAStudio creates as our own content is tagged `source::jastudio`: a vocab the
dictionary has no entry for, a vocab derived from another, such as its suffix version, and a kanji created by hand, as
the note menus' "Create" does (`KanjiNote.CreateByHand`). A clone (`VocabCloner.Clone`), a full copy carrying the
original's source data, keeps the original's source tags instead: until then it had none.

**Decided to keep:** `source::free::misc` stays apart from `source::misc` (Magnus: "Licensing is most of why we have
this data, so keep it for now").

**Open:**
- ~~**`source::jamdict` and `source::jastudio` count as tags JAStudio defines**, so they couldn't be added or removed by
  hand, and the `source` branch couldn't be renamed or deleted.~~ *Done 2026-11-10: they're added and removed as any
  tag, with a warning, as every change to a note's source now has; only their names can't be renamed; and `source`
  itself is never renamed or deleted, by a rule of its own. See `tags.md`, Rules.*
- **JLab's and Immersion Kit's works as tags, and `sourceName` gone:** the minimal alternative above. Magnus isn't sure
  of the value: "currently anime:: holds only full seasons and episodes. This would make it hold tons of tiny
  fragments with individual sentences.... Not sure I want/need that." If it's done, under a branch of their own so as
  not to clutter `source::anime::`, such as `source::fragments::anime::<work>` and `source::fragments::jdrama::<work>`.
  It needs the review of the work names.
- **Duplicate sentences:** 71 of JLab's 74 Natsume sentences (`natsumeyuujincho1-6`) are lines also in Magnus's own
  Natsume decks, and across the corpus 525 sentence texts of more than 4 characters are in 1,380 sentences. Knowing a
  sentence's work is what found the Natsume ones. Not a reason to remove duplicates (Magnus, 2026-11-10): "many sentences
  mean slightly different, or completely different, things in different contexts or with different tones of voice", and
  studying a work, his first recommendation for studying, needs every sentence of that work, however often the same
  sentence occurs elsewhere. Keeping the corpus from growing unbounded may still be needed some day. The only plainly
  redundant case is the same line of the same work arriving through a second deck, as JLab's Natsume lines do.
