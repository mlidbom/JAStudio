using System;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

/// <summary>
/// A vocab's relationship to other words that goes both ways, such as its synonyms: adding a word adds the vocab to the same relationship of the vocab<br/>
/// the word refers to, and removing one removes it there too.
/// </summary>
/// <remarks>
/// The other side is every vocab the word refers to (<see cref="Collection.VocabCollection.ReferredToBy"/>), the vocab <see cref="WordRelationship.Notes"/><br/>
/// shows, so the relationship shows the same from both sides. The vocab is added there by its own name (<see cref="VocabNoteQuestion.Reference"/>),<br/>
/// disambiguation included.
/// </remarks>
public class MutualWordRelationship : WordRelationship
{
   readonly Func<RelatedVocab, MutualWordRelationship> _theSameRelationshipIn;

   /// <param name="vocab">The vocab whose relationship this is.</param>
   /// <param name="words">The words the vocab has the relationship to.</param>
   /// <param name="guard">Saves the vocab when the relationship changes.</param>
   /// <param name="theSameRelationshipIn">The same relationship in another vocab's related vocab, such as <c>it => it.Antonyms</c>.</param>
   public MutualWordRelationship(VocabNote vocab, IEnumerable<WordReference> words, NoteGuard guard, Func<RelatedVocab, MutualWordRelationship> theSameRelationshipIn)
      : base(vocab, words, guard) => _theSameRelationshipIn = theSameRelationshipIn;

   /// <summary>Adds <paramref name="word"/>, and this vocab to the same relationship of the vocab <paramref name="word"/> refers to.</summary>
   public override void Add(WordReference word)
   {
      if(Vocab.Question.IsNamedBy(word)) return;

      AddTheWord(word);
      foreach(var otherSide in OtherSidesOf(word))
         otherSide.AddTheWord(Vocab.Question.Reference);
   }

   /// <summary>Removes <paramref name="word"/>, and this vocab from the same relationship of the vocab <paramref name="word"/> refers to.</summary>
   public override void Remove(WordReference word)
   {
      RemoveTheWord(word);
      foreach(var otherSide in OtherSidesOf(word))
         otherSide.RemoveTheWord(Vocab.Question.Reference);
   }

   /// <summary>The same relationship in each vocab <paramref name="word"/> refers to: the other side of the relationship, for that word.</summary>
   IEnumerable<MutualWordRelationship> OtherSidesOf(WordReference word) => Collection.ReferredToBy(word).Select(it => _theSameRelationshipIn(it.RelatedNotes));
}
