using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

/// <summary>A vocab's synonyms: words with about the same meaning, linked both ways (<see cref="MutualWordRelationship"/>).</summary>
public class Synonyms : MutualWordRelationship
{
   public Synonyms(VocabNote vocab, IEnumerable<WordReference> words, NoteGuard guard) : base(vocab, words, guard, it => it.Synonyms) {}

   /// <summary>
   /// Adds <paramref name="synonym"/>, and links the two vocab's synonyms one level deep: the vocab <paramref name="synonym"/> refers to get this vocab's<br/>
   /// synonyms, and this vocab gets theirs.
   /// </summary>
   public void AddTransitivelyOneLevel(WordReference synonym)
   {
      var newSynonymNotes = Collection.ReferredToBy(synonym);

      foreach(var synonymNote in newSynonymNotes)
      {
         foreach(var mySynonym in Words.ToList())
            synonymNote.RelatedNotes.Synonyms.Add(mySynonym);
      }

      var synonymsOfTheNewSynonyms = newSynonymNotes
                                    .SelectMany(it => it.RelatedNotes.Synonyms.Words)
                                    .Concat(newSynonymNotes.Select(it => it.Question.Reference))
                                    .ToHashSet();

      foreach(var newSynonym in synonymsOfTheNewSynonyms)
         Add(newSynonym);
   }
}
