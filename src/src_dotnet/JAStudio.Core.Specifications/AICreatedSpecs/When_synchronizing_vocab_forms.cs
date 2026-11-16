using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

public class When_synchronizing_vocab_forms : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class after_adding_a_form_that_matches_another_vocabs_question : When_synchronizing_vocab_forms
   {
      readonly ITaggregateReference<VocabNote> _vocabAReference;
      VocabNote _vocabA => _vocabAReference.Current;
      readonly ITaggregateReference<VocabNote> _vocabBReference;
      VocabNote _vocabB => _vocabBReference.Current;

      public after_adding_a_form_that_matches_another_vocabs_question()
      {
         _vocabAReference = CreateVocab("食べる", "to eat", "たべる").Reference();
         _vocabBReference = CreateVocab("食う", "to eat (casual)", "くう").Reference();
         _vocabA.Edit(it => it.Forms.Add("食う"));
      }

      [XF] public void the_other_vocab_gains_this_vocabs_question_as_a_form() =>
         _vocabB.Forms.EnteredFormsSet().Must().Contain("食べる");

      public class after_removing_it : after_adding_a_form_that_matches_another_vocabs_question
      {
         public after_removing_it() => _vocabA.Edit(it => it.Forms.Remove("食う"));

         [XF] public void the_other_vocab_no_longer_has_this_vocabs_question_as_a_form() =>
            _vocabB.Forms.EnteredFormsSet().Must().Satisfy(it => !it.Contains("食べる"));
      }
   }

   public class given_a_vocab_with_an_owned_and_a_not_owned_entered_form : When_synchronizing_vocab_forms
   {
      readonly ITaggregateReference<VocabNote> _vocabReference;
      VocabNote _vocab => _vocabReference.Current;

      public given_a_vocab_with_an_owned_and_a_not_owned_entered_form()
      {
         _vocabReference = CreateVocab("走る", "to run", "はしる").Reference();
         _vocab.Edit(it => it.Forms.SetEnteredForms([EnteredForm.Owned("駆ける"), EnteredForm.NotOwned("ダッシュする")]));
      }

      [XF] public void the_owned_entered_form_is_an_owned_form() => _vocab.Forms.IsOwnedForm("駆ける").Must().BeTrue();
      [XF] public void the_question_is_always_owned() => _vocab.Forms.IsOwnedForm("走る").Must().BeTrue();
      [XF] public void the_not_owned_entered_form_is_not_an_owned_form() => _vocab.Forms.IsOwnedForm("ダッシュする").Must().BeFalse();
   }

   public class given_a_newly_created_vocab : When_synchronizing_vocab_forms
   {
      readonly ITaggregateReference<VocabNote> _vocabReference;
      VocabNote _vocab => _vocabReference.Current;

      public given_a_newly_created_vocab() => _vocabReference = CreateVocab("本", "book", "ほん").Reference();

      [XF] public void the_question_is_included_in_owned_forms() => _vocab.Forms.OwnedForms().Must().Contain("本");
   }

   public class given_a_vocab_with_forms_pointing_to_other_vocabs : When_synchronizing_vocab_forms
   {
      readonly ITaggregateReference<VocabNote> _vocab1Reference;
      VocabNote _vocab1 => _vocab1Reference.Current;
      readonly ITaggregateReference<VocabNote> _vocab2Reference;
      VocabNote _vocab2 => _vocab2Reference.Current;
      readonly ITaggregateReference<VocabNote> _vocab3Reference;
      VocabNote _vocab3 => _vocab3Reference.Current;

      public given_a_vocab_with_forms_pointing_to_other_vocabs()
      {
         _vocab1Reference = CreateVocab("食べる", "to eat", "たべる").Reference();
         _vocab2Reference = CreateVocab("食う", "to eat (casual)", "くう").Reference();
         _vocab3Reference = CreateVocab("召し上がる", "to eat (honorific)", "めしあがる").Reference();
         _vocab1.Edit(it => it.Forms.SetEnteredForms([EnteredForm.NotOwned("食う"), EnteredForm.NotOwned("召し上がる")]));
      }

      [XF] public void VocabForQuestionAndEnteredForms_includes_the_first_form_vocab() =>
         _vocab1.Forms.VocabForQuestionAndEnteredForms().Must().Contain(_vocab2);

      [XF] public void VocabForQuestionAndEnteredForms_includes_the_second_form_vocab() =>
         _vocab1.Forms.VocabForQuestionAndEnteredForms().Must().Contain(_vocab3);

      [XF] public void VocabForQuestionAndEnteredForms_includes_the_vocab_itself() =>
         _vocab1.Forms.VocabForQuestionAndEnteredForms().Must().Contain(_vocab1);
   }
}
