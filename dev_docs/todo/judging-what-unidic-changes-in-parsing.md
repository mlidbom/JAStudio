# Judging what UniDic changes in parsing

> **Status: agreed with Magnus 2026-11-15, not started.** The tooling comes first, for Magnus to take part in the
> judging: it's to be designed with him. This is part of [switching to UniDic](switching-to-unidic.md), and replaces what
> that plan said was to be fixed and accepted among the parsing cases, and its step 3.

## The rule (Magnus, 2026-11-15)

- **The goal is that parsing is not worse, not that it's unchanged.** "The goal is not necessarily the tests passing as
  they were before unidic. The goal is looking at the parsing diff and saying: This is not worse."
- **What's judged is the vocab the breakdown displays.** The parsing cases also check every parsed word that isn't
  displayed, since they were made stricter. Magnus: "Much stricter is nice for regression and being sure we see
  everything that changed where only small changes occur at a time... for swapping out the tokenizer dictionary....
  Eeeehh."
- **Each case is judged by itself, by reading it.** "I'm thinking comparing the results for each case, one by one,
  manually, and saying either: this is a clear regression, or this is actually better, or at least not worse."
  - **A clear regression** is fixed now. "We deal with fixing only those that are clearly regressions now."
  - **Better, or at least not worse:** "then we change the test to accept the new breakdown."
  - **Debatable:** "we add todo comments about coming back to it and looking closer once the unidic migration has reach
    the all-tests-green stage." Claude's reading: its new breakdown is accepted meanwhile, so that every test can pass.
- **Never by an algorithm.** A count, a grouping or a script can find the sentences to read and put them in order. It
  never gives a verdict. Magnus, on judging sentences outside the parsing cases: "you will need to manually, and
  carefully, apply all your understanding of Japanese and what this project aims for and how we break down sentences,
  to each of the one at a time to figure out whether the change was an improvement or not. Guessing by some algorithm
  I would NOT trust."
- **The two JAStudios are for this.** "This is why main has spent tons of time on making sure you and I can now work
  with the two versions of jastudio on the two worktrees at the same time and compare results": JAStudio on `main`
  reads with IPADIC, JAStudio on `unidic` with UniDic, and Claude's MCP gateway connects to both at once
  (`jastudio-mcp.md`).

## What the parsing cases are (Magnus, 2026-11-15)

Not a sample of sentences. "They are added one by one by me as I studied and encountered sentences that parsed wrong
over months and months of coding and studying, most of them I added and then fixed the bug that made them display
wrong. They represent a pretty wide range of things that used to go wrong, that was fixed. The great majority of them
are such that we pretty much know that the visible vocab were correct in them, at least with greater confidence than
any sentence not among the 230...."

- **So they're the first measurement of the migration.** What a case expects the breakdown to display is, for the
  great majority, a breakdown Magnus looked at and found right.
- **A few may still be wrong.** So the first step of the judging is Claude's: go through what each of the 230 cases
  expects the breakdown to display, and say whether it agrees that it's a correct breakdown.

## Sentences outside the parsing cases

The corpus's sentences, as the full rebuild changes them, and the reservoir's.

- **Only by reading each sentence, as the rule says.** If Claude can do it that way and has confidence in the result,
  "then by all means do that too" (Magnus).
- **Not by counting.** Claude proposed judging the corpus class by class, grouped by the change, such as たって → た | って
  in so many sentences. Magnus didn't accept that as a judgment: a group's sentences are still read one at a time. The
  grouping only finds them.
- **How far this goes is open:** decided when the parsing cases are done.

## Where the parsing cases stand (2026-11-15, after merging `main` into `unidic`)

48 of the 230 cases fail, and 2 of the reservoir's specifications (`When_a_reservoir_sentence_is_analyzed`, on だけど).
Every other specification passes. Sorted by the rule, from the cases' failure messages, which tell what a breakdown
displays apart from the other parsed words:

| What differs from IPADIC | Cases |
|---|---|
| Only parsed words that aren't displayed | 5 |
| No breakdown displays other words, though a parsed word that was displayed went or came | 11 |
| The same vocab displayed, matched by its surface where it was matched by its base form (くる's う) | 1 |
| Other vocab displayed only with "hide all compounds" on | 18 |
| Other vocab displayed in the default breakdown | 13 |

The first three rows, 17 cases, are the ones where no displayed vocab changes.

"IPADIC" below is what the case expects, and "UniDic" what the breakdown displays now. Only the words that differ are
listed, in the sentence's order. A vocab's id is given where both sides display a vocab of the same name.

### Only parsed words that are not displayed differ: 5

- godan potential and imperative - 返せったら
- godan potential and imperative - 逆に大丈夫に思えてくる
- misc stuff - 私たちなら嘘をつかずに付き合っていけるかもしれないね
- wbr word separation - だったら普通に金貸せって言えよ
- だって before a verb of saying, thinking or hearing displays as だ and a word starting with the quoting って - 場違いだって思うかもしれません

### No breakdown displays other words, though a parsed word that was displayed went or came: 11

- forbid adverb stem - 難しく考えすぎ
- godan potential and imperative - お金貸せって
- identify words - すげえ
- identify words - のに
- misc stuff - 教科書落ちちゃうから
- misc stuff - 服を引き出しの中に入れてください
- misc stuff - 行ったって話
- require forbid dictionary form head and start - デカいな
- require forbid dictionary form head and start - 頭突き以外でな
- requires a stem - うるせえ
- だって before a verb of saying, thinking or hearing displays as だ and a word starting with the quoting って - 舞っているんだって思い込むように

### The same vocab displayed, matched by its surface where it was matched by its base form: 1

- **dictionary form splitting - くる**
  - IPADIC: う (its base form)
  - UniDic: う

### Other vocab displayed only with "hide all compounds" on: 18

- **forbid adverb stem - 考えすぎ**
  - IPADIC: 考え
  - UniDic: 考える (its base form)
- **godan potential and imperative - 清めの一波**
  - IPADIC: 一波
  - UniDic: 一 | 波 (no vocab)
- **hidden exclusions - 厳密に言えば　俺一人が友達だけど - hidden matches**
  - IPADIC: 一 | 人
  - UniDic: 一人
- **identify words - 「コーヒーはいかがですか？」「いえ、結構です。お構いなく。」**
  - IPADIC: いう (its base form) (the vocab 言う) | え
  - UniDic: いえ (the vocab いいえ)
- **identify words - それなのに 周りは化け物が出ることで有名だと聞き**
  - IPADIC: それなのに
  - UniDic: それ | な | の | に
- **identify words - 彼の日本語のレベルは私と同じ位だ。**
  - IPADIC: 日本語
  - UniDic: 日本 (no vocab) | 語 (no vocab)
- **identify words - １人でいる時間がこれほどまでに長く感じるとは**
  - IPADIC: １ (the vocab 一) | 人
  - UniDic: １人 (the vocab 一人)
- **incorrect exclusions - 厳密に言えば　俺一人が友達だけど - incorrect matches**
  - IPADIC: 一 | 人
  - UniDic: 一人
- **misc stuff - ばら撒かれるなんて死んでもいやだ**
  - IPADIC: ばら (the vocab バラ) | ばら (the vocab 散) | 撒く (its base form)
  - UniDic: ばら撒く (its base form)
- **misc stuff - 何なんだろうな**
  - IPADIC: な | ん
  - UniDic: なん (the vocab なの) | なん (the vocab 何)
- **misc stuff - 厳密に言えば　俺一人が友達だけど**
  - IPADIC: 一 | 人
  - UniDic: 一人
- **misc stuff - 厳密に言えば　俺一人が友達だけどだけど**
  - IPADIC: 一 | 人
  - UniDic: 一人
- **misc stuff - 待ってました**
  - IPADIC: て
  - UniDic: てる (its base form)
- **require forbid te prefix or stem - お金を貸していただけないでしょうか**
  - IPADIC: お金
  - UniDic: お | 金
- **だって before a verb not of saying, thinking or hearing displays as だって - 子供だって知ってる**
  - IPADIC: だって
  - UniDic: だ | って
- **だって before a verb not of saying, thinking or hearing displays as だって - 男友達だっているでしょうに**
  - IPADIC: だって
  - UniDic: だ | って
- **だって before a verb not of saying, thinking or hearing displays as だって - 見える俺だって信じられないのに**
  - IPADIC: だって | 信じる (its base form) | のに
  - UniDic: だ | って | 信ずる (its base form) (no vocab) | の | に
- **だって before a verb of saying, thinking or hearing displays as だ and a word starting with the quoting って - 本当に友達だって言えるのか**
  - IPADIC: 本当に
  - UniDic: 本当 | に

### Other vocab displayed in the default breakdown: 13

- **hidden exclusions - 厳密に言えばだけど俺一人が友達だけど - hidden matches**
  - IPADIC: nothing
  - UniDic: だ | けど
- **hide all compounds - 教えにしたがって**
  - IPADIC: 教え
  - UniDic: 教える (its base form) (no vocab)
- **ichidan imperative - 捕まえよ**
  - IPADIC: よ (the vocab よ `8d071a9e`)
  - UniDic: よ (the vocab よ `e268ce3b`)
- **ichidan imperative - 見つけよ**
  - IPADIC: よ (the vocab よ `8d071a9e`)
  - UniDic: よ (the vocab よ `e268ce3b`)
- **ichidan imperative - 離れよ**
  - IPADIC: 離れる (its base form)
  - UniDic: 離れ
- **incorrect exclusions - 厳密に言えばだけど俺一人が友達だけど - incorrect matches**
  - IPADIC: だけど
  - UniDic: だ | けど
- **misc stuff - 見えなくなったって そんな**
  - IPADIC: たって
  - UniDic: た | って
- **require forbid te prefix or stem - 何言っとんだ**
  - IPADIC: んだ
  - UniDic: だ
- **requires masu stem - 償いきれない**
  - IPADIC: 償う (its base form) | きれない
  - UniDic: 償い | きれない (the vocab きれず)
- **yield to surface - 後で下に下りてらっしゃいね**
  - IPADIC: 下りる (its base form)
  - UniDic: 下り
- **だって before a verb of saying, thinking or hearing displays as だ and a word starting with the quoting って - 易しいテストだって聞いてたんだけど**
  - IPADIC: て | たん
  - UniDic: てた | ん
- **だって before a verb of saying, thinking or hearing displays as だ and a word starting with the quoting って - 薄情だっていうのは分かってます**
  - IPADIC: 分かる (its base form) | てます
  - UniDic: 分かってる (its base form) (the vocab 分かっている) | ます
- **だって starting the sentence before a verb of saying displays as だって - だって言ったじゃん**
  - IPADIC: だって (the vocab だって `5cc160fc`)
  - UniDic: だって (the vocab だって `48336fe0`)

### Claude's first impression of the 13, not yet a verdict

- **Intended (1):** だって言ったじゃん displays the vocab `だって:s.start`, made for the sentence-initial conjunction.
- **Looks not worse (2):** UniDic's reading of the contracted ている. 分かってる | ます where it was 分かる | てます, and
  てた | ん where it was て | たん.
- **Looks like a regression (10):** よ in 捕まえよ and 見つけよ (another よ vocab), 離れよ (離れ), 下りてらっしゃい (下り),
  償いきれない (償い | きれず), 教えにしたがって (教える, with no vocab), 見えなくなったって (た | って), 何言っとんだ (だ), and the
  two だけど cases (だ | けど).

## The steps

1. **The tooling, for Magnus to take part.** Magnus, 2026-11-15: "before we do this I want tooling", and: "let's discuss
   how we make it so that I can participate effectively in this effort." To be designed with him. What there is to
   build on is listed below.
2. **Claude goes through what each of the 230 cases expects the breakdown to display,** and says whether it agrees
   that it's a correct breakdown. It lists the ones it doubts, with why. Magnus: "maybe the very first thing would you
   be you going through the visible vocab in 230 and verifying if you agree with that beeing an correct breakdown.
   Maybe there are some few that are not."
3. **The 17 cases where no displayed vocab changes are accepted** (agreed, 2026-11-15), with the full mode's accepting
   run.
4. **The other 31 go in a table for Magnus:** the sentence, what IPADIC displays, what UniDic displays, and Claude's
   verdict: a clear regression, better, not worse, or debatable. Each row also says what Claude thinks of whether the
   case's original expectation was right about how the breakdown should look (Magnus: "definitely include something
   about what you think of whether the original spec sentence was wrong"). The 18 "hide all compounds" cases are judged
   there one by one, not as a class.
5. **The verdicts are acted on:**
   - better and not worse: the case accepts the new breakdown;
   - debatable: the case accepts it, with a todo to come back to;
   - a clear regression: fixed, with the parsing rules first, and a split in the pre-processing stage only where no
     rule can do it (`switching-to-unidic.md`, step 2). Each rule edit joins that plan's list of corpus edits for the
     main checkout.
6. **Every specification passes.** Then the debatable cases are looked at again.
7. **Sentences outside the parsing cases,** as far as is decided then, each by reading.

The rest of the migration follows in `switching-to-unidic.md`: how the dictionary ships, and the names and kana forms.

## The basics first: the test sentences in the corpus (Magnus's proposal, 2026-11-15, under discussion)

Magnus, on Claude's tooling proposal below: "Some of that is certainly interesting. But I want some vital basics in
place first." His workplan:

1. **A new branch from where `main` is now,** "so that we work on the pre-unidic corpus with the ton on UI changes that
   have been happening".
2. **In a headless JAStudio, each sentence of the parsing cases is added to the corpus,** with a tag that marks it as
   one of the test sentences ("Something about gold something? [...] We can always rename it, it is temporary"), and
   they're "checked into the corpus in a state where they all parse exactly as expected". "That is step 1 getting us to
   the ability to sanely look at these in jastudio, and it requires zero code changes."
3. **A small code change:** "For sentences with this tag, I want the full parsing results that we save in the test
   suite to be saved in the corpus", when a sentence is reparsed or updated.
4. **What Claude proposed is built on top of that:** "Seeing which differered, how, making judgments about if it is OK
   or not."
5. **"From here on in, the corpus is the master of the test suite. These sentences are what is extrected to the test
   suite."**

What Claude measured for it (2026-11-15):

- **The 230 cases are 220 different sentences.** 15 cases have a sentence configuration (hidden or incorrect matches),
  and 6 sentences are shared by 16 cases.
- **56 of the sentences are already sentence notes of the corpus,** one of them with marked matches of its own. 164
  aren't, and 90 of those are word forms of five characters or fewer, such as 泳げず, 泳げた, くる and のに.
- **A sentence note already stores most of what a case file does:** its configuration, in the same shape, and every
  parsed word with whether it's displayed. It lacks the breakdowns under each combination of the options that change
  them (`BreakdownUnderOptions`, in the specifications project today). A case's `why` fits the note's `comments`.
- **The MCP server can already create notes and tag them** (`create_a_note`, `add_a_tag_to_the_notes_found`).
- **The full mode already parses every case against the whole corpus checkout,** and checks that the stripped-down
  corpus parses each case exactly as it does (`FullModeRun`). So a case and the whole corpus agree whenever the full
  mode has been run: the difference Claude feared between the two doesn't arise then.
- **The repository has no term with "gold" in it yet.** In language processing, a gold standard is a set of sentences
  whose analysis a person has verified.

## The tooling Claude proposed, parked until the basics exist

1. **A "compare parsing" view in the workspace:** a sentence's displayed breakdown from this JAStudio and from another
   running JAStudio, side by side, with the differences marked. For any text, typed or sent there by Claude.
2. **A review list in the same place:** each item a sentence with Claude's verdict and reasoning, and buttons for
   Magnus's verdict and a comment, saved to files in the repository that Claude reads.
3. **Corpus sentences in the same list,** later.
4. **For Claude itself:** a gateway tool that compares a sentence across the connected JAStudios, and a tool that sorts
   the failing cases by displayed vocab.

The questions it put to Magnus, to take up when the tooling is designed:

- **Where he looks:** one view with both breakdowns side by side, with a way to open the sentence in both JAStudios, or
  two JAStudio windows next to each other.
- **How he answers:** by clicking a verdict in the tool, saved to files Claude reads, or in chat.
- **Who fixes a regression:** Claude with rules on his word, or he edits the vocab himself, and then in which JAStudio
  and corpus checkout.
- **How much he reviews:** every verdict, or only those Claude flags.

## What there is to build the tooling on

- **Several JAStudios at once.** The gateway connects to every running JAStudio, each under its own name, such as
  `anki-main`, `standalone-main` and `headless-unidic`, and offers each one's tools, such as `analyze_sentence`.
  Nothing yet compares two JAStudios' answers: `new_ideas_to_discuss.md` has the idea.
- **The workspace's sentence page,** in Avalonia: the breakdown, and the Parsing tab with the tokens and every match
  with its reasons (`jastudio-workspace.md`). It shows a sentence note. A parsing case's sentence is a text, not a note.
- **A case file** holds the sentence, why the case exists (`why`), every parsed word, and what each breakdown displays.
  It's JSON, so it has no comments: a todo for a debatable case needs a field, or a home elsewhere.
- **The cases' failure messages** say what differs, and tell what a breakdown displays apart from the other parsed
  words.
- **The full mode's accepting run** (`In_the_full_mode`, `FullModeRun`) rewrites the case files to what parses now.
- **The corpus's diff tools,** in the MCP server: `diff_the_changed_notes`, and
  `list_the_sentences_where_a_words_parsing_changed`.
- **No tool sorts the failing cases as above.** Claude did it with scripts in its scratch folder, three times over.
  That makes it a tool to build, as part of step 1.

## Open

- **Whether the parsing cases stay as strict as they are** (Claude recommends it, Magnus hasn't answered). Magnus
  wondered whether they should fail on anything but displayed vocab. Claude's view: keep the check and accept the case
  files anew. The accepting run writes them, so the strictness costs nothing once a result is judged, and after the
  migration, when changes are small again, it's wanted. Accepting each case as it's judged, not all at the end, makes
  the failing cases exactly the judged regressions.
- **Where a debatable case's todo is kept:** a field in the case file, beside `why`, which a specification or a tool
  lists; or elsewhere.
- **Whether parsed words that aren't displayed matter anywhere.** A vocab's sentences come from every vocab matched in
  a sentence (`SentenceSnapshot.DetectedVocab`, from the parsing result's `MatchedVocabIds`). Claude hasn't verified
  whether that includes matches that aren't displayed. If it does, a vocab gaining or losing many sentences is visible
  to Magnus, and worth one look per vocab.
- **Who edits the vocab, and in which corpus checkout,** when a regression is fixed with a rule: today Claude edits
  the worktree's own corpus checkout, only to analyze, and lists the edits for the main checkout
  (`045-committing-with-the-corpus-repository.md`). Part of the tooling discussion.

## What Claude did before the rule, and what the rule changes

- **Claude sorted the failing cases into "to accept" and "to fix"** in `switching-to-unidic.md`, meaning to accept in
  one run at the end. It never ran it, so about 50 cases stayed failing, and it measured progress by that count.
- **Three entries of that plan's "To fix" list aren't regressions by the rule:** くる's う (the same vocab, matched by
  its surface), すぎ (考えすぎ and 難しく考えすぎ: the default breakdown displays the same vocab) and
  お金を貸していただけないでしょうか (it changes only with "hide all compounds" on). They're judged in step 4 like the others.
- **That plan's classes "to accept" aren't accepted as classes:** each of their cases is judged in step 4.
- **The full rebuild's diff counted every parsed word, displayed or not,** such as "いる (169 removed)". That plan's
  "the rest of the diff, word by word" is replaced by step 7.
