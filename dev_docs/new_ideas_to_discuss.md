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
- **(Claude, 2026-11-13) A tokenizer dictionary that knows which word a kana spelling is: UniDic.** JAStudio's MeCab
  reads with IPADIC, which gives ひとつ as a noun whose base form is ひとつ, and no more. UniDic, the dictionary MeCab is
  mostly used with today, gives each token a lemma (語彙素) that unites a word's spellings, chosen in context: ひとつ → 一つ,
  ボク → 僕, きょう → 今日. Switching would be a very large change, since every part of speech, inflection type and
  tokenizer condition in the matching configuration speaks IPADIC; running UniDic beside IPADIC for its lemmas alone
  would cost a second tokenization and a large dictionary. Not verified: how well it does on anime dialogue, and how it
  fits the .NET MeCab JAStudio uses. Worth an experiment over the reservoir, as the kana-readings one was.
  - **Checked (2026-11-13), with unidic-lite, UniDic 2.1.2:** MeCab.DotNet 1.2.0, JAStudio's engine, loads it and
    parses as fast as with IPADIC. Its lemmas unite kana and colloquial spellings: ボク → 僕, きょう → 今日, いま → 今,
    いっしょ → 一緒, しずか → 静か, アタシ → 私, ムリ → 無理, みやげ → 土産, すげえ → 凄い. It tags names with their
    reading (河合: surname, カワイ; 翔: given name; 戦場ヶ原: place). But not by meaning: かみ is always 髪, にる always
    煮る, even in 父ににている; 渡さん came out as the verb 渡す. It segments more finely: ひと + つ, お + みやげ. Each token
    also has the word's origin (native, Sino-Japanese, foreign, proper noun) and its pitch accent type.
  - **IPADIC hasn't changed since 2007;** UniDic is NINJAL's, the one maintained, at 3.1 (2021), about 770 MB on disk
    (unidic-lite: 249 MB). JAStudio has IPADIC presumably from its roots in Janome, which bundles it.
  - **Magnus, 2026-11-13:** "it sure as f-ing hell sounds like we are using the wrong dictionary with MeCab"; no time
    will be easier than now. Under discussion.
- **(Claude, 2026-11-13) Keep what JMdict and JMnedict say that `jmdict.db` drops.** The database keeps a word's
  spellings, readings, priorities and senses, and a name's spellings, readings and translations. It drops what tells a
  kana spelling's standing (a reading's irregular, outdated or search-only use; that it isn't a true reading of the
  kanji; which kanji spellings it belongs to), which spellings a sense is restricted to, and a name's type (surname,
  given name, place, company, product). Kana forms and names would both use them.
