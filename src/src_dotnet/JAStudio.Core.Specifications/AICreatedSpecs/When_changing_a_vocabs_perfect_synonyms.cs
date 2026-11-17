using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts.Exceptions;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using static Compze.Must.MustActions;
using static JAStudio.Core.Specifications.WordReferenceNames;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>The vocab てよろしい, ていい and んでいい, which all mean "may", made one web of perfect synonyms by adding the other two to てよろしい.</summary>
public class When_changing_a_vocabs_perfect_synonyms : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly ITaggregateReference<VocabNote> TeyoroshiiReference;
   protected VocabNote Teyoroshii => TeyoroshiiReference.Current;
   protected readonly ITaggregateReference<VocabNote> TeiiReference;
   protected VocabNote Teii => TeiiReference.Current;
   protected readonly ITaggregateReference<VocabNote> NdeiiReference;
   protected VocabNote Ndeii => NdeiiReference.Current;

   public When_changing_a_vocabs_perfect_synonyms()
   {
      TeyoroshiiReference = VocabNote.Create(NoteServices, "てよろしい", "may", "てよろしい").Reference();
      TeiiReference = VocabNote.Create(NoteServices, "ていい", "may", "ていい").Reference();
      NdeiiReference = VocabNote.Create(NoteServices, "んでいい", "may", "んでいい").Reference();
      Teyoroshii.Edit(it =>
      {
         it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(new WordReference("ていい"));
         it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(new WordReference("んでいい"));
      });
   }

   protected static string[] PerfectSynonymsOf(VocabNote vocab) => SortedNamesOf(vocab.RelatedNotes.PerfectSynonyms.Words);
   protected static string[] SynonymsOf(VocabNote vocab) => SortedNamesOf(vocab.RelatedNotes.Synonyms.Words);

   public class adding_a_word_no_vocab_has : When_changing_a_vocabs_perfect_synonyms
   {
      readonly ArgumentAssertionFailedException _refusal;

      public adding_a_word_no_vocab_has() =>
         _refusal = Invoking(() => Teyoroshii.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(new WordReference("てもいい")))
                   .Must().Throw<ArgumentAssertionFailedException>().Which;

      [XF] public void the_refusal_names_the_word() => _refusal.Message.Must().Contain("てもいい");
      [XF] public void the_web_is_unchanged() => PerfectSynonymsOf(Teyoroshii).Must().SequenceEqual(["ていい", "んでいい"]);
   }

   /// <summary>Emptying てよろしい's answer empties the whole web's, so てよろしい then shows its source's answer, which this sets.</summary>
   public class emptying_the_answer_of_てよろしい_and_then_giving_it_a_source_answer : When_changing_a_vocabs_perfect_synonyms
   {
      public emptying_the_answer_of_てよろしい_and_then_giving_it_a_source_answer()
      {
         Teyoroshii.Edit(it =>
         {
            it.SetCorpusAnswer(string.Empty);
            it.SourceData.SetAnswer("may, be allowed to");
         });
      }

      [XF] public void ていい_and_んでいい_show_the_source_answer_of_てよろしい() =>
         new[] { Teii.GetAnswer(), Ndeii.GetAnswer() }.Must().SequenceEqual(["may, be allowed to", "may, be allowed to"]);
   }

   public class giving_てよろしい_another_answer : When_changing_a_vocabs_perfect_synonyms
   {
      readonly List<string> _notesNotifiedAsUpdated = [];

      public giving_てよろしい_another_answer()
      {
         using var _ = GetService<JPCollection>().NoteUpdated.Subscribe(it => _notesNotifiedAsUpdated.Add(it.GetQuestion()));
         Teyoroshii.Edit(it => it.SetCorpusAnswer("may, be allowed to"));
      }

      [XF] public void ていい_and_んでいい_get_the_answer() => new[] { Teii.GetAnswer(), Ndeii.GetAnswer() }.Must().SequenceEqual(["may, be allowed to", "may, be allowed to"]);
      [XF] public void all_three_are_notified_as_updated_by_the_unit_of_work_that_gave_it() => _notesNotifiedAsUpdated.Order().Must().SequenceEqual(["ていい", "てよろしい", "んでいい"]);
   }

   public class giving_てよろしい_another_answer_in_a_unit_of_work_that_then_fails : When_changing_a_vocabs_perfect_synonyms
   {
      public giving_てよろしい_another_answer_in_a_unit_of_work_that_then_fails() =>
         Invoking(() => GetService<JPCollection>().InAUnitOfWork(unitOfWork =>
                 {
                    unitOfWork.ForUpdate(Teyoroshii).SetCorpusAnswer("may, be allowed to");
                    throw new InvalidOperationException("The unit of work fails.");
                 }))
           .Must().Throw<InvalidOperationException>();

      [XF] public void all_three_keep_their_answer() => new[] { Teyoroshii.GetAnswer(), Teii.GetAnswer(), Ndeii.GetAnswer() }.Must().SequenceEqual(["may", "may", "may"]);
   }

   public class removing_ていい_from_てよろしい_in_a_unit_of_work_that_then_fails : When_changing_a_vocabs_perfect_synonyms
   {
      public removing_ていい_from_てよろしい_in_a_unit_of_work_that_then_fails() =>
         Invoking(() => GetService<JPCollection>().InAUnitOfWork(unitOfWork =>
                 {
                    unitOfWork.ForUpdate(Teyoroshii).RelatedNotes.PerfectSynonyms.Remove(new WordReference("ていい"));
                    throw new InvalidOperationException("The unit of work fails.");
                 }))
           .Must().Throw<InvalidOperationException>();

      [XF] public void the_web_is_unchanged() =>
         new { てよろしい = PerfectSynonymsOf(Teyoroshii), ていい = PerfectSynonymsOf(Teii), んでいい = PerfectSynonymsOf(Ndeii) }
           .Must().DeepEqual(new { てよろしい = new[] { "ていい", "んでいい" }, ていい = new[] { "てよろしい", "んでいい" }, んでいい = new[] { "ていい", "てよろしい" } });
   }

   public class deleting_ていい : When_changing_a_vocabs_perfect_synonyms
   {
      public deleting_ていい() => GetService<JPCollection>().Delete(Teii, ItsStudyNote.IsDeleted);

      [XF] public void てよろしい_has_only_んでいい_as_a_perfect_synonym() => PerfectSynonymsOf(Teyoroshii).Must().SequenceEqual(["んでいい"]);
      [XF] public void んでいい_has_only_てよろしい_as_a_perfect_synonym() => PerfectSynonymsOf(Ndeii).Must().SequenceEqual(["てよろしい"]);
      [XF] public void both_keep_ていい_as_a_synonym() =>
         new { てよろしい = SynonymsOf(Teyoroshii), んでいい = SynonymsOf(Ndeii) }.Must().Satisfy(it => it.てよろしい.Contains("ていい") && it.んでいい.Contains("ていい"));
   }

   public class deleting_ていい_in_a_unit_of_work_that_then_fails : When_changing_a_vocabs_perfect_synonyms
   {
      public deleting_ていい_in_a_unit_of_work_that_then_fails() =>
         Invoking(() => GetService<JPCollection>().InAUnitOfWork(_ =>
                 {
                    GetService<JPCollection>().Delete(Teii, ItsStudyNote.IsDeleted);
                    throw new InvalidOperationException("The unit of work fails.");
                 }))
           .Must().Throw<InvalidOperationException>();

      [XF] public void the_web_is_unchanged() =>
         new { てよろしい = PerfectSynonymsOf(Teyoroshii), ていい = PerfectSynonymsOf(Teii), んでいい = PerfectSynonymsOf(Ndeii) }
           .Must().DeepEqual(new { てよろしい = new[] { "ていい", "んでいい" }, ていい = new[] { "てよろしい", "んでいい" }, んでいい = new[] { "ていい", "てよろしい" } });
   }
}
