using JAStudio.Core.Note.Vocabulary;
using Xunit;

namespace JAStudio.Core.Specifications.Note.Vocabulary;

/// <summary>
/// Tests ported from test_perfect_synonyms.py
/// </summary>
public class PerfectSynonymsTests : SpecificationStartingWithAnEmptyCollection
{
   [Fact]
   public void AnswerSyncsToSynonymOnAdd()
   {
      var first = GetService<VocabNoteFactory>().Create("first", "first_answer", []);
      var second = GetService<VocabNoteFactory>().Create("second", "second_answer", []);

      first.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(second.GetQuestion());
      Assert.Equal("first_answer", second.GetAnswer());
   }

   [Fact]
   public void AnswerSyncsToAddedSynonymOnUpdate()
   {
      var first = GetService<VocabNoteFactory>().Create("first", "first_answer", []);
      var second = GetService<VocabNoteFactory>().Create("second", "second_answer", []);

      first.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(second.GetQuestion());

      first.CorpusAnswer.Set("new answer");
      Assert.Equal("new answer", second.GetAnswer());

      var third = GetService<VocabNoteFactory>().Create("third", "third_answer", []);
      third.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(first.GetQuestion());
      Assert.Equal("third_answer", first.GetAnswer());
      Assert.Equal("third_answer", second.GetAnswer());

      third.CorpusAnswer.Set("third_new");
      Assert.Equal("third_new", first.GetAnswer());
      Assert.Equal("third_new", second.GetAnswer());

      Assert.Equal([second.GetQuestion(), third.GetQuestion()], first.RelatedNotes.PerfectSynonyms.Get());
      Assert.Equal([first.GetQuestion(), third.GetQuestion()], second.RelatedNotes.PerfectSynonyms.Get());
      Assert.Equal([first.GetQuestion(), second.GetQuestion()], third.RelatedNotes.PerfectSynonyms.Get());

      first.RelatedNotes.PerfectSynonyms.Remove(third.GetQuestion());
      Assert.Equal([second.GetQuestion()], first.RelatedNotes.PerfectSynonyms.Get());
      Assert.Equal([first.GetQuestion()], second.RelatedNotes.PerfectSynonyms.Get());
      Assert.Empty(third.RelatedNotes.PerfectSynonyms.Get());

      first.CorpusAnswer.Set("first_latest");
      Assert.Equal("first_latest", first.GetAnswer());
      Assert.Equal("first_latest", second.GetAnswer());
      Assert.Equal("third_new", third.GetAnswer());
   }

   [Fact]
   public void PerfectSynonymsAreKeptInSyncOnAdd()
   {
      var first = GetService<VocabNoteFactory>().Create("first", "", []);
      var second = GetService<VocabNoteFactory>().Create("second", "", []);
      first.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(second.GetQuestion());

      Assert.Equal(["second"], first.RelatedNotes.PerfectSynonyms.Get());
      Assert.Equal(["first"], second.RelatedNotes.PerfectSynonyms.Get());

      var third = GetService<VocabNoteFactory>().Create("third", "", []);
      var fourth = GetService<VocabNoteFactory>().Create("fourth", "", []);
      second.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(third.GetQuestion());
      second.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(fourth.GetQuestion());

      Assert.Equal(["second", "third", "fourth"], first.RelatedNotes.PerfectSynonyms.Get());
      Assert.Equal(["first", "third", "fourth"], second.RelatedNotes.PerfectSynonyms.Get());
      Assert.Equal(["first", "second", "fourth"], third.RelatedNotes.PerfectSynonyms.Get());
      Assert.Equal(["first", "second", "third"], fourth.RelatedNotes.PerfectSynonyms.Get());
   }
}
