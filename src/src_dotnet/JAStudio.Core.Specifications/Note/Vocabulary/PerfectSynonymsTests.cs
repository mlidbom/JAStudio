using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using Xunit;
using static JAStudio.Core.Specifications.WordReferenceNames;

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

      first.Edit(it => it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(second.Question.Reference));
      Assert.Equal("first_answer", second.GetAnswer());
   }

   [Fact]
   public void AnswerSyncsToAddedSynonymOnUpdate()
   {
      var first = GetService<VocabNoteFactory>().Create("first", "first_answer", []);
      var second = GetService<VocabNoteFactory>().Create("second", "second_answer", []);

      first.Edit(it => it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(second.Question.Reference));

      first.Edit(it => it.CorpusAnswer.Set("new answer"));
      Assert.Equal("new answer", second.GetAnswer());

      var third = GetService<VocabNoteFactory>().Create("third", "third_answer", []);
      third.Edit(it => it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(first.Question.Reference));
      Assert.Equal("third_answer", first.GetAnswer());
      Assert.Equal("third_answer", second.GetAnswer());

      third.Edit(it => it.CorpusAnswer.Set("third_new"));
      Assert.Equal("third_new", first.GetAnswer());
      Assert.Equal("third_new", second.GetAnswer());

      Assert.Equal([second.GetQuestion(), third.GetQuestion()], SortedNamesOf(first.RelatedNotes.PerfectSynonyms.Words));
      Assert.Equal([first.GetQuestion(), third.GetQuestion()], SortedNamesOf(second.RelatedNotes.PerfectSynonyms.Words));
      Assert.Equal([first.GetQuestion(), second.GetQuestion()], SortedNamesOf(third.RelatedNotes.PerfectSynonyms.Words));

      first.RelatedNotes.PerfectSynonyms.Remove(third.Question.Reference);
      Assert.Equal([second.GetQuestion()], SortedNamesOf(first.RelatedNotes.PerfectSynonyms.Words));
      Assert.Equal([first.GetQuestion()], SortedNamesOf(second.RelatedNotes.PerfectSynonyms.Words));
      Assert.Empty(SortedNamesOf(third.RelatedNotes.PerfectSynonyms.Words));

      first.Edit(it => it.CorpusAnswer.Set("first_latest"));
      Assert.Equal("first_latest", first.GetAnswer());
      Assert.Equal("first_latest", second.GetAnswer());
      Assert.Equal("third_new", third.GetAnswer());
   }

   [Fact]
   public void PerfectSynonymsAreKeptInSyncOnAdd()
   {
      var first = GetService<VocabNoteFactory>().Create("first", "", []);
      var second = GetService<VocabNoteFactory>().Create("second", "", []);
      first.Edit(it => it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(second.Question.Reference));

      Assert.Equal(["second"], SortedNamesOf(first.RelatedNotes.PerfectSynonyms.Words));
      Assert.Equal(["first"], SortedNamesOf(second.RelatedNotes.PerfectSynonyms.Words));

      var third = GetService<VocabNoteFactory>().Create("third", "", []);
      var fourth = GetService<VocabNoteFactory>().Create("fourth", "", []);
      second.Edit(it =>
      {
         it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(third.Question.Reference);
         it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(fourth.Question.Reference);
      });

      Assert.Equal(["fourth", "second", "third"], SortedNamesOf(first.RelatedNotes.PerfectSynonyms.Words));
      Assert.Equal(["first", "fourth", "third"], SortedNamesOf(second.RelatedNotes.PerfectSynonyms.Words));
      Assert.Equal(["first", "fourth", "second"], SortedNamesOf(third.RelatedNotes.PerfectSynonyms.Words));
      Assert.Equal(["first", "second", "third"], SortedNamesOf(fourth.RelatedNotes.PerfectSynonyms.Words));
   }
}
