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
