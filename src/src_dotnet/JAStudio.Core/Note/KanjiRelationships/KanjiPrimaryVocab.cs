using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.KanjiTevents;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.KanjiRelationships;

/// <summary>
/// The vocab a kanji's page shows first among the vocab with the kanji, and marks as its primary vocab (<see cref="KanjiNote.PrimaryVocab"/>), in their<br/>
/// order: each named by a <see cref="WordReference"/>, as a vocab's related vocab are. A change publishes<br/>
/// <see cref="IKanjiTevent.PropertyUpdated.PrimaryVocab"/>, the whole new list, which the kanji applies.
/// </summary>
/// <remarks>
/// A kanji without primary vocab shows default ones (<see cref="KanjiNote.PrimaryVocabOrDefaults"/>). Renaming a vocab renames the words that named it here<br/>
/// too (<see cref="JPCollection.RenameTheWordReferences"/>).
/// </remarks>
public class KanjiPrimaryVocab
{
   readonly KanjiNote _kanji;

   /// <param name="kanji">The kanji the primary vocab are of.</param>
   /// <param name="words">The primary vocab as the kanji stores them.</param>
   public KanjiPrimaryVocab(KanjiNote kanji, IEnumerable<WordReference> words)
   {
      _kanji = kanji;
      Words = [.. words];
   }

   /// <summary>The words naming the primary vocab, in their order.</summary>
   public IReadOnlyList<WordReference> Words { get; private set; }

   public bool Contains(WordReference word) => Words.Contains(word);

   /// <summary>The vocab the <see cref="Words"/> refer to (<see cref="VocabCollection.ReferredToBy"/>), in the words' order.</summary>
   public List<VocabNote> Notes() => _kanji.Services.Collection.Vocab.ReferredToByAny(Words);

   /// <summary>Puts <paramref name="word"/> at <paramref name="index"/>, moving it there if the kanji has it already.</summary>
   public void PutAt(WordReference word, int index)
   {
      var words = WordsWithout(word);
      words.Insert(index, word);
      SetTheWords(words);
   }

   /// <summary>Puts <paramref name="word"/> after the others, moving it there if the kanji has it already.</summary>
   public void PutLast(WordReference word) => SetTheWords([.. WordsWithout(word), word]);

   public void Remove(WordReference word) => SetTheWords(WordsWithout(word));

   /// <summary>Replaces the primary vocab with <paramref name="words"/>, in their order.</summary>
   public void Set(IEnumerable<WordReference> words) => SetTheWords([.. words]);

   public void Clear() => Set([]);

   /// <summary>Replaces <paramref name="oldName"/>, the old name of a vocab that was renamed, with its new name, <paramref name="newName"/>, where the kanji has it.</summary>
   internal void Rename(WordReference oldName, WordReference newName) => SetTheWords([.. Words.Select(it => it == oldName ? newName : it)]);

   /// <summary>Makes <paramref name="words"/> the words naming the primary vocab: what their tevent's applier does.</summary>
   internal void Apply(IReadOnlyList<WordReference> words) => Words = words;

   /// <summary>The <see cref="Words"/>, with the first that is <paramref name="word"/> left out.</summary>
   List<WordReference> WordsWithout(WordReference word)
   {
      List<WordReference> words = [.. Words];
      words.Remove(word);
      return words;
   }

   void SetTheWords(IReadOnlyList<WordReference> words) => _kanji.PublishIfTheListChanged(Words, words, static it => new KanjiTevent.PrimaryVocabUpdated(it));

   public override string ToString() => string.Join(", ", Words.Select(it => it.DisambiguationName));
}
