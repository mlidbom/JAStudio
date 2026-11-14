# Switching the tokenizer dictionary from IPADIC to UniDic

> **Status: under way on the branch `unidic` (Magnus, 2026-11-13).** Step 1, comparing how the two tokenize, is done. Step 2,
> the port, is in progress: the tokenizer reads with the spoken-language UniDic, and 48 of the 230 parsing cases parse differently,
> listed below with what's to be done. Open for Magnus: how the dictionary ships (Claude recommends downloading it on
> first use, from NINJAL: below).

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

## Which UniDic: spoken Japanese, 2025.12 (unidic-csj-202512)

**Chosen on the parsing cases (2026-11-13), over the written-language one the counts below first pointed to.** JAStudio's
sentences are mostly subtitles: speech. With the written-language UniDic, 50 of the 230 parsing cases parsed differently from
IPADIC; with the spoken-language one, 46, before any fix. The spoken one reads right what the written one reads wrong, in
7 cases, and they're everyday speech: imperatives followed by よ or って (聞けよ, 言えよ, 金貸せって, 放せよ, 返せったら,
which the written one reads as potential forms, 聞けよ as one token), しろ on its own (the written one: 白, "white"),
未練たらしい, 他人を気遣い (the written one: the noun 気遣い), こわっ and おっせぇ. It reads wrong in 2 the written one reads
right: 教えにしたがって (教え as a verb) and 償いきれない (償い as a noun); and in 行ったって話 it reads った + って, the quotative,
where the written one reads the conditional たって, which I believe is wrong there. Its one oddity met so far: katakana オレ's
lemma is `オレ-au lait` (café au lait), which matters only once lemmas are used. Switching back is one constant,
`TokenizerDictionary.UniDicRelease`.

### The first comparison: written Japanese (unidic-cwj-202512)

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

## Open: how the dictionary ships (Magnus; Claude recommends downloading it on first use, from NINJAL)

IPADIC comes in MeCab.DotNet's NuGet package, copied next to the assemblies. UniDic 2025.12 is 1.3 GB unpacked, 1 GB of it
one file, the matrix of connection costs, and 0.7 GB zipped. That rules out IPADIC's way: nuget.org takes packages of up to
250 MB, and GitHub refuses files over 100 MB (Git LFS would take it, beyond the free quota). The NuGet package
`Alissa.UniDic-CWJ.binary` is unofficial and stuck at UniDic 2.3.0 (2020). What's left is downloading it on first use, from
NINJAL (`https://clrd.ninjal.ac.jp/unidic_archive/2512/unidic-csj-202512.zip`) or from a mirror of our own, as JMdict's
database is generated on first use into the same folder. Until it's decided, the branch reads it from
`%LOCALAPPDATA%/JAStudio/unidic-csj-202512`, put there by hand; a missing folder fails saying how to get it.

**Claude's recommendation (2026-11-13): download it on first use, straight from NINJAL,** pinned to the release's URL and
checked by its SHA-256, unpacked into `%LOCALAPPDATA%/JAStudio/unidic-csj-202512`. Nothing hosted, nothing external changed.
- NINJAL served 100 MB in 10.7 seconds, so the 710 MB zip takes about 75 seconds, once.
- A mirror of ours would save about half of that: xz packs the dictionary into 372 MB (`matrix.bin` 346 MB, `sys.dic`
  27 MB). Not worth hosting for. If NINJAL ever moves the file, the check fails loudly, and a GitHub release asset of the
  public repository, which downloads without signing in, takes its place.
- CI (`ci.yml`, `dotnet test` on Ubuntu) needs it too: download on first use works there unchanged, about a minute
  slower. Caching it in GitHub Actions would change the workflow, which needs Magnus's word.
- MeCab.DotNet maps `matrix.bin` into memory (`MeCab.Core.Connector`), so its gigabyte isn't Anki's private memory: only
  the pages a parse touches are read, from the operating system's file cache.

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

## The parsing cases that parse differently (spoken UniDic, 2026-11-13)

48 of 230, after the fixes of step 2 so far. "Not displayed" means the sentence breakdown shows the same words; only the
parsed words, which include every candidate, changed.

- **Counted wrong at first:** 51 differed when this section said 34. The script that listed the failing cases dropped the
  17 whose names xUnit cut short at 50 characters (the だって cases and others). The specifications' runner prints them
  whole with `-printMaxStringLength 0`.
- **Fixed since:** ら抜き (食べれる, 寝れない), by `RaNukiPotentialSplitter`, below.

- **Better than IPADIC, to accept:** 待ってました (て is the contracted ている's, as UniDic says), 服を引き出しの中に (引き出し,
  the drawer, not the verb 引き出す), お構いなく and いえ (いえ the interjection, not 言う's imperative), 行ったって話 (above).
- **Extra or fewer candidates, nothing displayed changes, to accept:** の|に, それなのに, 日本|語, 教科|書, か|も, お|金, 一|波,
  １人 and 一人 now one token, ばら撒かれる (ばら撒く one verb), 返せったら (ったら one particle), the copula's に (だ) in
  逆に and 厳密に, 何なんだろうな.
- **UniDic's mistakes:** 教えにしたがって, 償いきれない (above).
- **Colloquial contractions, to accept (Magnus, 2026-11-13):** すげえ and うるせえ come out as one token whose base form is
  the word, すごい and うるさい, where IPADIC cut すげ|え and うる|せえ and JAStudio's vocab for the sound change (え, せえ)
  matched. The breakdown now shows すごい and うるさい. Magnus: "if the tokenizer get's them right right off the bat that is
  great. We change whatever we need to change to make the result come out well. [...] we don't go breaking correct
  tokenizing to fit our hacks designed to deal with bad tokenizing". The same goes for every case where UniDic tokenizes
  right and JAStudio's workarounds for IPADIC no longer fit: the workarounds change.
- **Fixed:** ら抜き: 食べれる and 寝れない are one token in UniDic (base 食べれる, lemma 食べる), where JAStudio expects the
  verb and れる. `RaNukiPotentialSplitter` splits a token whose lemma is its base form with る for れる, and
  an ichidan verb or 来る as UniDic tokenizes the lemma alone: so 走れる (走る, godan) and 忘れる (lemma 忘れる) stay whole.
- **The vocab that require a single token (`singleToken`), 48 in the corpus:** the condition said "IPADIC gave this as one
  token". UniDic, cutting short units, gives some of those words as several: だって is always だ|って, ので always の|で (行く|の|で,
  雨|な|の|で), において に|おい|て. Such a vocab no longer matches anywhere: ので has 489 sentences, だって 346. Others keep
  their meaning, a guard against matching across tokens by coincidence (たか, たね, にお). Each needs deciding, on the full
  rebuild's diff. For だって the distinction the condition stood for, the particle "even" against the copula and the quoting
  って before a verb of saying, was made in code for IPADIC (`MisTokenizedQuotativeDatte`, removed by the port), and its
  vocab's `yieldLastTokenToOverlappingCompound` may now make it on its own: だって yields its って to って言う.
- **To fix:**
  - たって (見えなくなったって): UniDic cuts た|って here, where 行ったって gets たって as one token; the vocab たって requires a
    single token.
  - すぎ (考えすぎ): UniDic has すぎ as a suffix (接尾辞,形状詞的), so the vocab すぎる doesn't match it.
  - てらっしゃい (下りてらっしゃい) is one auxiliary in UniDic; the vocab て and らっしゃる don't match.
  - くる's dictionary-form split gives its う as a surface match, where it was a base match.
  - 捕まえよ, 見つけよ, 離れよ: the classical imperative in よ, which UniDic gives as a volitional; the よ now matches, but
    differs from the expected in a way still to look at, and 離れよ shows 離れ rather than 離れる.
  - 何言っとんだ: とん|だ changed to とる and だ.
  - A parsing case for 行け夏目, which left a token spec (the commit of step 2 says why).

## The full rebuild with UniDic (2026-11-13)

Run in the branch's own corpus checkout, against its recorded commit, `37f3f7fede7`, taken as IPADIC's result: its last
rebuild changed nothing, and the source commits since are refactorings that change no behavior and the parser-performance merge.
It changed the parsed words of 20,709 sentences and the sentence count of 4,026 vocab. Most of it is new candidates, not
displayed: UniDic's short units give more of them (だ 4,053, に 2,087, も 1,954). Looked into with the MCP server's
`list_the_sentences_where_a_words_parsing_changed`:

- **Better:**
  - 行う: IPADIC read 行って and 行った as 行う in 99 sentences; UniDic reads 行く.
  - てくれ: UniDic reads くれ as くれる's imperative, so 219 sentences show 待つ | てくれ, no longer 待つ | てくれ | てくれる.
  - だってさ, だってこと: with だって's `singleToken` gone (below), 41 sentences show だ | ってさ, だ | ってこと, where IPADIC's single
    だって was wrong.
  - Colloquial and unknown words: うまく祓えなくて finds 祓う; すげえ and うるせえ (accepted above).
- **The single-token vocab, decided:** ので (489 sentences), だって (346) and において (8) lose `singleToken`: UniDic always
  cuts them (の|で, だ|って, に|おい|て), so the condition said nothing but "never". Without it, ので is back in all its
  sentences but 4: いいので and のでね now show いいの | で and の | でね, longer vocab claiming its parts, and こんなので見よう
  shows ので where it's こんなの + で. だって loses nothing; it's displayed in a few quotative sentences where it shouldn't be
  (女だって思われたい, 美少年だって知ってる).
- **The single-token vocab, kept:** たって and とも. UniDic cuts them both ways, sometimes wrongly: 見たって ("saw, they say")
  is now the conditional たって, 当たったって聞いた rightly た + って; 女とも is rightly と + も, but the concessive しようとも is
  lost. ただ, 話, うまい, もし and かつて change only in candidates.
- **だって, decided (2026-11-14), with the parsing rules only** (Magnus, 2026-11-13: "no tokenizer hack"; a vocab per
  meaning, each with its meanings ordered by what its match-configuration makes likely):
  - **The sentence-initial conjunction (だって言ったじゃん) is a vocab of its own, `だって:s.start`,** as `で:s.start` is:
    a clone of だって that requires `IsAtTheSentencesStart` and doesn't yield its って, so って言う doesn't take it. It wins at
    the sentence's start by its condition's weight, as `で:s.start` does over で. Its answer is だって's, whose order already
    puts "because/after-all | but" first.
  - **だって isn't matched before a verb of saying, thinking or hearing:** its match-condition, in place of `singleToken`,
    is that the following token's base form doesn't start with 言う, いう, 言える, 思う, 思える, 思い込む or 聞く, the
    verbs the removed `MisTokenizedQuotativeDatte` listed. That one left out 分かる, 知る and 信じる, which the "even"
    particle commonly precedes (子供だって知ってる). So 無敵な女だって思われたい shows だ | って | 思われる, where no って-compound
    matches 思われる, as 友達だって言えば shows だ | って言う. The yield stays, for the って-compounds other verbs may have.
  - **For Magnus to consider:** だって's meanings put "because/after-all | but" first, the conjunction's, which the
    `だって:s.start` vocab now covers. In the middle of a sentence "even/too/as-well/also" is the likelier.
- **"Hide all compounds" follows UniDic's cuts, to accept:** it hides a compound whose parts all match, and UniDic cuts
  words IPADIC gave as one token, so with the setting on such a word shows the words of its tokens, which are the compound
  parts its vocab declares:
  だって shows だ | って, のに の | に, それなのに それ | な | の | に, 日本語 日本 | 語, っていう って | いう, 本当に 本当 | に. And the
  other way: 一人, １人 and ばら撒く are one token in UniDic, so they're no compounds, and show whole.
- **Found on the way:**
  - 本当に's compound parts are 本当 and 当に; they should be 本当 and に.
  - UniDic reads 信じ before られ as the サ変 verb 信ずる (信じられない), so the vocab 信じる doesn't match it there. A form
    信ずる on 信じる would. Other じる verbs with a ずる twin may do the same: the full rebuild's diff will tell.
- **The corpus edits for the main checkout** (made here only to analyze):
  - Remove `singleToken` from the match-conditions of ので (`6c2ff488-df1d-401a-b2b5-33c3a43c4610`) and において
    (`a2f36dac-3c75-4547-9a5b-2cfa6b4b5ba8`).
  - だって (`5cc160fc-9a00-42d0-9931-786832956d0e`): its match-condition, in place of `singleToken`,
    `{"allOf": [{"not": {"followingToken": {"baseForm": {"startsWithOneOf": ["言う", "いう", "言える", "思う", "思える", "思い込む", "聞く"]}}}}]}`.
  - A new vocab `だって:s.start`: だって's "Clone with the disambiguation" `s.start`, then its match-condition
    `{"allOf": [{"sharedCondition": "IsAtTheSentencesStart"}]}` and its yield to the overlapping compound unset. The
    stripped-down corpus has it as `48336fe0-c86e-441a-8202-384fa3186ab9`.
  - 本当に's compound parts: 本当 and に.

  The parsing cases' stripped-down corpus has the edits of ので, だって and `だって:s.start`. It holds no において, and 本当に's
  compound parts change no parsing.
- **Still to decide:**
  - The rest of the diff, word by word: いる (169 removed), ー (294), ち (135), まあ (72), え (71), さ (72), たい (64), なる
    (57), and the display changes of が, た, は, の, に.

## The steps

1. **Compare, changing nothing.** Done (2026-11-13): the comparison tool, and the findings above.
2. **Port the tokenizer layer to UniDic.** In progress: the tokenizer reads with it, the registries are UniDic's, and JAStudio's
   facts about a token, the splitters and the fix for ichidan verbs the dictionary lacks speak UniDic's terms. The volitional,
   which UniDic gives as one token (書こう, だろう), is split as IPADIC cut it (`VolitionalSplitter`).
   **Every correction and split is the pre-processing stage's (Magnus, 2026-11-13):** the volitional and ら抜き splits
   were first written in the tokenizer (`MeCabTokenizer`, then named `JNTokenizer`), beside the older correction of ichidan verbs the dictionary lacks, each giving
   tokens indistinguishable from MeCab's, so nothing kept what MeCab gave, and the breakdown's edit mode showed the
   corrected tokens as the tokenizer's. Now `MeCabTokenizer` only translates, and the pre-processing stage only splits, each
   piece a type of its own with one source token. The ichidan verb the dictionary lacks isn't joined any more: the facts
   of its two tokens are the verb's (`IchidanVerbUnknownToTheTokenizer`), and they're split into the pieces of a verb the
   dictionary knows. Magnus: "joining tokens is what the parser excels at". A rebuild after both changes matched the one
   before them, sentence for sentence.
   What was planned: JAStudio's token (`MeCabToken`), its parts of speech (`UniDicPartOfSpeech`,
   `UniDicPartsOfSpeech`), inflection types and forms are IPADIC's. They become UniDic's, read from the dictionary's own words as
   `When_the_tokenizer_gives_a_word_of_any_inflection_type_its_dictionary_has` reads IPADIC's, and every use of them in
   the parsing and the matching configuration moves to UniDic's terms, guided by the correspondences the comparison
   counted. The four splitters are checked against UniDic's cuts: what they repair may not happen with UniDic, and
   UniDic's own mistakes may need others. The corpus's vocab with tokenizer conditions (9 when counted) are migrated.
3. **Check the result:** the parsing cases, a full rebuild's diff of the corpus, and the reservoir analyzed before and
   after, on a copy of the reservoir.
4. **Then names and kana forms,** built on the lemmas and the proper-noun tags.
