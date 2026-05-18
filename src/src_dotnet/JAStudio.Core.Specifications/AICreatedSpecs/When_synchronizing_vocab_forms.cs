using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

public class When_synchronizing_vocab_forms : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class after_adding_a_form_that_matches_another_vocabs_question : When_synchronizing_vocab_forms
   {
      readonly VocabNote _vocabA;
      readonly VocabNote _vocabB;

      public after_adding_a_form_that_matches_another_vocabs_question()
      {
         _vocabA = CreateVocab("食べる", "to eat", "たべる");
         _vocabB = CreateVocab("食う", "to eat (casual)", "くう");
         _vocabA.Forms.Add("食う");
      }

      [XF] public void the_other_vocab_gains_this_vocabs_question_as_a_form() =>
         _vocabB.Forms.AllSet().Must().Contain("食べる");

      public class after_removing_it : after_adding_a_form_that_matches_another_vocabs_question
      {
         public after_removing_it() => _vocabA.Forms.Remove("食う");

         [XF] public void the_other_vocab_no_longer_has_this_vocabs_question_as_a_form() =>
            _vocabB.Forms.AllSet().Contains("食べる").Must().BeFalse();
      }
   }

   public class given_a_vocab_with_bracketed_and_plain_forms : When_synchronizing_vocab_forms
   {
      readonly VocabNote _vocab;

      public given_a_vocab_with_bracketed_and_plain_forms()
      {
         _vocab = CreateVocab("走る", "to run", "はしる");
         _vocab.Forms.SetList(["[駆ける]", "ダッシュする"]);
      }

      [XF] public void bracketed_forms_are_owned() => _vocab.Forms.IsOwnedForm("駆ける").Must().BeTrue();
      [XF] public void the_question_is_always_owned() => _vocab.Forms.IsOwnedForm("走る").Must().BeTrue();
      [XF] public void non_bracketed_forms_are_not_owned() => _vocab.Forms.IsOwnedForm("ダッシュする").Must().BeFalse();
   }

   public class given_a_newly_created_vocab : When_synchronizing_vocab_forms
   {
      readonly VocabNote _vocab;

      public given_a_newly_created_vocab() => _vocab = CreateVocab("本", "book", "ほん");

      [XF] public void the_question_is_included_in_owned_forms() => _vocab.Forms.OwnedForms().Must().Contain("本");
   }

   public class given_a_vocab_with_forms_pointing_to_other_vocabs : When_synchronizing_vocab_forms
   {
      readonly VocabNote _vocab1;
      readonly VocabNote _vocab2;
      readonly VocabNote _vocab3;

      public given_a_vocab_with_forms_pointing_to_other_vocabs()
      {
         _vocab1 = CreateVocab("食べる", "to eat", "たべる");
         _vocab2 = CreateVocab("食う", "to eat (casual)", "くう");
         _vocab3 = CreateVocab("召し上がる", "to eat (honorific)", "めしあがる");
         _vocab1.Forms.SetList(["食う", "召し上がる"]);
      }

      [XF] public void AllListNotes_includes_the_first_form_vocab() =>
         _vocab1.Forms.AllListNotes().Must().Contain(_vocab2);

      [XF] public void AllListNotes_includes_the_second_form_vocab() =>
         _vocab1.Forms.AllListNotes().Must().Contain(_vocab3);
   }
}
