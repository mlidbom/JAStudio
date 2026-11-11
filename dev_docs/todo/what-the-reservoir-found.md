# What the reservoir found (Claude, the night of 2026-11-11)

> **Status: findings for Magnus to look at, from the first runs of the reservoir** (see
> [expanding-the-sentence-corpus.md](expanding-the-sentence-corpus.md), "The reservoir, as built"). Nothing here changed
> the corpus or matching on `main`. Each finding says what was seen, with examples, and what could be done about it.
> The reports themselves are in `C:\Dev\_JAReservoir\reports\`.

## Bugs found and fixed on the branch

- **Analysis crashed on any sentence with a classical verb.** The tokenizer's dictionary, IPADIC, has 58 inflection
  types, and JAStudio knew 48. A token of one of the other ten, such as 下二・ガ行 (捧ぐ, 見上ぐ) or 文語・マジ, threw
  `KeyNotFoundException` in `InflectionTypes.GetByName`, failing the whole sentence. None of the corpus's sentences has
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
  analyses of the older one were forgotten and made anew.

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

What gives nothing now has a reason: Korean dramas, subtitles kept as images (Spirited Away, Whisper of the Heart),
and decks in English or Spanish. **To know before taking sentences in:** some decks' subtitles were read off the screen
by OCR, and it shows: Baccano!'s 「知つてる力分力るよなぁ」 has 力 for か and つ for っ. Such a deck's "missing words"
are OCR errors, so the measures and the picks should leave such decks out, or weigh them down.

## Near-instant feedback, at 13 million sentences

The vocab feedback (`reservoir vocab`, and the MCP tool `what_a_vocab_does_in_the_reservoir`) loads every sentence's
text once, 17 seconds for 13 million, and then takes 1.5 to 6.6 seconds a vocab, analyzing up to 20,000 of the
sentences containing its texts: 鬱 is in 172,277 of them, たまえ in 6,442 and たん in 627,905. Most of the time is the
analyzing, which a smaller sample would cut. An index of which sentences hold each character would cut the search too,
but it's already under a second.

## Parsing problems, each seen across many sentences

- **たまえ, the imperative of the auxiliary 給う, is often tokenized as a person's name.** 「落ち着きたまえ」 tokenizes as
  落ち着き (名詞・一般) + たまえ (名詞・固有名詞・人名・名), and 「従いたまえ」 as に従い (助詞・格助詞・連語) + たまえ. The
  match-condition of たまえ and 給え, "the preceding token is a masu stem", is right, but sees no masu stem: of 3,155
  reservoir sentences with たまえ, 595 reject it so. A preprocessing stage, as for the tokenizer's other known mistakes
  (`MisTokenizedIchidanVerbUnknownToTheTokenizer`), could split such a pair back into the masu stem and たまえ.
- **A noun vocab matches the base form of a verb token.** The interjection うっ tokenizes as the verb うつ (五段・タ行,
  連用タ接続), and the vocab 鬱 (うつ) matches its base form, valid and in the parsing result: in about 4,000 of 20,000
  sampled sentences containing its texts, such as 「紅葉 てめえ いいかげんなこと… うっ…」. A rule that a vocab that doesn't
  inflect matches only surfaces, never a base form, would end the whole class of such matches; a condition on 鬱 alone
  would end this one.
- **A common word is missing, with no dictionary entry:** いいかげん in 「いいかげんなこと」, though JMdict has いい加減.
  To be looked into: whether the dictionary lookup misses kana spellings of words JMdict keeps in kanji.

## What the corpus doesn't test

- **Rejections only the reservoir has:** in the first 180,000 sentences, 817 of 4,540 pairs of a vocab and a reason
  rejecting its matches never happened in the corpus. Each is a condition, or another requirement, deciding something the
  corpus never shows it deciding. The full list is in each report's `rejections-only-the-reservoir-has.tsv`.
- **Vocab with no sentence:** the corpus matches 2,669 of its 28,004 vocab in no sentence; the reservoir's first 180,000
  sentences had some for 1,363 of them.
