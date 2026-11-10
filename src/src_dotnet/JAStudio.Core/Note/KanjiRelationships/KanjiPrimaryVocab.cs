using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.KanjiRelationships;

/// <summary>
/// The vocab a kanji's page shows first among the vocab with the kanji, and marks as its primary vocab (<see cref="KanjiNote.PrimaryVocab"/>), in their<br/>
/// order: each named by a <see cref="WordReference"/>, as a vocab's related vocab are. Every change saves the kanji, through its <see cref="NoteGuard"/>.
/// </summary>
/// <remarks>
/// A kanji without primary vocab shows default ones (<see cref="KanjiNote.PrimaryVocabOrDefaults"/>). Renaming a vocab renames the words that named it here<br/>
/// too (<see cref="JPCollection.RenameTheWordReferences"/>).
/// </remarks>
public class KanjiPrimaryVocab
{
   readonly KanjiNote _kanji;
   readonly List<WordReference> _words;
   readonly NoteGuard _guard;

   /// <param name="kanji">The kanji the primary vocab are of.</param>
   /// <param name="words">The primary vocab as the kanji stores them.</param>
   /// <param name="guard">Saves the kanji when its primary vocab change.</param>
   public KanjiPrimaryVocab(KanjiNote kanji, IEnumerable<WordReference> words, NoteGuard guard)
   {
      _kanji = kanji;
      _words = [.. words];
      _guard = guard;
   }

   /// <summary>The words naming the primary vocab, in their order.</summary>
   public IReadOnlyList<WordReference> Words => _words;

   public bool Contains(WordReference word) => _words.Contains(word);

   /// <summary>The vocab the <see cref="Words"/> refer to (<see cref="VocabCollection.ReferredToBy"/>), in the words' order.</summary>
   public List<VocabNote> Notes() => _kanji.Services.Collection.Vocab.ReferredToByAny(_words);

   /// <summary>Puts <paramref name="word"/> at <paramref name="index"/>, moving it there if the kanji has it already.</summary>
   public void PutAt(WordReference word, int index) => _guard.Update(() =>
   {
      _words.Remove(word);
      _words.Insert(index, word);
   });

   /// <summary>Puts <paramref name="word"/> after the others, moving it there if the kanji has it already.</summary>
   public void PutLast(WordReference word) => _guard.Update(() =>
   {
      _words.Remove(word);
      _words.Add(word);
   });

   public void Remove(WordReference word)
   {
      if(!Contains(word)) return;
      _guard.Update(() => _words.Remove(word));
   }

   /// <summary>Replaces the primary vocab with <paramref name="words"/>, in their order.</summary>
   public void Set(IEnumerable<WordReference> words) => _guard.Update(() =>
   {
      _words.Clear();
      _words.AddRange(words);
   });

   public void Clear() => Set([]);

   /// <summary>Replaces <paramref name="oldName"/>, the old name of a vocab that was renamed, with its new name, <paramref name="newName"/>, where the kanji has it.</summary>
   internal void Rename(WordReference oldName, WordReference newName)
   {
      var index = _words.IndexOf(oldName);
      if(index == -1) return;
      _guard.Update(() => _words[index] = newName);
   }

   public override string ToString() => string.Join(", ", _words.Select(it => it.DisambiguationName));
}
