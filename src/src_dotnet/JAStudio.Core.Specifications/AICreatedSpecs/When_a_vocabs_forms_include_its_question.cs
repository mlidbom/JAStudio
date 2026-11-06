using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Anki;
using JAStudio.Core.Note.CorpusData;
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
         _vocab.Forms.SetEnteredForms(["走る", "駆ける"]);
      }

      [XF] public void the_question_is_not_among_the_entered_forms() => _vocab.Forms.EnteredFormsWithBracketsString().Must().Be("駆ける");
      [XF] public void the_question_is_matched() => _vocab.Forms.MatchedFormsSet().Must().Contain("走る");
      [XF] public void the_other_form_is_matched() => _vocab.Forms.MatchedFormsSet().Must().Contain("駆ける");
      [XF] public void the_question_comes_first_among_all_the_forms() => string.Join(", ", _vocab.Forms.QuestionAndEnteredFormsList()).Must().Be("走る, 駆ける");
      [XF] public void the_stored_forms_are_only_the_other_form() => string.Join(", ", ((VocabData)_vocab.ToCorpusData()).Forms).Must().Be("駆ける");
      [XF] public void ankis_forms_field_has_the_question_first() => _vocab.ToCorpusData().ToNoteData().Fields[AnkiFieldNames.Vocab.Forms].Must().Be("走る, 駆ける");
   }

   public class because_they_were_stored_including_it_bracketed : When_a_vocabs_forms_include_its_question
   {
      readonly VocabNote _vocab;

      public because_they_were_stored_including_it_bracketed() =>
         _vocab = new VocabNote(NoteServices, new VocabData { Id = Guid.NewGuid(), Question = "走る", Forms = ["[走る]", "駆ける"] });

      [XF] public void the_question_is_dropped_from_the_entered_forms() => _vocab.Forms.EnteredFormsWithBracketsString().Must().Be("駆ける");
      [XF] public void the_question_is_owned() => _vocab.Forms.IsOwnedForm("走る").Must().BeTrue();
      [XF] public void the_question_is_matched() => _vocab.Forms.MatchedFormsSet().Must().Contain("走る");
   }

   public class with_a_disambiguated_question : When_a_vocabs_forms_include_its_question
   {
      readonly VocabNote _vocab;

      public with_a_disambiguated_question()
      {
         _vocab = CreateVocab("走る:run", "to run", "はしる");
         _vocab.Forms.SetEnteredForms(["駆ける"]);
      }

      [XF] public void ankis_forms_field_has_the_question_without_its_disambiguation_first() =>
         _vocab.ToCorpusData().ToNoteData().Fields[AnkiFieldNames.Vocab.Forms].Must().Be("走る, 駆ける");
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
