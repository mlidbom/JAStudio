# Switching the tokenizer dictionary from IPADIC to UniDic

> **Status: under way on the branch `unidic` (Magnus, 2026-11-13).** Step 1, comparing how the two tokenize, is done.
> Open for Magnus: how the dictionary ships (below).

JAStudio's tokenizer, MeCab (MeCab.DotNet), reads with a **tokenizer dictionary** (`TokenizerDictionary`): the words it
knows, with their parts of speech, inflections and base forms, and the costs by which it picks how to cut a text into
words. Not JMdict, the dictionary JAStudio looks words up in for their meanings. JAStudio's is IPADIC, the one
MeCab.DotNet's package comes with, and most likely the one Janome, the Python tokenizer JAStudio started on, bundles.

Claude raised it on 2026-11-13, looking for how to find the JMdict words spelled in kana that the reservoir found missing.
Magnus, 2026-11-13: "it sure as f-ing hell sounds like we are using the wrong dictionary with MeCab :~( Huge sounds all
scary. BUT, no time will be easier than now." Go-ahead the same day: "go right ahead according to your recommendation".

## Why UniDic

- **IPADIC hasn't changed since 2007.** UniDic is maintained by Japan's national language institute, NINJAL: releases
  2021-04 (3.1.0), 2022-09 (3.1.1), 2023-03 (2023.02) and 2025-12-31 (2025.12), the newest adding words and revising
  parts of speech. Licensed GPL 2.0, LGPL 2.1 or BSD, at the user's choice. It's what MeCab is mostly used with today.
- **It knows more of the words in our sentences:** 0.6% of its tokens in a million reservoir sentences are words it
  doesn't have, against IPADIC's 3.1%. It knows ポケモン, ピカチュウ, ルフィ, ナルト, interjections such as ハァ, フフッ
  and ハハハ, and colloquial spellings such as ヤツ, オラ, アンタ.
- **Its lemma (語彙素) says which word a token is,** uniting a word's spellings: ボク → 僕, きょう → 今日, いっしょ → 一緒,
  すげえ → 凄い. That's the kana-forms problem of [what-the-reservoir-found.md](what-the-reservoir-found.md) solved in the
  tokenizer. It doesn't tell homophones apart by meaning: かみ is always 髪, にる always 煮る.
- **It tags names,** with their kind and reading: 河合 is a surname, カワイ; 翔 a given name; 戦場ヶ原 a place.
- **Each token also has** the word's origin (native, Sino-Japanese, foreign, proper noun) and its pitch accent.

## Which UniDic: written Japanese, 2025.12 (unidic-cwj-202512)

Compared over the same million reservoir sentences with the comparison below:

| Compared | Sentences cut alike | Unknown tokens |
|---|---|---|
| IPADIC, against cwj 2025.12 | 55.2% | 3.1% against 0.6% |
| unidic-lite (2.1.2, 2013), against cwj 2025.12 | 90.4% | 1.1% against 0.6% |
| cwj 2025.12 (written), against csj 2025.12 (spoken) | 94.1% | 0.6% against 0.7% |

- **The spoken-language UniDic (csj)** marks fillers (あの, えー as 感動詞,フィラー), but makes lemma mistakes the written
  one doesn't: オレ's lemma is `オレ-au lait` (café au lait) 2,958 times in the million sentences, and 描く's is 書く.
  Nothing in the counts makes it better for our sentences.
- **unidic-lite**, the 2013 release fugashi installs, is a fifth of the size, but doesn't know the newer words and names,
  and cuts numbers and interjections less consistently.
- **Size:** 2025.12 is 1.3 GB unpacked, 1 GB of it the matrix of connection costs; 664 MB zipped. MeCab maps it into
  memory, so it loads in tens of milliseconds, and parses as fast as with IPADIC.
- **MeCab.DotNet reads it:** its features have 29 fields, some with quoted commas, such as the accent `"1,0"`, which
  `MeCabFeatureFields` reads.

## Open: how the dictionary ships (Magnus)

IPADIC comes in MeCab.DotNet's NuGet package, copied next to the assemblies. 1.3 GB of UniDic could be committed to the
repository (in git's history forever), packed as a NuGet package of our own, or downloaded on first use. Until it's
decided, the branch reads it from a folder outside the repository.

## The comparison tool

`JAStudio.HeadlessHost tokenizer-dictionaries compare --first <folder, or bundled-ipadic> --second <folder>
--reservoir-file <file> --sentences <count> --report-folder <folder>` tokenizes a sample of the reservoir's sentences,
spread evenly over it, with both dictionaries, aligns their tokens by position (`AlignedSpan`), and writes a report
(`TokenizerDictionaryComparisonReport`): `report.md`, and a tab-separated file of each kind of difference in full, with
examples. A million sentences take 15 seconds, plus a minute reading them from the reservoir, which is only read.

- **Where they cut differently** (`cutting-differences.tsv`): IPADIC's ひとつ against UniDic's ひと|つ.
- **For the tokens both cut alike, how each describes them:** parts of speech, inflection types, inflection forms, base
  forms, words (UniDic's lemma; IPADIC's base form, as it has no lemma), and the tokens only one of them knows.

## What changes from IPADIC to UniDic (IPADIC against cwj 2025.12, a million reservoir sentences)

- **Cutting.** 55% of sentences cut alike, 91% of IPADIC's tokens. UniDic cuts in short units (短単位):
  - **finer**, 380,000 times: でも → で|も, という → と|いう, として → と|し|て, どうして → どう|し|て, のに → の|に,
    本当に → 本当|に, お父さん → お|父|さん, すみません → すみ|ませ|ん, and every digit of a number apart (10 → 1|0);
  - **coarser**, 193,000 times: だろ|う → だろう, でしょ|う → でしょう, しよ|う → しよう, ２|人 → ２人, あ|あっ → ああっ,
    て|ん → てん, や|がっ → やがっ.
  JAStudio matches vocab over several tokens, so a word cut finer is still matched; what assumes IPADIC's cuts is the
  rules that ask whether something is a single token, and the four splitters in `PreProcessingStage`, which repair
  IPADIC's own mistakes.
- **Parts of speech.** UniDic's are finer in some places and coarser in others:
  - nouns: 名詞,一般 → 名詞,普通名詞,一般, or 固有名詞,人名 for names IPADIC didn't know; 名詞,形容動詞語幹 → 形状詞;
    名詞,代名詞 → 代名詞; 名詞,接尾 → 接尾辞;
  - **verbs lose IPADIC's 自立/非自立 distinction:** いる, おく, もらう, 来る are 動詞,非自立可能 ("can be auxiliary")
    whether they're used as auxiliaries or not. Where JAStudio needs to know, the token before has to tell;
  - てる, ちゃう, れる (IPADIC 動詞,非自立 or 接尾) are 助動詞; ん and の after a verb (IPADIC 名詞,非自立) are
    助詞,準体助詞; ない is 助動詞 after a verb (知らない) and the adjective 無い elsewhere (じゃない, 時間がない);
  - に after an adjectival noun (静かに) is the copula だ's 連用形-ニ, not IPADIC's 助詞,副詞化.
- **Inflections.** UniDic tells apart what IPADIC lumps together: the conclusive and attributive forms (IPADIC's 基本形 →
  終止形-一般 or 連体形-一般), and each euphonic change (IPADIC's 連用タ接続 → 連用形-促音便, -イ音便, -撥音便). The
  volitional is a form of its own (意志推量形: だろう, でしょう). Inflection types are named differently: 五段・ラ行 →
  五段-ラ行, 一段 → 上一段-ア行 or 下一段-カ行 and so on.
- **Words.** The lemma is spelled UniDic's way, often a kanji spelling few use: する → 為る, いる → 居る, こと → 事, この →
  此の, ちょっと → 一寸. Some carry a sub-classification: 僕-代名詞, そう-様態. So a lemma maps to a vocab through
  JMdict, which has those spellings among a word's forms, by spelling and reading.

## The steps

1. **Compare, changing nothing.** Done (2026-11-13): the comparison tool, and the findings above.
2. **Port the tokenizer layer to UniDic.** JAStudio's token (`JNToken`), its parts of speech (`JNPartsOfSpeech`,
   `JNPOS`), inflection types and forms are IPADIC's. They become UniDic's, read from the dictionary's own words as
   `When_the_tokenizer_gives_a_word_of_any_inflection_type_its_dictionary_has` reads IPADIC's, and every use of them in
   the parsing and the matching configuration moves to UniDic's terms, guided by the correspondences the comparison
   counted. The four splitters are checked against UniDic's cuts: what they repair may not happen with UniDic, and
   UniDic's own mistakes may need others. The corpus's vocab with tokenizer conditions (9 when counted) are migrated.
3. **Check the result:** the parsing cases, a full rebuild's diff of the corpus, and the reservoir analyzed before and
   after, on a copy of the reservoir.
4. **Then names and kana forms,** built on the lemmas and the proper-noun tags.
