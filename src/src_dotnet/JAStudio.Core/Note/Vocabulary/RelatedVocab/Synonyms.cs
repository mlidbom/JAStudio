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
      var theSynonymsVocab = Collection.ReferredToBy(synonym);
      GiveEachThisVocabsSynonyms();
      AddEachWithItsSynonyms();
      return;

      void GiveEachThisVocabsSynonyms()
      {
         foreach(var synonymVocab in theSynonymsVocab)
         {
            foreach(var mySynonym in Words.ToList())
               synonymVocab.RelatedNotes.Synonyms.Add(mySynonym);
         }
      }

      void AddEachWithItsSynonyms()
      {
         var theirNamesAndSynonyms = theSynonymsVocab.SelectMany(it => it.RelatedNotes.Synonyms.Words)
                                                     .Concat(theSynonymsVocab.Select(it => it.Question.Reference))
                                                     .ToHashSet();
         foreach(var newSynonym in theirNamesAndSynonyms)
            Add(newSynonym);
      }
   }
}
