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
