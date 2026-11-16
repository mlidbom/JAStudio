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
- **(Magnus, 2026-11-16) The notes as Compze Taggregates,** so that every change to a note is a tevent, and whatever
  follows a change subscribes to exactly the tevents it concerns. Today `NoteGuard`, `JPNote.Flush` and
  `JPCollection.NoteUpdated` say only that a note changed, and each listener works out the rest. The storage stays as
  it is: a note's file holds its state, not its history. *Claude explored it the same day:*
  - *It fits.* A note's stored state can be one tevent that is every property update at once, as a creation tevent is
    in Compze's own examples, so loading a note and changing it run the same tevent appliers. `ITaggregate.Commit` is
    what `Flush` is today: the tevents not yet committed are what isn't saved yet, with no hash of the whole note.
  - *What it replaces:* the hand-written calls between a note's parts (`VocabNoteForms.OnQuestionChanged`,
    `OnMatchingConfigurationChanged`); `PerfectSynonyms` subscribing to two `WritableStringValue`s;
    `VocabNoteQuestion` renaming word-references through the collection; a vocab's whole `VocabSnapshot` rebuilt for
    all nine indexes at every change; and the UI's editors each being handed the callback that reparses sentences,
    which is where "this change affects matching" is recorded today.
  - *What Compze needs first, measured with Compze.Teventive 0.6.0-alpha:* a Taggregate creates its tevent
    dispatchers for each instance. One with 4 tevent appliers keeps 1.6 KB, and each component about 1.0 KB more,
    against 350 bytes for the same data as plain objects: for the corpus's 80,000 notes that is several hundred MB to
    about 1 GB, on a heap of 733 MB (`memory-use.md`). Routing decided once for each Taggregate class would take it
    away. And a Taggregate can't be restored from stored state outside Compze's tevent store without a member marked
    for Compze's internal use.
  - *Smaller, in Compze:* `ITaggregateTevent` is an `IExactlyOnceTevent`, which a note's tevents aren't; and each
    tevent needs an interface and a class that repeats it, which a source generator could write.
  - *Worked out with a Compze session the same day,* at Magnus's word: what Compze.Teventive should change, and how, is
    in Compze's `dev_docs/taggregate-improvements/fable/`, starting at its `README.md`, for Magnus to decide on. In
    short: a static declaration of the tevent appliers for each class; a note loaded from a compacted-history, the
    tevents its own code makes from its stored state; a protected hook that tells the note's class it has published;
    and `ITaggregateTevent` as a plain `ITevent`.
  - *What that dialog found about the notes:* the same class of part several times in a note is the normal case (six
    word sets and four media reference lists in a vocab, three reading lists in a kanji), so the notes need Compze's
    shared tomponents throughout, and a vocab loads from about eleven tevents. No part needs a tevent handler of its
    own. A sentence's root needs one: a change to its configuration reparses it
    (`CachingSentenceConfigurationField`).
  - *A step on JAStudio's side that waits for nothing, deferred by Magnus the same day* while the changes to Compze
    are explored: one note type's tevent interfaces, interfaces only, written to be read.
- **(Claude, 2026-11-15) The card that says JAStudio is still loading could be JAStudio's own page.** While the
  collection loads, Anki's card shows a text (`Mine.AppStillLoadingMessage`), and JAStudio has Anki draw its cards
  again when the load is done (`ISRSWindows.Refresh`), by poking the reviewer's and the previewer's private members.
  If the card were a page on the `CardServer` from the start, saying that JAStudio is loading, it could load the
  card itself when the collection has loaded, and that last refresh could go.
