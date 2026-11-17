using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary.Tevents;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

/// <summary>A vocab's synonyms: words with about the same meaning, linked both ways (<see cref="MutualWordRelationship"/>).</summary>
public class Synonyms : MutualWordRelationship
{
   public Synonyms(VocabNote vocab, IEnumerable<WordReference> words)
      : base(vocab, words, static it => new VocabTevent.RelatedVocab.SynonymsUpdated(it), it => it.Synonyms) {}

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
         var mySynonyms = Words.ToList();
         foreach(var synonymVocab in theSynonymsVocab)
         {
            synonymVocab.Edit(it =>
            {
               foreach(var mySynonym in mySynonyms)
                  it.RelatedNotes.Synonyms.Add(mySynonym);
            });
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
