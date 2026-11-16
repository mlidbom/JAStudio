# New ideas to discuss

Ideas Magnus or Claude want to discuss, written down when they occur, so that none is lost (Magnus, 2026-11-11). They're
ideas that came up while working on something else, or that a code comment can't hold because they're about no one
piece of code, or that would otherwise be lost in a long response.

- **Each idea says who had it and when,** and enough to be understood without the conversation it came from.
- **Once discussed, an idea moves to the document it belongs in,** or is dropped, and goes from here.

## Ideas

- **(Claude, 2026-11-11) Read .apkg files directly, with no Anki profile for importing.** An .apkg is a zip of a SQLite
  database and the media files, with a map from their numbered names to their real ones. An `IAnkiRawNotes` that reads
  one would let the import profiles take a deck file as they take the open collection. The 26 decks of the subs2srs
  folder (`todo/expanding-the-sentence-corpus.md`) are all in the older formats, which need nothing but SQLite.
- **(Claude, 2026-11-11) The reservoir as plain sentences, read from the sources and parsed in bulk.** A reader per kind
  of source, the decks and the subtitle files, writes each sentence with its source and, for a deck, where its media is.
  Parsing it all with JAStudio's parser and vocab, without making notes, gives the measures of value: vocab and
  match-conditions the corpus never exercises but the reservoir does, the words no vocab matches by how often they occur
  (a queue of vocab to create), contested words, and the fewest sentences that add the most of what the corpus lacks.
  Taking a sentence into the working set is then an import of it, media included.
- **(Claude, 2026-11-11) Find JMdict words spelled in kana.** In the reservoir's 16 million sentences, about a quarter
  of the missing words' occurrences are JMdict words spelled in kana: ひとつ (一つ, 14,979 sentences), いま, きょう,
  いいかげん, しずか, いっしょ, and katakana such as ボク (24,948), アタシ, ムリ. `DictLookup.LookupWordRawInner` keeps,
  for a kana word, only entries that are kana-only or "usually written using kana alone", and a katakana spelling isn't
  looked up at all, so all of them come out missing, in the corpus too. The filter guards against a kana word being many
  words' reading, and many of the kana "words" are fragments or interjections that happen to be readings (うう, れ, ハッ).
  A middle way: look a kana word up by its reading, katakana as hiragana, when it's a whole token and the reading has
  exactly one entry. The numbers and examples are in `dev_docs/todo/what-the-reservoir-found.md` on the branch
  `corpus-reservoir`.
- **(Claude, 2026-11-13) Keep what JMdict and JMnedict say that `jmdict.db` drops.** The database keeps a word's
  spellings, readings, priorities and senses, and a name's spellings, readings and translations. It drops what tells a
  kana spelling's standing (a reading's irregular, outdated or search-only use; that it isn't a true reading of the
  kanji; which kanji spellings it belongs to), which spellings a sense is restricted to, and a name's type (surname,
  given name, place, company, product). Kana forms and names would both use them.
- **(Magnus, 2026-11-13) Tokenize each sentence with the UniDic that fits its source.** UniDic comes as a spoken-language
  dictionary (csj), which JAStudio uses, and a written-language one (cwj). Eventually both, picked by what's known of the
  sentence's source: anime, spoken; an audiobook, written. A guess: whether it gives better results is to be found out. The
  comparison tool (`tokenizer-dictionaries compare`) and the parsing cases are where to find out: on the parsing cases the
  written one read 7 everyday-speech sentences wrong that the spoken one reads right, and 2 right that the spoken one reads
  wrong (`dev_docs/todo/switching-to-unidic.md`).
- **(Claude, 2026-11-13) Compare a tool's answers across connected JAStudios, in the gateway.** Comparing how two
  branches parse means calling `analyze_sentence` on `anki-main` and on `standalone-unidic` and reading the two answers
  side by side, sentence by sentence. A gateway tool could call one tool with the same arguments in several connected
  JAStudios and report what differs, such as the words of a sentence that only one of them matches; and for many
  sentences at once, only the sentences that parse differently. That is the experiment Magnus described: change the
  parsing configuration in one, and see at once where the two now disagree. The gateway knows nothing of JAStudio's
  tools, so it would compare answers as JSON, generally, which suits it.
- **(Claude, 2026-11-15) Get the solution to no inspection warnings, so that a new one shows.** Every inspection of a
  few files answers with dozens of standing warnings, which the new ones hide: `MemberCanBeInternal` and
  `MemberCanBePrivate.Global` throughout, the latter on every specification's helpers, and
  `NotAccessedPositionalProperty.Global` on the records JAStudio's MCP server only serializes to JSON. Either fix them
  in a campaign (the shared skill `run-and-fix-resharper-inspections`), or turn off the ones that are noise here, for
  the specifications and for the records that are serialized. *Magnus, 2026-11-15: "There will be a campaign, but not
  right now."*
- **(Claude, 2026-11-15) A type for a study-note's id, in place of `long`,** as the code standard on primitives asks.
  Best done now that the notifications from Anki are gone (`moving-jastudio-out-of-anki.md`, increment 2, part 4): much of
  the code that passes the id about today goes with them.
- **(Claude, 2026-11-15) View models dispatch to the UI thread with Avalonia's `Dispatcher`,** though the rule for the
  UI says that view models don't reference Avalonia types (`ImportRunWindowViewModel`, `StudyingWindowViewModel`).
  Core has `IUIThreadDispatcher` for it, but the specifications that open windows run with one that dispatches nowhere,
  while their windows live on the headless UI thread. A dispatcher for those specifications that posts to that thread
  would let the view models use Core's.
- **(Claude, 2026-11-15) The Sync button's green mark could go when a sync is due again.** Today it stays until the next
  sync starts, so it says only that the last sync finished. It could go when JAStudio has changed a note that has a
  study-note, since that study-note is then out of date until the next sync, and when Anki's own sync has finished,
  since that may have brought changes from AnkiWeb. JAStudio knows of both without Anki telling it of anything more.
- **(Claude, 2026-11-15) The workspace window can't be opened in a specification,** so a change to it, such as its
  Sync button, is seen first when Magnus restarts Anki. It's created once for the process (`WorkspaceWindow._instance`),
  where the specifications that open windows need one for each of them, as the Studying window has
  (`StudyingWindow.OpenWindowsByServices`). The headless JAStudio renders its workspace, but has no spaced repetition
  system, so what is shown only with one can't be seen there either.
- **(Claude, 2026-11-15) An automatic import's result shows in Anki's tooltip, though the Sync that started it was
  clicked in the workspace.** `AutomaticImportRunReports` says "The import profile … imported N sentences" in a
  passing message in Anki's window, from when an import in Anki started the run. Since increment 2 a Sync starts it,
  from the Sync button. The workspace could say it instead, such as beside the Sync button, where the reason a sync
  didn't run already shows.
- **(Claude, 2026-11-15) Each change JAStudio makes in Anki could be an Anki operation that Anki's windows are told
  of,** as its tag changes are (`aqt.operations.on_op_finished`). Anki's browser, reviewer and undo menu then follow
  it by themselves, which is the way Anki's developers recommend. Today the write of a study-note, suspending and
  unsuspending a note's cards, and deleting a study-note change the collection and tell no window.
  - *The sync's writes as one operation:* one call with every study-note to write (`col.update_notes`), which is one
    undo entry, "undo the sync", where each study-note is an entry of its own today, and one notification.
  - *Suspending many notes' cards as one operation,* for the same reasons: `BulkNoteChanges` suspends a note at a
    time.
  - *Deleting the note under review with its study-note:* told of the operation, Anki's reviewer moves to the next
    card. Untold, it keeps showing a card that no longer exists.
- **(Claude, 2026-11-15) The card that says JAStudio is still loading could be JAStudio's own page.** While the
  collection loads, Anki's card shows a text (`Mine.AppStillLoadingMessage`), and JAStudio has Anki draw its cards
  again when the load is done (`ISRSWindows.Refresh`), by poking the reviewer's and the previewer's private members.
  If the card were a page on the `CardServer` from the start, saying that JAStudio is loading, it could load the
  card itself when the collection has loaded, and that last refresh could go.
