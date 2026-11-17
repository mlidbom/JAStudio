using System;
using System.Collections.Generic;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

/// <summary>
/// A vocab's relationship to other words, such as the words it's confused with (<see cref="RelatedVocab.ConfusedWith"/>): the <see cref="Words"/>,<br/>
/// each named by a <see cref="WordReference"/>. It goes one way: the other words' vocab don't know of it. <see cref="MutualWordRelationship"/> goes both ways.
/// </summary>
public class WordRelationship
{
   readonly WordSet _words;

   /// <param name="vocab">The vocab whose relationship this is.</param>
   /// <param name="words">The words the vocab has the relationship to.</param>
   /// <param name="teventOf">The relationship's tevent: that its words are now those it's given.</param>
   public WordRelationship(VocabNote vocab, IEnumerable<WordReference> words, Func<IReadOnlySet<WordReference>, NoteTevent> teventOf)
   {
      Vocab = vocab;
      _words = new WordSet(vocab, words, teventOf);
   }

   /// <summary>The vocab whose relationship this is.</summary>
   protected VocabNote Vocab { get; }

   protected VocabCollection Collection => Vocab.Services.Collection.Vocab;

   /// <summary>The words the vocab has this relationship to.</summary>
   public IReadOnlySet<WordReference> Words => _words.Words;

   public bool Contains(WordReference word) => _words.Contains(word);

   /// <summary>The vocab notes the <see cref="Words"/> refer to (<see cref="VocabCollection.ReferredToBy"/>).</summary>
   public List<VocabNote> Notes() => Collection.ReferredToByAny(Words);

   /// <summary>Adds <paramref name="word"/>. A vocab has no relationship to itself, so adding its own word does nothing.</summary>
   public virtual void Add(WordReference word)
   {
      if(Vocab.Question.IsNamedBy(word)) return;
      AddTheWord(word);
   }

   public virtual void Remove(WordReference word) => RemoveTheWord(word);

   /// <summary>Replaces <paramref name="oldName"/>, the old name of a vocab that was renamed, with its new name, <paramref name="newName"/>, if the relationship has it.</summary>
   internal void Rename(WordReference oldName, WordReference newName) => _words.Rename(oldName, newName);

   /// <summary>Makes <paramref name="words"/> the relationship's words: what its tevent's applier does.</summary>
   internal void Apply(IReadOnlySet<WordReference> words) => _words.Apply(words);

   /// <summary>Adds <paramref name="word"/> to this vocab's side of the relationship only.</summary>
   protected void AddTheWord(WordReference word) => _words.Add(word);

   /// <summary>Removes <paramref name="word"/> from this vocab's side of the relationship only.</summary>
   protected void RemoveTheWord(WordReference word) => _words.Remove(word);

   /// <summary>Adds <paramref name="word"/> to this vocab's side of the relationship only, publishing the tevent <paramref name="teventOf"/> makes of it and the new set.</summary>
   protected void AddTheWord(WordReference word, Func<WordReference, IReadOnlySet<WordReference>, NoteTevent> teventOf) => _words.Add(word, words => teventOf(word, words));

   /// <summary>Removes <paramref name="word"/> from this vocab's side of the relationship only, publishing the tevent <paramref name="teventOf"/> makes of it and the new set.</summary>
   protected void RemoveTheWord(WordReference word, Func<WordReference, IReadOnlySet<WordReference>, NoteTevent> teventOf) => _words.Remove(word, words => teventOf(word, words));

   public override string ToString() => _words.ToString();
}
