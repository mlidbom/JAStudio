using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Vocabulary;

public static class VocabNoteSorting
{
   public static List<VocabNote> SortVocabListByStudyingStatus(
      IEnumerable<VocabNote> vocabs,
      IReadOnlyList<VocabNote>? primaryVocab = null,
      string? preferredKanji = null)
   {
      var primaryVocabInOrder = primaryVocab?.ToList() ?? [];

      //refactor: The name says it sorts by studying status, which it doesn't look at: it sorts the vocab with preferredKanji first, then by their place among
      //   primaryVocab, then those with the most sentences first, then by question. Its callers, the vocab's related vocab components among them, would read
      //   truer with a name that says that.
      return
      [
         .. vocabs.OrderBy(PreferVocabWithKanji)
                  .ThenBy(PreferPrimaryVocabInOrder)
                  .ThenBy(PreferMoreSentences)
                  .ThenBy(it => it.GetQuestion())
      ];

      int PreferVocabWithKanji(VocabNote vocab) => preferredKanji == null || vocab.GetQuestion().Contains(preferredKanji) ? 0 : 1;

      int PreferPrimaryVocabInOrder(VocabNote vocab)
      {
         var index = primaryVocabInOrder.IndexOf(vocab);
         return index == -1 ? 1000 : index;
      }

      int PreferMoreSentences(VocabNote vocab) => -vocab.Sentences.All().Count;
   }
}
