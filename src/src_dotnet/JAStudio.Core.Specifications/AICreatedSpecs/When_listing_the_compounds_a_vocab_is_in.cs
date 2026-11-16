using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using static JAStudio.Core.Specifications.WordReferenceNames;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

public class When_listing_the_compounds_a_vocab_is_in : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   VocabNote CreateCompound(string question, params string[] primaryCompoundParts)
   {
      var compound = CreateVocab(question, "a compound");
      compound.Edit(it => it.CompoundParts.SetPrimary(Words(primaryCompoundParts)));
      return compound;
   }

   static List<string> QuestionsOfTheCompoundsListedFor(VocabNote vocab) => vocab.RelatedNotes.InCompounds().Select(it => it.GetQuestion()).ToList();

   public class given_a_vocab_with_another_form : When_listing_the_compounds_a_vocab_is_in
   {
      readonly ITaggregateReference<VocabNote> _vocabReference;
      VocabNote _vocab => _vocabReference.Current;

      public given_a_vocab_with_another_form() => _vocabReference = CreateVocab("時間", "time", ["じかん"], ["時間", "じかん"]).Reference();

      public class with_a_compound_that_has_its_question_as_a_part : given_a_vocab_with_another_form
      {
         public with_a_compound_that_has_its_question_as_a_part() => CreateCompound("空き時間", "空き", "時間");

         [XF] public void the_compound_is_listed() => QuestionsOfTheCompoundsListedFor(_vocab).Must().SequenceEqual(["空き時間"]);
      }

      public class with_a_compound_that_has_its_other_form_as_a_part : given_a_vocab_with_another_form
      {
         public with_a_compound_that_has_its_other_form_as_a_part() => CreateCompound("時間割", "じかん", "割");

         [XF] public void the_compound_is_listed() => QuestionsOfTheCompoundsListedFor(_vocab).Must().SequenceEqual(["時間割"]);
      }

      public class with_a_compound_that_has_its_other_form_only_as_an_alternative_part : given_a_vocab_with_another_form
      {
         public with_a_compound_that_has_its_other_form_only_as_an_alternative_part() => CreateCompound("時間割", "割").Edit(it => it.CompoundParts.SetAlternative(Words("じかん")));

         [XF] public void the_compound_is_listed() => QuestionsOfTheCompoundsListedFor(_vocab).Must().SequenceEqual(["時間割"]);
      }

      public class with_a_compound_that_has_its_other_form_as_a_part_while_another_vocab_has_that_form_as_its_question : given_a_vocab_with_another_form
      {
         readonly ITaggregateReference<VocabNote> _otherVocabReference;
         VocabNote _otherVocab => _otherVocabReference.Current;

         public with_a_compound_that_has_its_other_form_as_a_part_while_another_vocab_has_that_form_as_its_question()
         {
            _otherVocabReference = CreateVocab("じかん", "time").Reference();
            CreateCompound("じかん割", "じかん", "割");
         }

         [XF] public void the_compound_is_not_listed() => QuestionsOfTheCompoundsListedFor(_vocab).Must().BeEmpty();
         [XF] public void the_compound_is_listed_for_the_other_vocab() => QuestionsOfTheCompoundsListedFor(_otherVocab).Must().SequenceEqual(["じかん割"]);
      }

      public class with_a_compound_of_a_compound_that_has_its_other_form_as_a_part : given_a_vocab_with_another_form
      {
         public with_a_compound_of_a_compound_that_has_its_other_form_as_a_part()
         {
            CreateCompound("時間割", "じかん", "割");
            CreateCompound("時間割表", "時間割", "表");
         }

         [XF] public void both_compounds_are_listed() => QuestionsOfTheCompoundsListedFor(_vocab).Must().SequenceEqual(["時間割", "時間割表"]);
      }

      public class with_a_compound_with_its_question_as_a_part_a_compound_starting_with_it_and_a_compound_with_neither : given_a_vocab_with_another_form
      {
         public with_a_compound_with_its_question_as_a_part_a_compound_starting_with_it_and_a_compound_with_neither()
         {
            CreateCompound("あきじかん", "あき", "じかん");
            CreateCompound("時間割", "じかん", "割");
            CreateCompound("空き時間", "空き", "時間");
         }

         [XF] public void the_compound_with_its_question_as_a_part_comes_first_then_the_compound_starting_with_it_then_the_other() =>
            QuestionsOfTheCompoundsListedFor(_vocab).Must().SequenceEqual(["空き時間", "時間割", "あきじかん"]);
      }
   }

   public class given_a_disambiguated_vocab : When_listing_the_compounds_a_vocab_is_in
   {
      readonly ITaggregateReference<VocabNote> _vocabReference;
      VocabNote _vocab => _vocabReference.Current;

      public given_a_disambiguated_vocab()
      {
         _vocabReference = CreateVocab("方", "way of doing").Reference();
         _vocab.Edit(it => it.Question.SetDisambiguation("かた"));
      }

      public class with_a_compound_that_has_its_question_without_the_disambiguation_as_a_part : given_a_disambiguated_vocab
      {
         public with_a_compound_that_has_its_question_without_the_disambiguation_as_a_part() => CreateCompound("書き方", "書く", "方");

         [XF] public void the_compound_is_listed() => QuestionsOfTheCompoundsListedFor(_vocab).Must().SequenceEqual(["書き方"]);
      }
   }
}
