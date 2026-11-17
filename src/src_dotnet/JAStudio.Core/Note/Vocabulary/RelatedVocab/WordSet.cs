using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

/// <summary>
/// A set of words a vocab stores, each named by a <see cref="WordReference"/>, such as the words of one of its relationships (<see cref="WordRelationship"/>)<br/>
/// or its <see cref="PerfectSynonyms"/>. A change publishes the set's tevent with the whole new set, which the vocab's related vocab applies<br/>
/// (<see cref="Apply"/>).
/// </summary>
/// <remarks>
/// It changes only this vocab's set: what a change means beyond it, such as adding the vocab to the other side of a relationship that goes both ways,<br/>
/// is for the relationship holding the set to do.
/// </remarks>
sealed class WordSet
{
   readonly VocabNote _vocab;
   readonly Func<IReadOnlySet<WordReference>, NoteTevent> _teventOf;

   /// <param name="vocab">The vocab that stores the set, which publishes its tevent.</param>
   /// <param name="words">The words the vocab stores.</param>
   /// <param name="teventOf">The set's tevent: that its words are now those it's given.</param>
   public WordSet(VocabNote vocab, IEnumerable<WordReference> words, Func<IReadOnlySet<WordReference>, NoteTevent> teventOf)
   {
      _vocab = vocab;
      _teventOf = teventOf;
      Words = words.ToHashSet();
   }

   public IReadOnlySet<WordReference> Words { get; private set; }

   public bool Contains(WordReference word) => Words.Contains(word);

   public void Add(WordReference word) => SetTheWords([.. Words, word]);

   public void Remove(WordReference word) => SetTheWords(Words.Where(it => it != word));

   public void Clear() => SetTheWords([]);

   /// <summary>Replaces <paramref name="oldName"/>, the old name of a vocab that was renamed, with its new name, <paramref name="newName"/>, if the set has it.</summary>
   public void Rename(WordReference oldName, WordReference newName)
   {
      if(!Contains(oldName)) return;
      SetTheWords([.. Words.Where(it => it != oldName), newName]);
   }

   /// <summary>Makes <paramref name="words"/> the set's words: what the set's tevent's applier does.</summary>
   public void Apply(IReadOnlySet<WordReference> words) => Words = words;

   void SetTheWords(IEnumerable<WordReference> words) => _vocab.PublishIfTheSetChanged(Words, words.ToHashSet(), _teventOf);

   public override string ToString() => string.Join(", ", Words.Select(it => it.DisambiguationName));
}
