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
      var firstReference = GetService<VocabNoteFactory>().Create("first", "first_answer", []).Reference();
      var secondReference = GetService<VocabNoteFactory>().Create("second", "second_answer", []).Reference();

      firstReference.Current.Edit(it => it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(secondReference.Current.Question.Reference));
      Assert.Equal("first_answer", secondReference.Current.GetAnswer());
   }

   [Fact]
   public void AnswerSyncsToAddedSynonymOnUpdate()
   {
      var firstReference = GetService<VocabNoteFactory>().Create("first", "first_answer", []).Reference();
      var secondReference = GetService<VocabNoteFactory>().Create("second", "second_answer", []).Reference();

      firstReference.Current.Edit(it => it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(secondReference.Current.Question.Reference));

      firstReference.Current.Edit(it => it.SetCorpusAnswer("new answer"));
      Assert.Equal("new answer", secondReference.Current.GetAnswer());

      var thirdReference = GetService<VocabNoteFactory>().Create("third", "third_answer", []).Reference();
      thirdReference.Current.Edit(it => it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(firstReference.Current.Question.Reference));
      Assert.Equal("third_answer", firstReference.Current.GetAnswer());
      Assert.Equal("third_answer", secondReference.Current.GetAnswer());

      thirdReference.Current.Edit(it => it.SetCorpusAnswer("third_new"));
      Assert.Equal("third_new", firstReference.Current.GetAnswer());
      Assert.Equal("third_new", secondReference.Current.GetAnswer());

      Assert.Equal([secondReference.Current.GetQuestion(), thirdReference.Current.GetQuestion()], SortedNamesOf(firstReference.Current.RelatedNotes.PerfectSynonyms.Words));
      Assert.Equal([firstReference.Current.GetQuestion(), thirdReference.Current.GetQuestion()], SortedNamesOf(secondReference.Current.RelatedNotes.PerfectSynonyms.Words));
      Assert.Equal([firstReference.Current.GetQuestion(), secondReference.Current.GetQuestion()], SortedNamesOf(thirdReference.Current.RelatedNotes.PerfectSynonyms.Words));

      firstReference.Current.RelatedNotes.PerfectSynonyms.Remove(thirdReference.Current.Question.Reference);
      Assert.Equal([secondReference.Current.GetQuestion()], SortedNamesOf(firstReference.Current.RelatedNotes.PerfectSynonyms.Words));
      Assert.Equal([firstReference.Current.GetQuestion()], SortedNamesOf(secondReference.Current.RelatedNotes.PerfectSynonyms.Words));
      Assert.Empty(SortedNamesOf(thirdReference.Current.RelatedNotes.PerfectSynonyms.Words));

      firstReference.Current.Edit(it => it.SetCorpusAnswer("first_latest"));
      Assert.Equal("first_latest", firstReference.Current.GetAnswer());
      Assert.Equal("first_latest", secondReference.Current.GetAnswer());
      Assert.Equal("third_new", thirdReference.Current.GetAnswer());
   }

   [Fact]
   public void PerfectSynonymsAreKeptInSyncOnAdd()
   {
      var firstReference = GetService<VocabNoteFactory>().Create("first", "", []).Reference();
      var secondReference = GetService<VocabNoteFactory>().Create("second", "", []).Reference();
      firstReference.Current.Edit(it => it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(secondReference.Current.Question.Reference));

      Assert.Equal(["second"], SortedNamesOf(firstReference.Current.RelatedNotes.PerfectSynonyms.Words));
      Assert.Equal(["first"], SortedNamesOf(secondReference.Current.RelatedNotes.PerfectSynonyms.Words));

      var thirdReference = GetService<VocabNoteFactory>().Create("third", "", []).Reference();
      var fourthReference = GetService<VocabNoteFactory>().Create("fourth", "", []).Reference();
      secondReference.Current.Edit(it =>
      {
         it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(thirdReference.Current.Question.Reference);
         it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(fourthReference.Current.Question.Reference);
      });

      Assert.Equal(["fourth", "second", "third"], SortedNamesOf(firstReference.Current.RelatedNotes.PerfectSynonyms.Words));
      Assert.Equal(["first", "fourth", "third"], SortedNamesOf(secondReference.Current.RelatedNotes.PerfectSynonyms.Words));
      Assert.Equal(["first", "fourth", "second"], SortedNamesOf(thirdReference.Current.RelatedNotes.PerfectSynonyms.Words));
      Assert.Equal(["first", "second", "third"], SortedNamesOf(fourthReference.Current.RelatedNotes.PerfectSynonyms.Words));
   }
}
