using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using static JAStudio.Core.Specifications.WordReferenceNames;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>The vocab 行く, with the form 行きます, and the vocab 向かう. No vocab is named 行きます, so the word 行きます refers to 行く, which has it as a form.</summary>
public class When_changing_a_vocabs_synonyms : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly VocabNote ToGo;
   protected readonly VocabNote ToHeadFor;

   public When_changing_a_vocabs_synonyms()
   {
      ToGo = VocabNote.Create(NoteServices, "行く", "to go", "いく");
      ToGo.Forms.Add("行きます");
      ToHeadFor = VocabNote.Create(NoteServices, "向かう", "to head for", "むかう");
   }

   protected static string[] SynonymsOf(VocabNote vocab) => SortedNamesOf(vocab.RelatedNotes.Synonyms.Words);

   public class adding_向かう_to_行く : When_changing_a_vocabs_synonyms
   {
      public adding_向かう_to_行く() => ToGo.RelatedNotes.Synonyms.Add(new WordReference("向かう"));

      [XF] public void 行く_has_向かう_as_a_synonym() => SynonymsOf(ToGo).Must().SequenceEqual(["向かう"]);
      [XF] public void 向かう_has_行く_as_a_synonym() => SynonymsOf(ToHeadFor).Must().SequenceEqual(["行く"]);
   }

   public class adding_one_of_its_own_forms_行きます_to_行く : When_changing_a_vocabs_synonyms
   {
      public adding_one_of_its_own_forms_行きます_to_行く() => ToGo.RelatedNotes.Synonyms.Add(new WordReference("行きます"));

      [XF] public void 行く_has_行きます_as_a_synonym_but_not_itself() => SynonymsOf(ToGo).Must().SequenceEqual(["行きます"]);
   }
}
