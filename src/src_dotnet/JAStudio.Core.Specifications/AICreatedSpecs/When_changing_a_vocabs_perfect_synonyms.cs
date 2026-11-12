using System.Linq;
using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using static Compze.Must.MustActions;
using static JAStudio.Core.Specifications.WordReferenceNames;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>The vocab てよろしい, ていい and んでいい, which all mean "may", made one web of perfect synonyms by adding the other two to てよろしい.</summary>
public class When_changing_a_vocabs_perfect_synonyms : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly VocabNote Teyoroshii;
   protected readonly VocabNote Teii;
   protected readonly VocabNote Ndeii;

   public When_changing_a_vocabs_perfect_synonyms()
   {
      Teyoroshii = VocabNote.Create(NoteServices, "てよろしい", "may", "てよろしい");
      Teii = VocabNote.Create(NoteServices, "ていい", "may", "ていい");
      Ndeii = VocabNote.Create(NoteServices, "んでいい", "may", "んでいい");
      Teyoroshii.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(new WordReference("ていい"));
      Teyoroshii.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(new WordReference("んでいい"));
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

   public class deleting_ていい : When_changing_a_vocabs_perfect_synonyms
   {
      public deleting_ていい() => GetService<JPCollection>().Delete(Teii, ItsAnkiNote.IsDeleted);

      [XF] public void てよろしい_has_only_んでいい_as_a_perfect_synonym() => PerfectSynonymsOf(Teyoroshii).Must().SequenceEqual(["んでいい"]);
      [XF] public void んでいい_has_only_てよろしい_as_a_perfect_synonym() => PerfectSynonymsOf(Ndeii).Must().SequenceEqual(["てよろしい"]);
      [XF] public void both_keep_ていい_as_a_synonym() =>
         new { てよろしい = SynonymsOf(Teyoroshii), んでいい = SynonymsOf(Ndeii) }.Must().Satisfy(it => it.てよろしい.Contains("ていい") && it.んでいい.Contains("ていい"));
   }
}
