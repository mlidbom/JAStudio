# Expanding the sentence corpus, intelligently

> **Status: a to-do of high importance (Magnus, 2026-11-11). Not started.** In his words: "We want our sentence corpus to
> be about as big and diverse in type of sentences as we can make it, so expanding on it, and doing so intelligently,
> bringing in high value sentences, is a super important TODO item."

## Why

The goal of matching is sentences that parse right without corrections on the sentence (Magnus, 2026-11-10/11). Vocab
are adjusted by hand as a matter of course, through their match-conditions; sentences shouldn't have to be. The vocab
are finite and the sentences out there unlimited, so every condition that describes the language correctly pays off in
every sentence JAStudio's users will ever bring.

Everything that measures and steers that work runs over the sentence corpus (see
[matching-configuration.md](../matching-configuration.md), the yardstick):
- **The yardstick** counts the sentence corrections a rule change makes unnecessary.
- **The generality measure** counts, for each condition in a vocab's match-condition, the matches it decides across
  the corpus. It tells a rule about the language from a patch for one sentence. It also tells a condition worth
  sharing among vocab from one that shouldn't exist at all, because a correction on its one sentence would do.
- **The full rebuild's diff** shows what a change does to every sentence.

Each is only as good as the corpus it runs over (see also [improving-parsing.md](../improving-parsing.md)). A rule tested on sentences of one kind can't be told apart from a patch
for that kind. The corpus today has 49,162 sentences, and on 2026-11-10 two of today's rules, godan and godan imperative
(head), were set only on vocab that never match in it.

## A reservoir and a working set (Magnus, 2026-11-11)

Growing the corpus by one or two orders of magnitude would be fantastic, but parsing all of it, and judging what a
change did to it, would then slow every change down. So, in Magnus's words: "rather than being very selecting in what
we import, we could import a ton of more subs2srs decks, but a flag on the notes would include/exclude them from the
standard parsing, maybe even standard loading at all. That way we could have an enormous treasure trove of sentences
ready to work on when we need them."

- **The reservoir:** every sentence imported, not loaded or parsed in normal use.
- **The working set:** the sentences JAStudio loads and the full rebuild parses, as all of today's are.
- **Choosing the working set:** a separate mode or function loads and parses the whole reservoir, however slowly, and
  runs selection over it, which moves sentences into the working set, or out of it, by how valuable they are. What
  "valuable" means is still to be decided: the ideas below are candidates.
- **The names were agreed with Magnus (2026-11-11):** the reservoir, the working set, and the confirmed sentences in it.
  His first guesses were unused, active and validated.

**Near-instant feedback.** With the sentences sorted into such sets, "we could also have all known to be important
sentences reparse automatically within a second or so of any edit, giving near instant feedback on the effects on
parsing" (Magnus). The confirmed sentences of [improving-parsing.md](../improving-parsing.md) are such a set: an edit to a
vocab reparses those it can affect, and says at once what it broke and what it fixed.

**What the numbers say so far:**
- **Parsing:** a full rebuild of today's 49,162 sentences takes 6.5 seconds, so about 0.13 ms a sentence. Ten times as
  many would take about a minute, and a hundred times about 11 minutes: fine for a selection run now and then.
- **Memory, not time, is the limit:** today every sentence is a note held in memory. Millions wouldn't fit as notes, so
  the reservoir is probably best kept as plain sentences with their source, without parse results, and without media
  until a sentence joins the working set.
- **A second after each edit is within reach:** an edit to a vocab already reparses only the sentences that could
  contain its forms.

## The first sources: Magnus's subs2srs folder (surveyed 2026-11-11)

Magnus's `Dropbox\KEEP\Anki\SUBS2SRS`, 29 GB. Sentences are counted after removing whitespace and markup, and against
the corpus's 46,986 distinct sentences. Matching on text undercounts what the corpus already has where its sentences
were edited: Girls' Last Tour, whose 2,758 sentences are all in the corpus, counts 1,720 "new". So the new counts are
upper bounds.

- **Decks (.apkg), with audio, screenshots and English: 121,587 new sentences** from 26 shows and films, 2.6 times the
  corpus. The largest are Legend of the Galactic Heroes (25,297), Polar Bear Cafe (13,344), March Comes In Like a Lion
  (7,269), Fruits Basket (7,245), So I'm a Spider, So What? (6,760), Teasing Master Takagi-san (6,701), Yuru Camp
  (5,719) and Carole & Tuesday (5,367).
  - **`Unprocessed/` and `Unprocessed/Imported/`** hold the decks as downloaded, in six note types: subs2srs,
    subs2srs-japanese, Japanese Jo-Mako Morphman Audio, Japanese Morphman Audio Ankiweb, Migaku Japanese and Japanese
    Sentences Listening 2020. All are in Anki's older package formats: a plain SQLite database and a JSON media map.
  - **`Processed/`** holds 15 of them converted in May 2024 to the Python-era JAStudio note type `_japanese_sentence`
    (98,411 notes, in Anki's newest, zstd-compressed format), and `__original_pack.apkg` holds all 15 together. Only
    Natsume and Violet Evergarden of them are in the corpus. Their originals are in `Unprocessed/Imported/`.
  - **What needs cleaning:** speaker names such as （ユーリ）, `&lrm;` marks, several lines joined into one, ideographic
    spaces.
  - **The Anki profiles `subs2srs` and `processed-subs2srs`** hold the 2024 imports.
- **The kitsunekko backup (`SubsOnly/kitsunekko_backup.zip`, 4.9 GB): the ocean.** Japanese subtitles of 1,871 shows:
  25,323 subtitle files (.srt, .ass) and 3,291 archives (.zip, .rar, .7z). A sample of 400 files held 552 Japanese lines
  a file, so the loose files hold about 14 million lines, before removing the duplicate versions of an episode. Text
  only: no audio and no translation. The sample's files were all UTF-8 or UTF-16.
- **`SubsOnly/`** also holds subtitles for 8 shows, some of them the decks' shows.

## The real treasure trove: the "User Uploaded Anki Decks" folder (Magnus, 2026-11-11)

<https://www.mediafire.com/folder/p17g5uk4phb41/User_Uploaded_Anki_Decks>. Its listing, through MediaFire's public API
on 2026-11-11 (`folder/get_content.php?folder_key=p17g5uk4phb41`): **1,404 files, 517 GB, 1,382 of them .apkg decks.**
Nothing was downloaded yet.

- **168 audiobooks:** novels and some non-fiction, a sentence a note with the narrator's audio. Narration and written
  Japanese, the registers the corpus lacks most: 狼と香辛料 1–8, 本好きの下剋上, 無職転生, Overlord, コンビニ人間, 君の名は。,
  かがみの孤城, 1日1ページ、読むだけで身につく世界の教養365, and many more.
- **About 1,200 subs2srs decks:** anime (Death Note, Clannad, Evangelion, One Piece, Haikyu, Ranma ½ …), anime films
  (Akira, Ponyo, Porco Rosso …), live-action drama and reality TV (半分、青い。, まだ結婚できない男, Terrace House, ドクターX),
  games (Final Fantasy VII Remake, Final Fantasy X, Ace Attorney), and a YouTube channel (ジオヒストリー).
- **"00 - 11.10 UPDATE - read before downloading.txt"** and some READMEs: to read before taking anything.
- **Only the text may be needed at first:** an .apkg is a zip whose database is a few MB of its hundreds. If MediaFire's
  downloads allow range requests, as download managers' resuming suggests, a deck's database can be read without its
  media, about 4 GB for all 1,382 rather than 517. Not tried yet.

## The reservoir, as built on the branch `corpus-reservoir` (Claude, the night of 2026-11-11)

Built on a branch of its own, in the worktree `C:\Dev\JAStudio.worktrees\corpus-reservoir`, so that Magnus can choose
whether to merge it. The data lives outside git, in `C:\Dev\_JAReservoir\`, a few GB in all: the reservoir, the fetched databases, the tools
and the reports. The full decks, hundreds of GB, are to live where Magnus puts his download of the MediaFire folder.

- **Reading the sources, with no Anki** (`JAStudio.Core.Reservoir.Sources`):
  - `AnkiPackageDatabase` reads an .apkg's collection database, or one taken out of its package, in all three of Anki's
    formats (`collection.anki2`, `anki21`, and the zstd-compressed `anki21b`), as the import's own `RawNote`s and
    `AnkiNoteType`s. That's the ground for an `IAnkiRawNotes` reading a package, so the import profiles could import a
    deck file with no Anki profile for importing.
  - `DeckFieldRoles` recognizes which field holds the Japanese, the translation, the audio and the image, by known names
    such as Expression and source_question, and else by what a sample of the notes hold (`FieldContents`). It recognized
    all 31 of the Dropbox folder's decks right, across eleven note types.
  - `SubtitleFile` reads .srt, .ass and .ssa, in UTF-8, UTF-16 or Shift JIS; `SubtitleCollection` reads a folder per
    show, zipped as the kitsunekko backup is, or not, zips inside included.
  - `ReservoirSentenceText` cleans a sentence of what its source wraps it in: HTML, ruby, direction marks, subtitle
    formatting, speaker names in brackets such as （ユーリ）, music notes; half-width katakana become full-width.
- **The reservoir itself** (`ReservoirFile`): a SQLite file of distinct sentences, each with the source it was first
  found in (or the first with its audio), where in it, its translation and media file names, and how often it was found.
  A work is added once (`ReservoirFilling`), so filling can stop and start again.
- **The bulk analysis** (`JAStudio.Core.Reservoir.Analysis`): every sentence analyzed as a stored parsing result is made
  (`TextAnalysis`, no configuration), with the corpus's vocab, about 6,000 sentences a second in parallel. Each analysis
  is kept as a MemoryPack record (`ReservoirSentenceAnalysis`): its words (valid matches and displayed ones) and the vocab
  matches it rejected, with why, vocab and failure reasons numbered (`ReservoirAnalysisNumbers`, in `.Analysis.Numbering`)
  to keep it small.
- **The measures, the picks and the report** (`JAStudio.Core.Reservoir.Measures`, `.Picking` and `.Reporting`): one pass
  over the analyses counts, with examples, the sentences matching each vocab, each vocab's rejections by reason, and each
  word no vocab has (`ReservoirMeasures`); the corpus is counted the same way (`WhatTheCorpusHas`).
  `SentencesWorthTakingIn` then picks, greedily, the fewest sentences giving the corpus what it lacks of each feature
  (`SentenceFeatures`): three sentences for each vocab, two for each vocab and rejection reason, one for each frequent
  word no vocab has. `ReservoirReport` writes `report.md` and a .tsv per list (`ReportFolder`).
- **Run as batch commands of the headless JAStudio** (`ReservoirCommands`), from a copy of its build:
  `JAStudio.HeadlessHost reservoir fill-decks | fill-subtitles | analyze | report`, with `--name value` arguments.
- **Fetching the MediaFire folder's databases only:** MediaFire answers range requests (HTTP 206), so a deck's
  collection database is read out of its zip without the media, a few MB of a deck's hundreds. A script did it for all
  1,382 decks, into `C:\Dev\_JAReservoir\mediafire\databases` (`C:\Dev\_JAReservoir\tools\fetch_mediafire.py`); Magnus is downloading
  the whole folder himself, with an account that allows it.

## What "intelligently" could mean (ideas, to work out with Magnus)

- **Diversity of kind.** The corpus is mostly a few anime series and course decks (Core 2000, WaniKani, JLab, Immersion
  Kit). Other registers and genres would test the rules where they're weakest: conversation, narration, news, writing.
- **Sentences that exercise what the corpus doesn't:** grammar patterns that are rare in it, such as sequences of parts
  of speech and inflection forms; vocab with few sentences; rules and conditions that decide few matches.
- **Sentences where parsing is contested:** several vocab competing for a word, words only the dictionary or nothing
  matches. These are where conditions are tested hardest.
- **A measure of a sentence's value:** what it adds to the corpus's coverage, so that candidates from a large source
  can be ranked, and the best brought in first.

## Questions for when the work starts

- **Do sentences brought in for coverage become study notes, or live in the corpus without cards?** Which is right
  depends on how Magnus wants to use them.
- **Sources, and their licenses:** the corpus is meant to serve other users too, so a source's license must allow that.
  Candidates to check include Tatoeba and public-domain literature.
- **Who checks how a new sentence parses?** Its parse is unverified until someone looks. The measures above say where
  to look first.
