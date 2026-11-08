using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

public class When_working_with_a_VocabNote : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_a_newly_created_vocab : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public given_a_newly_created_vocab() => _vocab = CreateVocab("食べる", "to eat", "たべる");

      [XF] public void the_question_is_set() => _vocab.GetQuestion().Must().Be("食べる");
      [XF] public void the_answer_is_set() => _vocab.GetAnswer().Must().Be("to eat");
      [XF] public void it_has_an_id() => _vocab.GetId().Must().NotBeNull();
   }

   public class given_a_vocab_with_two_readings : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public given_a_vocab_with_two_readings() => _vocab = CreateVocab("食べる", "to eat", "たべる", "くう");

      [XF] public void it_has_two_readings() => _vocab.GetReadings().Count.Must().Be(2);
      [XF] public void it_includes_the_first_reading() => _vocab.GetReadings().Must().Contain("たべる");
      [XF] public void it_includes_the_second_reading() => _vocab.GetReadings().Must().Contain("くう");
   }

   public class after_updating_a_vocabs_readings : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public after_updating_a_vocabs_readings()
      {
         _vocab = CreateVocab("本", "book", "ほん");
         _vocab.SetReadings(["ほん", "もと"]);
      }

      [XF] public void it_has_two_readings() => _vocab.GetReadings().Count.Must().Be(2);
      [XF] public void it_includes_the_first_reading() => _vocab.GetReadings().Must().Contain("ほん");
      [XF] public void it_includes_the_second_reading() => _vocab.GetReadings().Must().Contain("もと");
   }

   public class given_a_question_with_a_disambiguation : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public given_a_question_with_a_disambiguation()
      {
         _vocab = new VocabNote(NoteServices);
         _vocab.Question.SetText("取る");
         _vocab.Question.SetDisambiguation("to take");
      }

      [XF] public void it_is_disambiguated() => _vocab.Question.IsDisambiguated.Must().BeTrue();
      [XF] public void the_text_is_the_question_alone() => _vocab.Question.Text.Must().Be("取る");
      [XF] public void the_vocab_question_is_the_question_alone() => _vocab.GetQuestion().Must().Be("取る");
      [XF] public void the_disambiguation_name_is_the_question_and_the_disambiguation_joined_by_the_disambiguation_marker() =>
         _vocab.Question.DisambiguationName.Must().Be("取る:to take");

      public class after_changing_the_question : given_a_question_with_a_disambiguation
      {
         public after_changing_the_question() => _vocab.Question.SetText("採る");

         [XF] public void the_disambiguation_is_kept() => _vocab.Question.DisambiguationName.Must().Be("採る:to take");
      }

      public class after_removing_the_disambiguation : given_a_question_with_a_disambiguation
      {
         public after_removing_the_disambiguation() => _vocab.Question.SetDisambiguation("");

         [XF] public void it_is_not_disambiguated() => _vocab.Question.IsDisambiguated.Must().BeFalse();
         [XF] public void the_disambiguation_name_is_the_question_alone() => _vocab.Question.DisambiguationName.Must().Be("取る");
      }

      [XF] public void setting_a_question_containing_the_disambiguation_marker_is_refused() =>
         Invoking(() => _vocab.Question.SetText("取る:to grab")).Must().Throw<ArgumentAssertionFailedException>();

      [XF] public void setting_a_disambiguation_containing_the_disambiguation_marker_is_refused() =>
         Invoking(() => _vocab.Question.SetDisambiguation("to:grab")).Must().Throw<ArgumentAssertionFailedException>();

      [XF] public void setting_an_empty_question_is_refused() => Invoking(() => _vocab.Question.SetText("")).Must().Throw<ArgumentAssertionFailedException>();
   }

   public class after_adding_a_form : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public after_adding_a_form()
      {
         _vocab = CreateVocab("走る", "to run", "はしる");
         _vocab.Forms.Add("駆ける");
      }

      [XF] public void the_form_appears_in_the_entered_forms() => _vocab.Forms.EnteredFormsList().Must().Contain("駆ける");

      public class after_removing_it : after_adding_a_form
      {
         public after_removing_it() => _vocab.Forms.Remove("駆ける");

         [XF] public void the_form_no_longer_appears_in_the_entered_forms() =>
            _vocab.Forms.EnteredFormsList().Contains("駆ける").Must().BeFalse();
      }
   }

   public class given_a_vocab_with_no_extra_forms : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public given_a_vocab_with_no_extra_forms() => _vocab = CreateVocab("走る", "to run", "はしる");

      [XF] public void OwnedForms_includes_the_question() => _vocab.Forms.OwnedForms().Must().Contain("走る");
   }

   public class given_a_vocab_with_an_owned_and_a_not_owned_entered_form : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public given_a_vocab_with_an_owned_and_a_not_owned_entered_form()
      {
         _vocab = new VocabNote(NoteServices);
         _vocab.Question.SetText("走る");
         _vocab.Forms.SetEnteredForms([EnteredForm.Owned("駆ける"), EnteredForm.NotOwned("はしる")]);
      }

      [XF] public void the_owned_entered_form_is_an_owned_form() => _vocab.Forms.IsOwnedForm("駆ける").Must().BeTrue();
      [XF] public void the_question_is_owned() => _vocab.Forms.IsOwnedForm("走る").Must().BeTrue();
      [XF] public void the_not_owned_entered_form_is_not_an_owned_form() => _vocab.Forms.IsOwnedForm("はしる").Must().BeFalse();

      public class after_changing_the_question_to_a_new_form : given_a_vocab_with_an_owned_and_a_not_owned_entered_form
      {
         public after_changing_the_question_to_a_new_form() => _vocab.Question.SetText("奔る");

         [XF] public void the_owned_entered_form_stays_owned() => _vocab.Forms.EnteredForms().Must().Contain(EnteredForm.Owned("駆ける"));
         [XF] public void the_new_question_is_matched() => _vocab.Forms.MatchedFormsSet().Must().Contain("奔る");
         [XF] public void the_new_question_is_not_among_the_entered_forms() => _vocab.Forms.EnteredFormsSet().Contains("奔る").Must().BeFalse();
         [XF] public void the_old_question_is_no_longer_matched() => _vocab.Forms.MatchedFormsSet().Contains("走る").Must().BeFalse();
         [XF] public void the_new_question_is_owned() => _vocab.Forms.IsOwnedForm("奔る").Must().BeTrue();
      }

      public class after_changing_the_question_to_its_owned_entered_form : given_a_vocab_with_an_owned_and_a_not_owned_entered_form
      {
         public after_changing_the_question_to_its_owned_entered_form() => _vocab.Question.SetText("駆ける");

         [XF] public void the_new_question_is_dropped_from_the_entered_forms() => _vocab.Forms.EnteredFormsList().Must().SequenceEqual(["はしる"]);
         [XF] public void the_new_question_is_matched() => _vocab.Forms.MatchedFormsSet().Must().Contain("駆ける");
      }
   }

   public class given_a_vocab_with_user_fields_set : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public given_a_vocab_with_user_fields_set()
      {
         _vocab = CreateVocab("食べる", "to eat", "たべる");
         _vocab.User.Answer.Set("to consume");
         _vocab.User.Mnemonic.Set("Remember 'taberu' sounds like 'table' where you eat");
         _vocab.User.Explanation.Set("Common verb");
      }

      [XF] public void the_user_answer_is_stored() => _vocab.User.Answer.Value.Must().Be("to consume");
      [XF] public void the_user_mnemonic_is_stored() => _vocab.User.Mnemonic.Value.Must().Be("Remember 'taberu' sounds like 'table' where you eat");
      [XF] public void the_user_explanation_is_stored() => _vocab.User.Explanation.Value.Must().Be("Common verb");
   }

   public class given_a_vocab_with_a_source_answer : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public given_a_vocab_with_a_source_answer() => _vocab = CreateVocab("本", "book", "ほん");

      [XF] public void GetAnswer_returns_the_source_answer() => _vocab.GetAnswer().Must().Be("book");

      public class after_setting_a_user_answer : given_a_vocab_with_a_source_answer
      {
         public after_setting_a_user_answer() => _vocab.User.Answer.Set("written work");

         [XF] public void GetAnswer_returns_the_user_answer() => _vocab.GetAnswer().Must().Be("written work");
      }
   }

   public class after_setting_a_question_on_a_new_note : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public after_setting_a_question_on_a_new_note()
      {
         _vocab = new VocabNote(NoteServices);
         _vocab.Question.SetText("新しい");
      }

      [XF] public void the_question_is_matched() =>
         _vocab.Forms.MatchedFormsSet().Must().Contain("新しい");

      [XF] public void the_question_is_not_among_the_entered_forms() =>
         _vocab.Forms.EnteredFormsList().Must().BeEmpty();
   }

   public class given_a_vocab_created_with_explicit_forms : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public given_a_vocab_created_with_explicit_forms() =>
         _vocab = CreateVocab("食べる", "to eat", ["たべる"], ["食う", "召し上がる"]);

      [XF] public void the_first_form_is_present() => _vocab.Forms.EnteredFormsSet().Must().Contain("食う");
      [XF] public void the_second_form_is_present() => _vocab.Forms.EnteredFormsSet().Must().Contain("召し上がる");
   }
}
