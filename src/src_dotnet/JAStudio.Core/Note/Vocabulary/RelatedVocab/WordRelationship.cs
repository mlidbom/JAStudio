using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

/// <summary>
/// A vocab's relationship to other words, such as the words it's confused with (<see cref="RelatedVocab.ConfusedWith"/>): the <see cref="Words"/>,<br/>
/// each named by a <see cref="WordReference"/>. It goes one way: the other words' vocab don't know of it. <see cref="MutualWordRelationship"/> goes both ways.
/// </summary>
public class WordRelationship
{
   readonly HashSet<WordReference> _words;

   public WordRelationship(VocabNote vocab, IEnumerable<WordReference> words, NoteGuard guard)
   {
      Vocab = vocab;
      Guard = guard;
      _words = [.. words];
   }

   /// <summary>The vocab whose relationship this is.</summary>
   protected VocabNote Vocab { get; }

   protected NoteGuard Guard { get; }

   protected VocabCollection Collection => Vocab.Services.Collection.Vocab;

   /// <summary>The words the vocab has this relationship to.</summary>
   public IReadOnlySet<WordReference> Words => _words;

   public bool Contains(WordReference word) => _words.Contains(word);

   /// <summary>The vocab notes the <see cref="Words"/> refer to (<see cref="VocabCollection.ReferredToBy"/>).</summary>
   public List<VocabNote> Notes() => Collection.ReferredToByAny(_words);

   /// <summary>Adds <paramref name="word"/>. A vocab has no relationship to itself, so adding its own word does nothing.</summary>
   public virtual void Add(WordReference word)
   {
      if(Vocab.Question.IsNamedBy(word)) return;
      AddTheWord(word);
   }

   public virtual void Remove(WordReference word) => RemoveTheWord(word);

   /// <summary>Replaces <paramref name="oldName"/>, the old name of a vocab that was renamed, with its new name, <paramref name="newName"/>, if the relationship has it.</summary>
   internal void Rename(WordReference oldName, WordReference newName)
   {
      if(!Contains(oldName)) return;

      Guard.Update(() =>
      {
         _words.Remove(oldName);
         _words.Add(newName);
      });
   }

   /// <summary>Adds <paramref name="word"/> to this vocab's side of the relationship only.</summary>
   protected void AddTheWord(WordReference word)
   {
      if(Contains(word)) return;
      Guard.Update(() => _words.Add(word));
   }

   /// <summary>Removes <paramref name="word"/> from this vocab's side of the relationship only.</summary>
   protected void RemoveTheWord(WordReference word)
   {
      if(!Contains(word)) return;
      Guard.Update(() => _words.Remove(word));
   }

   public override string ToString() => string.Join(", ", _words.Select(it => it.DisambiguationName));
}
