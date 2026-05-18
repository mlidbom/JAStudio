using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;

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

   public class given_a_disambiguated_question : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public given_a_disambiguated_question()
      {
         _vocab = new VocabNote(NoteServices);
         _vocab.Question.Set("取る:to take");
      }

      [XF] public void IsDisambiguated_is_true() => _vocab.Question.IsDisambiguated.Must().BeTrue();
      [XF] public void Raw_returns_the_base_question() => _vocab.Question.Raw.Must().Be("取る");
      [XF] public void DisambiguationName_returns_the_full_form() => _vocab.Question.DisambiguationName.Must().Be("取る:to take");
   }

   public class given_an_invalid_question_format : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public given_an_invalid_question_format()
      {
         _vocab = new VocabNote(NoteServices);
         _vocab.Question.Set("a:b:c");
      }

      [XF] public void IsValid_is_false() => _vocab.Question.IsValid.Must().BeFalse();
      [XF] public void Raw_returns_the_invalid_question_message() => _vocab.Question.Raw.Must().Be(VocabNoteQuestion.InvalidQuestionMessage);
   }

   public class after_adding_a_form : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public after_adding_a_form()
      {
         _vocab = CreateVocab("走る", "to run", "はしる");
         _vocab.Forms.Add("駆ける");
      }

      [XF] public void the_form_appears_in_AllList() => _vocab.Forms.AllList().Must().Contain("駆ける");

      public class after_removing_it : after_adding_a_form
      {
         public after_removing_it() => _vocab.Forms.Remove("駆ける");

         [XF] public void the_form_no_longer_appears_in_AllList() =>
            _vocab.Forms.AllList().Contains("駆ける").Must().BeFalse();
      }
   }

   public class given_a_vocab_with_no_extra_forms : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public given_a_vocab_with_no_extra_forms() => _vocab = CreateVocab("走る", "to run", "はしる");

      [XF] public void OwnedForms_includes_the_question() => _vocab.Forms.OwnedForms().Must().Contain("走る");
   }

   public class given_a_vocab_with_forms_set_using_brackets : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public given_a_vocab_with_forms_set_using_brackets()
      {
         _vocab = new VocabNote(NoteServices);
         _vocab.Question.Set("走る");
         _vocab.Forms.SetList(["[駆ける]", "はしる"]);
      }

      [XF] public void a_bracketed_form_is_owned() => _vocab.Forms.IsOwnedForm("駆ける").Must().BeTrue();
      [XF] public void the_question_is_owned() => _vocab.Forms.IsOwnedForm("走る").Must().BeTrue();
      [XF] public void a_plain_form_is_not_owned() => _vocab.Forms.IsOwnedForm("はしる").Must().BeFalse();
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
         _vocab.Question.Set("新しい");
      }

      [XF] public void the_question_is_automatically_included_in_forms() =>
         _vocab.Forms.AllSet().Must().Contain("新しい");
   }

   public class given_a_vocab_created_with_explicit_forms : When_working_with_a_VocabNote
   {
      readonly VocabNote _vocab;

      public given_a_vocab_created_with_explicit_forms() =>
         _vocab = CreateVocab("食べる", "to eat", ["たべる"], ["食う", "召し上がる"]);

      [XF] public void the_first_form_is_present() => _vocab.Forms.AllSet().Must().Contain("食う");
      [XF] public void the_second_form_is_present() => _vocab.Forms.AllSet().Must().Contain("召し上がる");
   }
}
