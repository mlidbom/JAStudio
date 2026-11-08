using System;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>A vocab's question is always one of its forms, so it's never stored among its entered forms.</summary>
public class When_a_vocabs_forms_include_its_question : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class because_they_were_set_including_it : When_a_vocabs_forms_include_its_question
   {
      readonly VocabNote _vocab;

      public because_they_were_set_including_it()
      {
         _vocab = CreateVocab("走る", "to run", "はしる");
         _vocab.Forms.SetEnteredForms([EnteredForm.NotOwned("走る"), EnteredForm.NotOwned("駆ける")]);
      }

      [XF] public void the_question_is_not_among_the_entered_forms() => _vocab.Forms.EnteredFormsList().Must().SequenceEqual(["駆ける"]);
      [XF] public void the_question_is_matched() => _vocab.Forms.MatchedFormsSet().Must().Contain("走る");
      [XF] public void the_other_form_is_matched() => _vocab.Forms.MatchedFormsSet().Must().Contain("駆ける");
      [XF] public void the_question_comes_first_among_all_the_forms() => string.Join(", ", _vocab.Forms.QuestionAndEnteredFormsList()).Must().Be("走る, 駆ける");
      [XF] public void the_stored_forms_are_only_the_other_form() => ((VocabDto)_vocab.ToDto()).Forms.Select(it => it.Text).Must().SequenceEqual(["駆ける"]);
   }

   public class because_they_were_stored_including_it_owned : When_a_vocabs_forms_include_its_question
   {
      readonly VocabNote _vocab;

      public because_they_were_stored_including_it_owned() =>
         _vocab = new VocabNote(NoteServices,
                                new VocabDto
                                {
                                   Id = Guid.NewGuid(),
                                   Question = new VocabQuestionDto { Text = "走る" },
                                   Forms = [new EnteredFormDto { Text = "走る", IsOwned = true }, new EnteredFormDto { Text = "駆ける" }]
                                });

      [XF] public void the_question_is_dropped_from_the_entered_forms() => _vocab.Forms.EnteredFormsList().Must().SequenceEqual(["駆ける"]);
      [XF] public void the_question_is_owned() => _vocab.Forms.IsOwnedForm("走る").Must().BeTrue();
      [XF] public void the_question_is_matched() => _vocab.Forms.MatchedFormsSet().Must().Contain("走る");
   }

   public class after_adding_the_question_as_a_form : When_a_vocabs_forms_include_its_question
   {
      readonly VocabNote _vocab;

      public after_adding_the_question_as_a_form()
      {
         _vocab = CreateVocab("走る", "to run", "はしる");
         _vocab.Forms.Add("走る");
      }

      [XF] public void the_entered_forms_stay_empty() => _vocab.Forms.EnteredFormsList().Must().BeEmpty();
   }
}
