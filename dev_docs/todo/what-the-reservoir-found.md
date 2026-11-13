# What the reservoir found (Claude, the night of 2026-11-11)

> **Status: findings for Magnus to look at, from the first runs of the reservoir** (see
> [expanding-the-sentence-corpus.md](expanding-the-sentence-corpus.md), "The reservoir, as built"). Nothing here changed
> the corpus or matching on `main`. Each finding says what was seen, with examples, and what could be done about it.
> The reports themselves are in `C:\Dev\_JAReservoir\reports\`; the full one is `full-2026-11-11`.

## The reservoir, at the end of the night

**16,293,250 distinct sentences, every one analyzed,** against the corpus's 49,162; **5,220,934 of them with audio.**

| Collection | Works | Sentences read | New to the reservoir | Kept from it |
|---|---|---|---|---|
| The kitsunekko backup's subtitles | 1,871 | 23,785,426 | 12,839,146 | 10,815,323 |
| The MediaFire folder's decks | 1,383 | 7,827,302 | 3,224,822 | 5,271,175 |
| The Dropbox folder's decks | 31 | 170,361 | 156,475 | 156,471 |
| The Dropbox folder's other subtitles | 7 | 178,571 | 72,807 | 50,281 |

A sentence found again keeps the source that has its audio, so the MediaFire decks, read last, took over 2 million
sentences first read from subtitles: 5,078,381 of the 5,271,175 kept from them have audio. The kitsunekko sentences
have none. Analyzing all 16 million took about 40 minutes, with the MediaFire fill running alongside, and the full
report takes 5.

## Bugs found and fixed on the branch

- **Analysis crashed on any sentence with a classical verb.** The tokenizer's dictionary, IPADIC, has 58 inflection
  types, and JAStudio knew 48. A token of one of the other ten, such as 下二・ガ行 (捧ぐ, 見上ぐ) or 文語・マジ, threw
  `KeyNotFoundException` in `InflectionTypes.GetByName` (now `UniDicInflectionTypes`), failing the whole sentence. None of the corpus's sentences has
  one; the first bulk analysis of anime subtitles hit one within 80,000 sentences. All ten, and the inflected form
  現代基本形, are added (`LowerNidan`, `UpperNidan`, `Yodan.SaEnding`, `Yodan.TaEnding`, `Bungo.Maji`), and the spec
  `When_the_tokenizer_gives_a_word_of_any_inflection_type_its_dictionary_has` reads every type and form from the
  dictionary itself, so a dictionary update can't bring the crash back unseen.
- **The bulk analysis took rejected matches for matches.** It kept each sentence's valid matches together with those the
  sentence breakdown displays, and the breakdown displays a rejected match when nothing else covers its word
  (`Match.IsEmergencyDisplayed`). So たまえ counted as matching in 落ち着きたまえ, where its match-condition rejects it:
  the measures' sentences matching each vocab and the picks counted such sentences, and the vocab feedback said 777
  sentences with たまえ "no longer match" since the bulk analysis, though nothing had changed. Each word now keeps
  whether it's a valid match (`ReservoirWord.IsValid`), and the reservoir file records its analyses' format, so the
  analyses of the older one were forgotten and made anew; a newer one is neither read nor written to.
- **Foreign subtitles misread as Shift JIS counted as Japanese.** kitsunekko's Portuguese and Italian subtitles for
  Dragon Ball Z: Resurrection F and Kaze Tachinu have ・ for their accented letters (「J・esperava por isso.」), and ・
  was counted as kana. It isn't, now; the sentences already in the reservoir stay until it's filled anew.
- **The picks were paragraphs.** The greedy pick takes the sentence giving the most features the corpus lacks, and an
  audiobook's paragraph gives the most of all: 1,995 of the 2,000 picks were longer than 60 characters, such as a
  bibliography. Only sentences as long as the corpus's are candidates now, up to 50 characters: the corpus's are 15 long
  at the median and 50 at the 99th percentile. The 2,000 picks now give 15,763 things the corpus lacks, and 1,703 of
  them have audio.

## Reading the MediaFire folder's decks

`reservoir check-decks` reads every deck in a folder without filling anything, and reports what each note type would
give: the fields recognized, the notes, the sentences, their average length and examples
(`C:\Dev\_JAReservoir\reports\deck-check-mediafire\`). Its first run over the 980 decks fetched by then showed the field
recognition, built on the Dropbox folder's 31 decks, wrong for several kinds of deck:

- **Migaku's decks give their sentences only with furigana,** as `急[いそ,いそぐ;k2]い でる[,でる;k1] から`, which ruled
  the field out, so a deck gave nothing, or its Target field, a single word: Kimetsu no Yaiba, Charlotte, Baccano!,
  Itazura na Kiss, Seraph of the End and others, about 41,000 sentences. The furigana are now taken away, Anki's and
  Migaku's.
- **SentKanji,** the sentence field of a common sentence-mining note type, was ruled out by the kanji in its name.
- **Decks with their fields swapped,** such as BLAME!, whose Japanese is in Meaning.
- **Vocabulary decks** (the iKnow decks, a katakana deck) gave words as sentences: a note type whose Japanese averages
  under 6 characters now gives none.
- **Tags and sequence markers were taken for translations,** such as `Cardcaptor_Sakura_46_01` in a field named Tag: a
  field found to hold translations by what it holds must hold prose, several words to a value.

What gives nothing now has a reason: Korean dramas, subtitles kept as images (Spirited Away, Whisper of the Heart),
and decks in English or Spanish.

## What the full report found

### Kana spellings the dictionary doesn't find: a quarter of the missing words

The missing words, those neither a vocab nor the dictionary has, are displayed in 5.9 million of the 16.3 million
sentences. **About a quarter of them are JMdict words spelled in kana:**

- **Words JMdict keeps in kanji, spelled in hiragana:** 5,180 distinct words, 11.0% of the missing words' occurrences,
  such as ひとつ (14,979 sentences), いま, きょう, いいかげん (5,666), しずか, いっしょ, におい, うえ, おやじ.
- **Katakana spellings:** 8,454 distinct words, 12.6%, such as ボク (24,948), アタシ, テメエ, ムリ, クマ.

**Why:** `DictLookup.LookupWordRawInner` keeps, for a word written only in kana, only the entries that are kana-only
or marked "word usually written using kana alone". 一つ isn't marked so, so ひとつ finds nothing, though JMdict has it
as 一つ's reading; the lookup then falls back to the names dictionary, which has ひとつ as the name "Hitotsu". A
katakana spelling isn't looked up at all, as the forms the lookup knows (`MightBeWord`) are JMdict's exact spellings.
The filter looks deliberate: a kana word can be the reading of many words, as かみ is of 紙, 髪 and 神.

**The catch:** many of the kana "words" are tokenizer fragments, interjections or names that happen to be readings,
such as うう, れ, ふっ, えと, ハッ, ス, ナルト; a lookup by reading would match them wrongly. Of the occurrences, about
half are of a reading with one entry, half of a reading with several. Options, for Magnus to weigh:
1. Look a kana word up by reading when it's a whole token of the tokenizer's, and the reading has exactly one entry.
2. Look katakana up as hiragana, under the same rule.
3. Leave the dictionary as it is, and give the common ones vocab: the reservoir ranks them by how many works show them.

Any of the first two changes parsing, so it needs a full rebuild and its diff looked at.

**Tried (Magnus, 2026-11-12), with names matched too:** on the branch `experiment/kana-readings-and-names`, never merged,
over a copy of the reservoir. Kana words were looked up by reading in hiragana and in katakana, and a single token that
neither a vocab nor the dictionary has became a name match (`NameMatch`, displayed and never valid, as a missing match
is) when the names dictionary has it. Two versions of the lookup:

- **Any kana word, by any reading:** 20 of the 230 parsing cases broke. A run of kana spanning several tokens is too
  often the reading of an unrelated kanji word, which then beats the vocab: たから (宝), ないよう (内容), えて (得手),
  だったん (韃靼), がよく, たいん, えたり.
- **Only a word that is one whole token, by any reading;** a longer run keeps the kana-only filter, in either kana: 6
  cases changed, mostly matches nobody sees, as single-token base forms such as しれる, したがう and とぶ now find kanji
  words by reading. One shows: with compounds hidden, かもしれない breaks into かも and しれる.

| Words no vocab has, shown in enough works | Today | Any kana word | One whole token |
|---|---|---|---|
| Dictionary words (three works) | 68,514 | 86,185 | 78,463 |
| Missing words (five works) | 30,137 | 15,933 | 16,087 |
| Names (five works) | | 6,383 | 6,415 |

- **The missing words halve.** The names take 6,415 of them: mostly real ones, 咲, 太郎, ハリー, 戦場ヶ原, コナン, 甲子園,
  鈴木, and products such as ウルトラマン; but some fragments the names dictionary happens to hold, ていう, 思, 違, 教, and
  さがせ. The kana readings make about 10,000 more dictionary words: ボク, ひとつ, いま, きょう, いいかげん, アタシ, ムリ,
  オマエ, におい, but also fragments and cries a reading happens to fit, れ, そ, ほ, ス, うう, ハッ, ふっ, ハハ, and names,
  ナルト, ナツ, ハル.
- **What stays missing** is now mostly not words: small kana and fragments (っ, ぇ, ぁ, ッ), punctuation (—, 〜, ％),
  cries and laughter (フフッ, ハハッ, ぐっ, クッ), numbers, Chinese and English text, and a few franchises' names
  (プリキュア, ラディッツ); with some real words in spellings nothing finds (てめぇ, 呆然と, に関する).
- **The working set it would take** grows by about a tenth, as more words get a floor: with the vocab and the dictionary
  words at 3, 5, 10 and 20 sentences each, 204,236, 308,558, 537,881 and 913,966 sentences with the one-token version,
  against 188,014, 282,132, 490,163 and 833,632 today.
- **Analyzing the whole reservoir took 6.5 to 8 minutes,** about 34,000 sentences a second, against 40 minutes and 6,000
  a second on 2026-11-11: the parsing speedups of the branch `parser-performance`.

### Vocab that never match, though their form is everywhere

30 vocab match no sentence of the 16 million, though at least ten sentences have their form. The top: たの (696,063
rejections), くなる (563,668), たんだ (207,717), にい (124,594), にも (121,752), 様だ (94,719), ものだ (61,943), うんだ
(48,062), おいて (30,665). Most fail "is a single token", and the tokenizer seems never to give these forms as one
token. Each is either a design I don't know, such as a vocab kept only to own a form, or a vocab dead in practice:
`vocab-the-reservoir-always-rejects.tsv` has the examples.

### Punctuation shown as words

— (66,169 sentences), 〜 (31,024) and - (10,499) are among the most displayed missing words: the tokenizer gives them as
words, and the sentence breakdown shows them. The cleaning could drop a line-ending —, or the analysis could take such
characters for punctuation.

### Works that may be garbled

The median work displays a missing word in 25.5% of its sentences; the works far above it are garbled:

- **Foreign subtitles misread as Shift JIS** (fixed above).
- **Bilingual subtitle files,** a line in Japanese and in Chinese or English: Haiyore! Nyaruko-san, Saiyuuki Reload
  Blast, Sakurasou no Pet na Kanojo (「大阪の大学に行くのわ 因为要去大阪的大学」), Asagao to Kase-san.
- **Subtitles read off the screen by OCR:** Baccano!'s 「知つてる力分力るよなぁ」 has 力 for か and つ for っ; a deck of
  たまこまーけっと is named "(OCR)".

Such works' missing words are errors, and their sentences poor picks: the picks, and the missing-word lists, should
leave out works far above the median. Not done: where to draw the line is a judgment to make with the list in hand
(`works-that-may-be-garbled.tsv`).

### The picks still favor the unusual

What the corpus lacks most is what's most unusual, so the greedy pick takes the strangest sentences the reservoir has.
Among the first 25 of `sentences-worth-taking-in.tsv`, besides good ones such as 「寝所に無断で忍び入ったことは 不問に処す」:

- **Chinese:** bilingual subtitle lines (「会いたい时はどうする？翼があればいいのに 想邂逅的时候该怎么办？」, Nourin) and a
  translator's credits (「二小姐 中级神仙 翻译：石头 …」, Servant x Service). Their single kanji match vocab such as 有 and
  明. Such works have only a few such lines, so the measure per work misses them (Nourin 20%); a test per sentence would
  catch them: characters found only in simplified Chinese, such as 们, 这, 说, 时, 级, mean the line isn't Japanese.
- **Furigana flattened into the text,** in some light-novel audiobooks: 「材ざい木もく座ざは顔を逸そらす」 (Oregairu).
- **Pre-war spelling:** Aozora Bunko's Jane Eyre, 「落着かぬやうで寢返りをしながら」. Real Japanese, but not what the
  corpus is for; a judgment per source.

None of it is built yet: the test for Chinese changes what the reservoir counts as a Japanese sentence, so it's for
Magnus to agree first.

### What the corpus doesn't test

- **Rejections only the reservoir has:** of 7,390 pairs of a vocab and a reason rejecting its matches in the reservoir,
  the corpus has none of 3,475. Each is a condition, or another requirement, deciding something the corpus never shows
  it deciding: right or wrong, it's untested.
- **Vocab with no sentence:** the corpus matches 2,669 of its 28,004 vocab in no sentence; the reservoir has sentences
  for 2,517 of them. 16,025 vocab have fewer than 3 sentences in the corpus and some in the reservoir.

## Near-instant feedback, at 13 million sentences

The vocab feedback (`reservoir vocab`, and the MCP tool `what_a_vocab_does_in_the_reservoir`) loads every sentence's
text once, 17 seconds for 13 million, and then takes 1.5 to 6.6 seconds a vocab, analyzing up to 20,000 of the
sentences containing its texts: 鬱 is in 172,277 of them, たまえ in 6,442 and たん in 627,905. A vocab's time covers
scanning the 13 million texts for its own, reading the numbering and the sample's bulk analyses from the reservoir, and
analyzing the sample anew; 鬱's, the first, also covers loading the dictionary (1.1 seconds). How it splits between
those wasn't measured: the bulk analysis's speed suggests 2 to 2.5 seconds for analyzing 20,000 sentences, which would
leave a second or two for the scan and the reads. A profile would tell.

## Parsing problems, each seen across many sentences

- **たまえ, the imperative of the auxiliary 給う, is often tokenized as a person's name.** 「落ち着きたまえ」 tokenizes as
  落ち着き (名詞・一般) + たまえ (名詞・固有名詞・人名・名), and 「従いたまえ」 as に従い (助詞・格助詞・連語) + たまえ. The
  match-condition of たまえ and 給え, "the preceding token is a masu stem", is right, but sees no masu stem: of 6,442
  reservoir sentences with たまえ, 1,061 reject it so. A reading of the pair's tokens, as for the ichidan verbs the
  tokenizer's dictionary lacks (`IchidanVerbUnknownToTheTokenizer`), could make the first a masu stem. (Found with
  IPADIC; UniDic may cut these differently.)
- **A noun vocab matches the base form of a verb token.** The interjection うっ tokenizes as the verb うつ (五段・タ行,
  連用タ接続), and the vocab 鬱 (うつ) matches its base form, valid and in the parsing result: in 3,823 of 20,000
  sampled sentences containing its texts, such as 「紅葉 てめえ いいかげんなこと… うっ…」. A rule that a vocab that doesn't
  inflect matches only surfaces, never a base form, would end the whole class of such matches; a condition on 鬱 alone
  would end this one.
