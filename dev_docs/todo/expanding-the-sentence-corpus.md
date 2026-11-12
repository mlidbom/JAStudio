# Expanding the sentence corpus, intelligently

> **Status: a to-do of high importance (Magnus, 2026-11-11). The reservoir is built and merged into main (2026-11-11);
> no sentence has been taken from it into the corpus yet.** In his words: "We want our sentence corpus to
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
- **Memory, as sentence notes, is the limit:** today every sentence is a note held in memory, and a sentence note costs
  about 15.5 KB of process memory (measured 2026-11-11: 10.9 KB of live objects, about 4 KB of it each field's own lock,
  observable and actions). So 80 GB would hold about 5 million sentence notes, and the reservoir's 16.3 million would
  need about 250 GB.
- **A compact form for the reservoir holds it all in 4.5 GB, and loads in about a second** (measured 2026-11-12, with a
  throwaway prototype). Magnus asked how many sentences memory-efficient types could hold, with duplicate strings
  interned. The prototype held the reservoir in a few large arrays rather than an object per sentence: every sentence's
  text in one array of characters, its translation in one of UTF-8 bytes, and numbers for the rest, with media file
  names, subtitle file names, the words' forms and the sets of rejection reasons interned.
  - **Everything the reservoir file holds of the sentences** (text, translation, source, position, audio and image file
    names, occurrences): **1.74 GB, about 107 bytes a sentence.** The Japanese is 285 million characters, 17.5 a
    sentence, and UTF-16 suits it: two bytes a character, where UTF-8 takes three.
  - **With their bulk analyses as well** (195 million words and 126 million rejected vocab matches; 437,873 distinct
    forms, 406 distinct sets of reasons): **4.5 GB, about 277 bytes a sentence**, against the 7 GB the analyses take as
    MemoryPack records in the reservoir file.
  - **So 80 GB would hold about 290 million sentences with their analyses,** 18 times the reservoir, or about 750
    million without them. The raw tokens the value measure below would need aren't in the bulk analysis; at about ten
    a sentence, as numbers of distinct tokens, they'd add roughly 40 bytes a sentence (an estimate, not measured).
  - **Loading:** written as one file of those arrays (4.5 GB), it was read back into a fresh process in 1.2 seconds.
    The file had just been written, so the operating system probably served it from memory; from the NVMe disk, which
    reads several GB a second, it should take a few seconds. Built from the reservoir's SQLite file instead, it takes
    about a minute: 32 seconds for the sentences, 31 for the analyses.
  - **What it means:** memory stops being the limit once the reservoir's sentences are a read-only type of their own
    rather than sentence notes, and a sentence becomes a sentence note only when it joins the working set. The limit is
    then the time to analyze: 40 minutes for all 16 million today, of which 60% of the threads' time is spent waiting
    on two locks, the tokenizer's single shared tagger and the note caches' reads.
- **A second after each edit is within reach:** an edit to a vocab already reparses only the sentences that could
  contain its forms.

## How large the working set needs to be (Claude's thoughts, to discuss, 2026-11-12)

Magnus asked how large the corpus must be to build the vocab and the matching rules with confidence that the parsing
results are representative, given that too large a corpus could make a parsing change touch too many sentences to judge.

**The sentences per vocab, today** (each vocab's `sentenceCount`, 28,004 vocab, 585,368 vocab-sentence pairs):

| Sentences | Vocab |
|---|---|
| 0 | 2,669 (9.5%) |
| 1 | 10,314 (36.8%) |
| 2–4 | 7,276 (26.0%) |
| 5–9 | 3,263 (11.7%) |
| 10–29 | 2,573 (9.2%) |
| 30–99 | 1,200 (4.3%) |
| 100–999 | 636 (2.3%) |
| 1,000 or more | 73 (0.3%), up to 16,332 sentences, holding 45% of the pairs |

- **What decides confidence is the sentences per vocab, not the corpus's size.** 72% of the vocab have fewer than five
  sentences, too few to tell how their matching behaves; 73 have more than anyone can read. Words in text follow
  Zipf's law, so adding sentences at random grows both ends alike: ten times the sentences gives the 73 ten times as
  many, and most of the 10,314 vocab with one sentence still a handful.
- **So choose the working set from the reservoir by coverage:** a floor of sentences per vocab, where the reservoir has
  them, chosen to differ (forms, inflections, the words around them, sources) and to cover the vocab's senses, not only
  its matches (Magnus: see the value of senses below); and the sentences where a vocab's form appears but must not match,
  which the reservoir's rejected matches find (agreed with Magnus). The floor can follow the risk: a long kanji
  compound matches the same everywhere, and a few sentences show it; a short kana word, or one sharing its form with
  others, needs tens. With n sentences and no error seen, the error rate is likely below 3/n: 10 sentences show only
  that it's under 30%, 30 under 10%, 100 under 3%.
- **Representative frequencies come from the reservoir, not the working set:** the working set exercises each vocab and
  holds the confirmed answers; how often a change's effect occurs in real text is a question to ask the reservoir per
  change, as `what_a_vocab_does_in_the_reservoir` does for a vocab.
- **Judging a change depends on grouping, not on how many sentences changed.** Every added sentence holds particles and
  auxiliaries, so the frequent vocab grow with any growth, and they already have up to 16,332 sentences. A change to one
  of them is judged by its kinds of change, with counts and a few examples of each, and by the confirmed sentences, where
  any change is suspect. The kinds grow far more slowly than the sentences. Sentences whose matches are marked, confirmed
  right or wrong and with their sense, come first when the results are analyzed (Magnus, 2026-11-12).
- **The size, measured (2026-11-12):** `reservoir working-set-sizes` picks, greedily, the fewest reservoir sentences of
  up to 50 characters that give every vocab a floor of sentences, the corpus's own counting, as the report's picking does
  but for vocab alone:

  | Sentences per vocab | Taken in from the reservoir | Working set | Vocab the reservoir can't fill |
  |---|---|---|---|
  | 3 | 15,058 | 64,220 | 231 |
  | 5 | 31,287 | 80,449 | 616 |
  | 10 | 75,988 | 125,150 | 1,392 |
  | 20 | 170,328 | 219,490 | 2,559 |

  These are floors: each pick is the sentence giving the most vocab still lacking, so they take no account of varied
  contexts, senses or rejections, which would take more sentences. They come from the analyses of 2026-11-11, with the
  vocab as they were then, and leave out vocab the reservoir doesn't match at all. Even the 219,490 of a floor of 20
  would take about 1.1 GB as sentence notes (`dev_docs/memory-use.md`) and a full rebuild of about 30 seconds.

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
Magnus downloaded the whole folder on 2026-11-11 (with JDownloader and a MediaFire account), into his Dropbox
(`KEEP\Anki\SUBS2SRS\JLAB Decks Mediafire`) and a local folder.

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

## The reservoir, as built (Claude, the night of 2026-11-11)

Built on the branch `corpus-reservoir`, and merged into main on 2026-11-11. The data lives outside git, in
`C:\Dev\_JAReservoir\`, a few GB in all: the reservoir, the fetched databases, the tools and the reports. The full
decks, hundreds of GB, are to live where Magnus puts his download of the MediaFire folder.

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
  `SentencesWorthTakingIn` then picks, greedily, among sentences of up to 50 characters, the fewest giving the corpus
  what it lacks of each feature
  (`SentenceFeatures`): three sentences for each vocab, two for each vocab and rejection reason, one for each frequent
  word no vocab has (`SentencesWanted.AFewOfEach`). `ReservoirReport` writes `report.md` and a .tsv per list
  (`ReportFolder`). `working-set-sizes` picks the same way for a floor of sentences per vocab alone, and tells how many
  sentences each floor takes in.
- **Checking decks before filling** (`WhatADeckGives`, `DeckCheckReport`): what each note type of each deck in a
  folder would give, the fields recognized, the sentences, their length and examples, and which note types give
  nothing or hold words; to look at before filling the reservoir from decks nobody has looked at.
- **The analyses' format** is recorded in the reservoir file (`ReservoirFile.AnalysisFormat`): when it changes,
  analyzing forgets the analyses of the older one and makes them all anew, and nothing reads them meanwhile.
- **Run as batch commands of the headless JAStudio** (`ReservoirCommands`), from a copy of its build:
  `JAStudio.HeadlessHost reservoir check-decks | fill-decks | fill-subtitles | analyze | vocab | report | working-set-sizes`, with
  `--name value` arguments.
- **Fetching the MediaFire folder's databases only:** MediaFire answers range requests (HTTP 206), so a deck's
  collection database is read out of its zip without the media, a few MB of a deck's hundreds. A script did it for all
  1,382 decks, into `C:\Dev\_JAReservoir\mediafire\databases` (`C:\Dev\_JAReservoir\tools\fetch_mediafire.py`); Magnus is downloading
  the whole folder himself, with an account that allows it.

## Keeping the reservoir safe (agreed with Magnus, 2026-11-11)

The reservoir file is rebuilt from sources that sit in Dropbox, on MediaFire and on a USB drive. **There's no hurry,
though** (Magnus, 2026-11-12): every MediaFire deck is in three places, on MediaFire, where the folder has been for
many years, in his Dropbox and in a local folder. Storing the sources as JAStudio's own is about having them in a form
JAStudio and git can work with. Agreed:

- **The text goes in git, as the source of truth; the reservoir's SQLite file is an index derived from it.** Each
  source's sentences are kept faithfully, a line of JSON each, in a sentence note's shape without configuration or
  parsing result, a file per work, with everything there's any notable chance of needing: the deck's or subtitle
  file's identity, the note's raw fields and tags, a subtitle line's times, and the mapping from the deck's fields to
  the sentence's. A manifest per repository gives the format's version and what each collection is.
- **The format is meant never to change,** and history isn't vital: if it must change, a new repository replaces the
  old. A sentence JAStudio comes to care about is copied into the corpus, and lives on there.
- **Measured (2026-11-11):** the 16.3 million distinct sentences take 5.6 GB as such lines, 343 bytes each, and about
  0.95 GB in git; the MediaFire collection's 2.3 GB of lines packed to 415 MB. Every MediaFire deck's own database,
  without media, takes 1.13 GB in git, the largest 66 MB, so keeping the decks themselves, as well as the lines, is
  affordable too.
- **The media is stored once, normalized losslessly, named by its content's SHA-256,** and so immutable; the text
  refers to it by hash, and a list of every file's hash, kept with the text, says what must exist. Three copies: the
  working copy, a cloud object store holding a file per object for access, and a cold copy at another provider, in
  bundles of a few GB, since cold storage charges per object.
- **Measured (2026-11-11):** the MediaFire folder's 1,368 decks hold 515 GB of media in 13.1 million files: 7.5 million
  mp3s (415 GB), 5.4 million jpgs (79 GB), some png, ogg and wav, and 5.7 GB of the card templates' fonts and scripts.
  No video.
- **Built, and tried on five decks (2026-11-11): `reservoir import-decks`** (`DeckImporter`, in
  `JAStudio.Core.Reservoir.Importing`; the files' format and the media store are in `JAStudio.Core.Reservoir.Storage`,
  as the corpus has `Importing` and `Storage`. First called keeping, then archiving, to avoid the corpus's "import";
  Magnus found neither said what's done, 2026-11-11). For each deck it writes `decks/<package name>/` with the
  collection database as the package has it, `deck.json` (the package's name, length and SHA-256, each note type's
  mapping, and each media file's hash and the gain applied to it) and
  `notes.jsonl` (a line per note: its fields as they are, its tags, and the sentence it gives, in a sentence note's
  `sourceData` shape, with each media file's hash); and the media in a `MediaStore`. The trial decks: an audiobook
  (君の名は。), two uploaders' decks of Akame ga Kill, a Migaku deck (Kimetsu no Yaiba) and a deck in the latest package
  format (Chi). What it showed:
  - **Content addressing saves nothing on size:** of 60,563 media files, 60,550 are distinct, none shared between the
    decks, not even between the two decks of one show, whose clips are cut differently. Its worth is immutability and
    checking, not space.
  - **Normalizing changes the files a lot, and their size not at all:** 4,892 of about 30,000 MP3s were at the reference
    loudness; most moved by 1 to 5 steps of 1.5 dB, some very quiet ones by up to 43.
  - **It's bound by the CPU,** decoding the MP3s to measure them: 28 of 32 logical processors busy, about 22 MB a second,
    so about 6.5 hours for the folder's 515 GB.
  - **The text is small:** 55.9 MB for the five decks, 14.2 MB as git stores it.
  - **9 of about 30,000 MP3s couldn't be measured,** and threw: very short clips, which the decoder refuses, or which
    are shorter than ReplayGain's 50 ms window. Fixed (Magnus agreed): `Mp3File.MeasureReplayGainOrNone` says so, and
    such a clip is kept as it is, as mp3gain leaves a file it can't measure; the corpus's imports no longer fail on one either.
  - **Once a deck is imported, its files are the truth, and its package only a backup** (Magnus, 2026-11-11: "the id in
    the file wins once there is one"). The files are never written again from the package: a deck imported already is
    left as it is, so a run that was interrupted goes on where it stopped (`deck.json` is written last), and should the
    format have to change, the files are migrated into a new repository. So each note's id is simply a new GUID, the
    line's for good, and the link from a sentence taken into the corpus back to its line holds. Another package with the
    same file name, as two uploaders' decks can have, is refused rather than imported over. The media are listed in the
    media map's order, and every line ends with LF, on every platform.

## Magnus's direction for importing (2026-11-12)

- **A mind in the loop for every import, not a fully automatic one.** The reservoir's import so far detects each deck's
  layout and imports it unseen. Magnus: "I think I want a mind in the loop for every import." For each deck, someone,
  Claude or Magnus, looks at it with good tooling and decides:
  - whether it's essentially a duplicate of a deck imported already;
  - whether it holds data worth keeping that the automatic import doesn't handle;
  - whether the columns are mapped right;
  - whether some of its data needs adjusting to fit the data already imported.

  A deck that's a problem is set aside for discussion rather than imported in a way that may be wrong. About a thousand
  decks is a lot one at a time, but not an unreasonable amount of work with tooling, even for a human, and much less of
  one for Claude. The tooling is
  part of the work: the deck check (`reservoir check-decks`) and the field-layout mappings proposed on 2026-11-11 (the
  1,317 decks checked use only 187 distinct lists of fields; the top 40 give 88% of the sentences) are a start.
  Detection then only proposes; what is imported is what someone looked at.
- **Normalize the media on demand rather than at import.** Reconsiders the agreement above that the media is stored
  normalized. Magnus: "maybe do normalization on demand instead, caching the files if that is required for good
  latency behavior." The media would be stored as the decks have it, and a clip normalized when it's used, such as when
  its sentence joins the working set or is played, with the normalized file cached if that's needed for latency. What
  it would change, to weigh when it's decided:
  - The import gets much faster: measuring the MP3s' loudness, which normalizing needs, is what made the trial bound by
    the CPU, about 6.5 hours for the 515 GB.
  - The stored media is then the original, byte for byte, so a hash of it can be checked against the deck it came from.
  - The normalizing code and its decisions stay changeable, as nothing stored depends on them.
- **Sentences without a translation: find out, think it over, discuss later.** Magnus's first thought was to import only
  sentences that have both the Japanese and a translation; his second, that we may want them after all, as long as
  they're easy to filter out. What the reservoir holds (measured 2026-11-12): **1,874,063 of its 16,293,250 sentences
  have a translation.**
  - The subtitles have none: kitsunekko's 10,815,323 and the Dropbox folder's other 50,281.
  - The MediaFire decks: 1,717,598 of 5,271,175, a third.
  - The Dropbox folder's decks: 156,465 of 156,471.

  Some of the untranslated deck sentences may come from note types whose translation field wasn't recognized, rather
  than from decks without translations; the deck-by-deck review above would tell.

## What "intelligently" could mean (ideas, to work out with Magnus)

- **A sentence's value is in how rare what it holds is** (Magnus, 2026-11-12, brainstorming). The rarer the tokens and
  matches found in a sentence, the more valuable it is. Two things to look at, at least:
  1. **The tokens the tokenizer gives, without any processing by JAStudio's code:** rare parts of speech, inflection
     types and forms, and rare sequences of them.
  2. **The matches the full parsing makes:** rare vocab, rare kinds of match, and rare reasons for rejecting a match.

  The bulk analysis keeps the second (the words and the rejected vocab matches), but not the first: the raw tokens would
  have to be kept as well. The ideas below are ways of making it concrete.

- **Which sense matches, not only which vocab** (Magnus, 2026-11-12): "It is not just about which token or vocab
  matches, it is which sense matches. Rare sense matches are valuable." The same word in an unusual sense is worth more
  than another sentence with its common one, and doing this well "requires a mind to look at the sentences in most
  cases": a vocab's matches picked at random give its common sense over and over. His hand-made highlights often mark
  such a sense. Marking senses in sentences is to be designed: [annotating-sentences.md](annotating-sentences.md).
  - **Varied contexts as a proxy at first:** different forms, inflections, surrounding words and sources stand in for
    different senses well enough for an automatic first selection, before agents go over the sentences in loops and
    judge by hand what to bring in, which Magnus wants eventually.

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
