using System;
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

/// <summary>The vocab 行く, with the form 行きます, and the vocab 向かう. No vocab is named 行きます, so the word 行きます refers to 行く, which has it as a form.</summary>
public class When_changing_a_vocabs_synonyms : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly ITaggregateReference<VocabNote> ToGoReference;
   protected VocabNote ToGo => ToGoReference.Current;
   protected readonly ITaggregateReference<VocabNote> ToHeadForReference;
   protected VocabNote ToHeadFor => ToHeadForReference.Current;

   public When_changing_a_vocabs_synonyms()
   {
      ToGoReference = VocabNote.Create(NoteServices, "行く", "to go", "いく").Reference();
      ToGo.Edit(it => it.Forms.Add("行きます"));
      ToHeadForReference = VocabNote.Create(NoteServices, "向かう", "to head for", "むかう").Reference();
   }

   protected static string[] SynonymsOf(VocabNote vocab) => SortedNamesOf(vocab.RelatedNotes.Synonyms.Words);

   public class adding_向かう_to_行く : When_changing_a_vocabs_synonyms
   {
      public adding_向かう_to_行く() => ToGo.Edit(it => it.RelatedNotes.Synonyms.Add(new WordReference("向かう")));

      [XF] public void 行く_has_向かう_as_a_synonym() => SynonymsOf(ToGo).Must().SequenceEqual(["向かう"]);
      [XF] public void 向かう_has_行く_as_a_synonym() => SynonymsOf(ToHeadFor).Must().SequenceEqual(["行く"]);

      public class and_then_removing_it : adding_向かう_to_行く
      {
         public and_then_removing_it() => ToGo.Edit(it => it.RelatedNotes.Synonyms.Remove(new WordReference("向かう")));

         [XF] public void neither_has_a_synonym() => new { 行く = SynonymsOf(ToGo), 向かう = SynonymsOf(ToHeadFor) }.Must().DeepEqual(new { 行く = Array.Empty<string>(), 向かう = Array.Empty<string>() });
      }
   }

   public class adding_向かう_to_行く_in_a_unit_of_work_that_then_fails : When_changing_a_vocabs_synonyms
   {
      public adding_向かう_to_行く_in_a_unit_of_work_that_then_fails() =>
         Invoking(() => GetService<JPCollection>().InAUnitOfWork(unitOfWork =>
                 {
                    unitOfWork.ForUpdate(ToGo).RelatedNotes.Synonyms.Add(new WordReference("向かう"));
                    throw new InvalidOperationException("The unit of work fails.");
                 }))
           .Must().Throw<InvalidOperationException>();

      [XF] public void neither_has_a_synonym() => new { 行く = SynonymsOf(ToGo), 向かう = SynonymsOf(ToHeadFor) }.Must().DeepEqual(new { 行く = Array.Empty<string>(), 向かう = Array.Empty<string>() });
   }

   /// <summary>The word 行きます refers to 行く, which has it as a form, so 行く gets 向かう as a synonym, by its name.</summary>
   public class adding_行きます_to_向かう : When_changing_a_vocabs_synonyms
   {
      public adding_行きます_to_向かう() => ToHeadFor.Edit(it => it.RelatedNotes.Synonyms.Add(new WordReference("行きます")));

      [XF] public void 行く_has_向かう_as_a_synonym() => SynonymsOf(ToGo).Must().SequenceEqual(["向かう"]);
      [XF] public void 向かう_has_only_行きます_as_a_synonym_not_the_name_of_行く_too() => SynonymsOf(ToHeadFor).Must().SequenceEqual(["行きます"]);
   }

   public class adding_one_of_its_own_forms_行きます_to_行く : When_changing_a_vocabs_synonyms
   {
      public adding_one_of_its_own_forms_行きます_to_行く() => ToGo.Edit(it => it.RelatedNotes.Synonyms.Add(new WordReference("行きます")));

      [XF] public void 行く_has_行きます_as_a_synonym_but_not_itself() => SynonymsOf(ToGo).Must().SequenceEqual(["行きます"]);
   }
}
