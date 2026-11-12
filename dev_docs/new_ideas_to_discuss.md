# New ideas to discuss

Ideas Magnus or Claude want to discuss, written down when they occur, so that none is lost (Magnus, 2026-11-11). They're
ideas that came up while working on something else, or that a code comment can't hold because they're about no one
piece of code, or that would otherwise be lost in a long response.

- **Each idea says who had it and when,** and enough to be understood without the conversation it came from.
- **Once discussed, an idea moves to the document it belongs in,** or is dropped, and goes from here.

## Ideas

- **(Claude, 2026-11-11) The match-condition's tree, checked against a word in a sentence.** When a vocab matches a word
  it shouldn't, or misses one it should, the question is which of its conditions decided it. The tree editor could take
  a sentence and a place in it, such as from the word's line in the sentence's breakdown, and mark each line of the tree
  met or not met for that word. Editing a condition then shows at once whether that word now matches. It would make
  editing conditions while studying much faster than reading the failure reasons in the breakdown.
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
- **(Claude, 2026-11-12) Parsing's first run after a start is about 4 times slower: does the full rebuild pay that?**
  In the headless JAStudio, analyzing every sentence on 32 threads takes 2.1-2.7 s the first time after a start, and
  0.42-0.59 s from then on. The process used 65 processor-seconds for the first run against about 19 for a later one,
  with nothing else busy, so the extra is work the process does, most likely the JIT's tiers (the first runs execute
  unoptimized code while the optimized code is compiled on a background thread that 32 busy threads starve), maybe
  also caches filling. A full rebuild in Anki runs once, perhaps cold, so it may pay that warm-up in full. Worth
  measuring a cold full rebuild, and if it's so, trying ReadyToRun or the runtime's tiering settings. Also, the times of
  the same code vary by about a third from one start to the next while steady within a start, and why is unknown.
