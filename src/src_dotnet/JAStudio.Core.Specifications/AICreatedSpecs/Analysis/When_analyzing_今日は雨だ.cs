using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Analysis;

/// <summary>
/// The analysis builds a candidate word of several tokens only when a vocab form or a dictionary word form starts with the surface of all its<br/>
/// tokens but the last: any other could match nothing. 今日は雨だ's tokens are 今日, は, 雨 and だ, and no dictionary word form starts with は雨,<br/>
/// so only a vocab with a form starting with は雨 makes the analysis build the candidate word は雨だ.
/// </summary>
public class When_analyzing_今日は雨だ : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected List<string> SurfacesOfTheCandidateWords() =>
      [.. CreateSentence("今日は雨だ").CreateAnalysis().Locations.SelectMany(it => it.CandidateWords).Select(it => it.SurfaceForm)];

   public class with_no_vocab : When_analyzing_今日は雨だ
   {
      readonly List<string> _candidateWords;
      public with_no_vocab() => _candidateWords = SurfacesOfTheCandidateWords();

      [XF] public void it_builds_a_candidate_word_of_each_token() => _candidateWords.Must().Contain("今日").Contain("は").Contain("雨").Contain("だ");
      [XF] public void it_builds_no_candidate_word_は雨だ() => _candidateWords.Contains("は雨だ").Must().BeFalse(); //todo:compze: Compze.Must has no NotContain for collections, only for strings; use it once it does
   }

   public class with_a_vocab_は雨だ : When_analyzing_今日は雨だ
   {
      protected readonly VocabNote Vocab;
      readonly List<string> _candidateWords;

      public with_a_vocab_は雨だ()
      {
         Vocab = CreateVocab("は雨だ", "it rains");
         _candidateWords = SurfacesOfTheCandidateWords();
      }

      [XF] public void it_builds_the_candidate_word_は雨だ() => _candidateWords.Must().Contain("は雨だ");

      public class after_the_vocab_is_deleted : with_a_vocab_は雨だ
      {
         readonly List<string> _candidateWordsAfterTheDeletion;

         public after_the_vocab_is_deleted()
         {
            GetService<JPCollection>().Delete(Vocab, ItsAnkiNote.IsDeleted);
            _candidateWordsAfterTheDeletion = SurfacesOfTheCandidateWords();
         }

         [XF] public void it_builds_no_candidate_word_は雨だ() => _candidateWordsAfterTheDeletion.Contains("は雨だ").Must().BeFalse(); //todo:compze: Compze.Must has no NotContain for collections, only for strings; use it once it does
      }
   }
}
