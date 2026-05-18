using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.When_updating_cached_note_data;

public class for_a_vocab : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class after_changing_its_question : for_a_vocab
   {
      readonly VocabNote _vocab;

      public after_changing_its_question()
      {
         _vocab = CreateVocab("食べる", "to eat", "たべる");
         _vocab.GetQuestion();
         _vocab.Question.Set("飲む");
         _vocab.UpdateGeneratedData();
      }

      [XF] public void the_old_question_no_longer_finds_it() =>
         GetService<VocabCollection>().WithQuestion("食べる").Must().BeEmpty();

      [XF] public void the_new_question_finds_it() =>
         GetService<VocabCollection>().WithQuestion("飲む").Count.Must().Be(1);

      [XF] public void the_new_question_returns_the_same_note() =>
         GetService<VocabCollection>().WithQuestion("飲む")[0].Must().Be(_vocab);
   }

   public class after_adding_a_form : for_a_vocab
   {
      public after_adding_a_form()
      {
         var vocab = CreateVocab("食べる", "to eat", "たべる");
         vocab.Forms.Add("taberu-form");
      }

      [XF] public void WithQuestion_does_not_find_the_form() =>
         GetService<VocabCollection>().WithQuestion("taberu-form").Must().BeEmpty();

      [XF] public void the_original_question_still_finds_it() =>
         GetService<VocabCollection>().WithQuestion("食べる").Count.Must().Be(1);
   }

   public class after_adding_and_then_removing_a_form : for_a_vocab
   {
      readonly VocabNote _vocab;

      public after_adding_and_then_removing_a_form()
      {
         _vocab = CreateVocab("食べる", "to eat", "たべる");
         _vocab.Forms.Add("食う");
         _vocab.Forms.Remove("食う");
      }

      [XF] public void the_removed_form_is_no_longer_present() =>
         _vocab.Forms.AllSet().Contains("食う").Must().BeFalse();

      [XF] public void the_original_question_still_finds_it() =>
         GetService<VocabCollection>().WithQuestion("食べる").Count.Must().Be(1);
   }

   public class after_multiple_updates : for_a_vocab
   {
      readonly VocabNote _vocab;

      public after_multiple_updates()
      {
         _vocab = CreateVocab("走る", "to run", "はしる");
         _vocab.Forms.Add("駆ける");
         _vocab.Forms.Add("ダッシュする");
         _vocab.Question.Set("疾走する");
         _vocab.UpdateGeneratedData();
         _vocab.Forms.Remove("駆ける");
      }

      [XF] public void the_new_question_finds_it() =>
         GetService<VocabCollection>().WithQuestion("疾走する").Count.Must().Be(1);

      [XF] public void the_old_question_does_not_find_it() =>
         GetService<VocabCollection>().WithQuestion("走る").Must().BeEmpty();

      [XF] public void removed_forms_are_gone() =>
         _vocab.Forms.AllSet().Contains("駆ける").Must().BeFalse();

      [XF] public void surviving_forms_remain() =>
         _vocab.Forms.AllSet().Must().Contain("ダッシュする");
   }
}
