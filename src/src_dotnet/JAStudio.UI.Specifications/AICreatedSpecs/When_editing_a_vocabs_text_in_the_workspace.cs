using System;
using System.Linq;
using System.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.ViewModels.Workspace.Editors;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

public class When_editing_a_vocabs_text_in_the_workspace : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   readonly VocabNote _vocab;
   readonly VocabEditorSectionViewModel _editor;

   protected When_editing_a_vocabs_text_in_the_workspace()
   {
      _vocab = CreateVocab("走る", "to run", "はしる");
      _editor = Edit<VocabEditorSectionViewModel>(_vocab);
   }

   VocabTextEditorViewModel Text => _editor.Text;
   TextFieldEditorViewModel CorpusAnswer => Text.CorpusFields.Single(it => it.Label == "Answer");

   /// <summary>A change to the vocab made elsewhere, as by Anki or a note menu, which makes the editor refresh.</summary>
   void ChangeTheMnemonicElsewhere() => _vocab.Mnemonic.Set("changed elsewhere");

   void WaitForTheReparseToFinish() => SpinWait.SpinUntil(() => !_editor.SentencesReparser.IsReparsingSentences, TimeSpan.FromMinutes(1)).Must().BeTrue();

   public class after_typing_an_answer : When_editing_a_vocabs_text_in_the_workspace
   {
      public after_typing_an_answer() => CorpusAnswer.Text = "to dash";

      [XF] public void the_answer_is_written_to_the_vocabs_corpus_answer() => _vocab.CorpusAnswer.Value.Must().Be("to dash");
      [XF] public void the_answer_shown_is_it() => _vocab.GetAnswer().Must().Be("to dash");
   }

   public class after_typing_an_answer_followed_by_a_space : When_editing_a_vocabs_text_in_the_workspace
   {
      public after_typing_an_answer_followed_by_a_space()
      {
         CorpusAnswer.Text = "to dash";
         CorpusAnswer.Text = "to dash ";
      }

      [XF] public void the_vocab_stores_the_answer_trimmed() => _vocab.CorpusAnswer.Value.Must().Be("to dash");

      public class and_a_change_made_elsewhere : after_typing_an_answer_followed_by_a_space
      {
         public and_a_change_made_elsewhere() => ChangeTheMnemonicElsewhere();

         [XF] public void the_answers_text_keeps_the_space() => CorpusAnswer.Text.Must().Be("to dash ");
      }
   }

   public class after_typing_another_reading_followed_by_a_comma : When_editing_a_vocabs_text_in_the_workspace
   {
      public after_typing_another_reading_followed_by_a_comma()
      {
         Text.Readings.Text = "はしる, かける";
         Text.Readings.Text = "はしる, かける, ";
      }

      [XF] public void the_vocab_has_both_readings() => string.Join(", ", _vocab.GetReadings()).Must().Be("はしる, かける");
      [XF] public void the_readings_text_keeps_the_comma() => Text.Readings.Text.Must().Be("はしる, かける, ");

      public class and_a_change_made_elsewhere : after_typing_another_reading_followed_by_a_comma
      {
         public and_a_change_made_elsewhere() => ChangeTheMnemonicElsewhere();

         [XF] public void the_readings_text_still_keeps_the_comma() => Text.Readings.Text.Must().Be("はしる, かける, ");
      }
   }

   public class after_typing_a_new_question_without_committing_it : When_editing_a_vocabs_text_in_the_workspace
   {
      public after_typing_a_new_question_without_committing_it() => Text.Question.Text = "駆ける";

      [XF] public void the_question_has_uncommitted_text() => Text.Question.HasUncommittedText.Must().BeTrue();
      [XF] public void the_vocabs_question_is_unchanged() => _vocab.GetQuestion().Must().Be("走る");

      public class and_a_change_made_elsewhere : after_typing_a_new_question_without_committing_it
      {
         public and_a_change_made_elsewhere() => ChangeTheMnemonicElsewhere();

         [XF] public void the_uncommitted_question_is_kept() => Text.Question.Text.Must().Be("駆ける");
      }

      public class and_committing_it : after_typing_a_new_question_without_committing_it
      {
         public and_committing_it()
         {
            Text.Question.Commit();
            WaitForTheReparseToFinish();
         }

         [XF] public void the_vocabs_question_is_the_new_one() => _vocab.GetQuestion().Must().Be("駆ける");
         [XF] public void the_question_no_longer_has_uncommitted_text() => Text.Question.HasUncommittedText.Must().BeFalse();
      }
   }

   public class with_a_sentence_containing_another_form_of_it : When_editing_a_vocabs_text_in_the_workspace
   {
      readonly SentenceNote _sentence;

      public with_a_sentence_containing_another_form_of_it() => _sentence = CreateSentence("毎日駆ける。");

      [XF] public void the_sentence_does_not_match_the_vocab() => _vocab.Sentences.All().Contains(_sentence).Must().BeFalse();

      public class after_adding_that_form : with_a_sentence_containing_another_form_of_it
      {
         public after_adding_that_form()
         {
            Text.Forms.NewForm = "駆ける";
            Text.Forms.AddCommand.Execute(null);
            WaitForTheReparseToFinish();
         }

         EnteredFormChipViewModel TheFormsChip => Text.Forms.Items.Single(it => it.Text == "駆ける");

         [XF] public void the_form_is_among_the_vocabs_entered_forms() => _vocab.Forms.EnteredFormsSet().Must().Contain("駆ける");
         [XF] public void the_vocab_does_not_own_the_form() => _vocab.Forms.IsOwnedForm("駆ける").Must().BeFalse();
         [XF] public void the_forms_chip_shows_it_as_not_owned() => TheFormsChip.IsOwned.Must().BeFalse();
         [XF] public void the_sentence_is_reparsed_and_matches_the_vocab() => _vocab.Sentences.All().Must().Contain(_sentence);

         public class and_checking_owned_on_its_chip : after_adding_that_form
         {
            public and_checking_owned_on_its_chip()
            {
               TheFormsChip.IsOwned = true;
               WaitForTheReparseToFinish();
            }

            [XF] public void the_vocab_owns_the_form() => _vocab.Forms.IsOwnedForm("駆ける").Must().BeTrue();
            [XF] public void the_forms_chip_shows_it_as_owned() => TheFormsChip.IsOwned.Must().BeTrue();
            [XF] public void the_sentence_still_matches_the_vocab() => _vocab.Sentences.All().Must().Contain(_sentence);
         }
      }
   }
}
