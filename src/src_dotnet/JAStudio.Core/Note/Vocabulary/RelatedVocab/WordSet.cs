using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

/// <summary>
/// A set of words a vocab stores, each named by a <see cref="WordReference"/>, such as the words of one of its relationships (<see cref="WordRelationship"/>)<br/>
/// or its <see cref="PerfectSynonyms"/>. Every change saves the vocab, through its <see cref="NoteGuard"/>.
/// </summary>
/// <remarks>
/// It changes only this vocab's set: what a change means beyond it, such as adding the vocab to the other side of a relationship that goes both ways,<br/>
/// is for the relationship holding the set to do.
/// </remarks>
sealed class WordSet
{
   readonly HashSet<WordReference> _words;
   readonly NoteGuard _guard;

   /// <param name="words">The words the vocab stores.</param>
   /// <param name="guard">Saves the vocab when the set changes.</param>
   public WordSet(IEnumerable<WordReference> words, NoteGuard guard)
   {
      _words = [.. words];
      _guard = guard;
   }

   public IReadOnlySet<WordReference> Words => _words;

   public bool Contains(WordReference word) => _words.Contains(word);

   public void Add(WordReference word)
   {
      if(Contains(word)) return;
      _guard.Update(() => _words.Add(word));
   }

   public void Remove(WordReference word)
   {
      if(!Contains(word)) return;
      _guard.Update(() => _words.Remove(word));
   }

   public void Clear() => _guard.Update(_words.Clear);

   /// <summary>Replaces <paramref name="oldName"/>, the old name of a vocab that was renamed, with its new name, <paramref name="newName"/>, if the set has it.</summary>
   public void Rename(WordReference oldName, WordReference newName)
   {
      if(!Contains(oldName)) return;

      _guard.Update(() =>
      {
         _words.Remove(oldName);
         _words.Add(newName);
      });
   }

   public override string ToString() => string.Join(", ", _words.Select(it => it.DisambiguationName));
}
