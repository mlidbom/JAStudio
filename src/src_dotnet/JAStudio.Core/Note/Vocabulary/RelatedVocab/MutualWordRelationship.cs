using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Tevents;

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
   /// <param name="teventOf">The relationship's tevent: that its words are now those it's given.</param>
   /// <param name="theSameRelationshipIn">The same relationship in another vocab's related vocab, such as <c>it => it.Antonyms</c>.</param>
   public MutualWordRelationship(VocabNote vocab, IEnumerable<WordReference> words, Func<IReadOnlySet<WordReference>, NoteTevent> teventOf, Func<RelatedVocab, MutualWordRelationship> theSameRelationshipIn)
      : base(vocab, words, teventOf) => _theSameRelationshipIn = theSameRelationshipIn;

   /// <summary>Adds <paramref name="word"/>, and this vocab to the same relationship of the vocab <paramref name="word"/> refers to.</summary>
   public override void Add(WordReference word)
   {
      if(Vocab.Question.IsNamedBy(word)) return;

      AddTheWord(word);
      ChangeTheOtherSidesOf(word, it => it.AddTheWord(Vocab.Question.Reference));
   }

   /// <summary>Removes <paramref name="word"/>, and this vocab from the same relationship of the vocab <paramref name="word"/> refers to.</summary>
   public override void Remove(WordReference word)
   {
      RemoveTheWord(word);
      ChangeTheOtherSidesOf(word, it => it.RemoveTheWord(Vocab.Question.Reference));
   }

   /// <summary>
   /// Changes, with <paramref name="change"/>, the same relationship in each vocab <paramref name="word"/> refers to, but this one, each in an edit of that<br/>
   /// vocab (<see cref="EditingANote"/>): the other side of the relationship, for that word. A word that names no vocab exactly refers to the vocab with<br/>
   /// it as a form, which can be this vocab, as る is a form of <c>う:non-past</c>, and this vocab would otherwise become its own synonym.
   /// </summary>
   void ChangeTheOtherSidesOf(WordReference word, Action<MutualWordRelationship> change)
   {
      foreach(var otherVocab in Collection.ReferredToBy(word).Where(it => it.GetId() != Vocab.GetId()))
         otherVocab.Edit(it => change(_theSameRelationshipIn(it.RelatedNotes)));
   }
}
