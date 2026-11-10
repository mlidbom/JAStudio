using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// No two vocab share a question, its text and its disambiguation together (<see cref="VocabNoteQuestion"/>): other notes refer to a vocab by its question,<br/>
/// so two vocab with one question would make those references ambiguous. Creating a version of a vocab that exists already gives back that vocab, and<br/>
/// giving a vocab the question of another fails.
/// </summary>
public class When_a_vocab_would_get_the_question_of_another_vocab : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly VocabNote _vocab;

   public When_a_vocab_would_get_the_question_of_another_vocab() => _vocab = CreateVocab("食べる", "to-eat", "たべる");

   int NumberOfVocab => GetService<VocabCollection>().All().Count;

   public class creating_its_masu_form_when_the_masu_form_exists_already : When_a_vocab_would_get_the_question_of_another_vocab
   {
      readonly VocabNote _masuForm;
      readonly VocabNote _givenBack;
      readonly int _numberOfVocabBefore;

      public creating_its_masu_form_when_the_masu_form_exists_already()
      {
         _masuForm = _vocab.Cloner.CreateMasuForm();
         _masuForm.CorpusAnswer.Set("eat (polite)");
         _numberOfVocabBefore = NumberOfVocab;
         _givenBack = _vocab.Cloner.CreateMasuForm();
      }

      [XF] public void gives_back_the_masu_form_that_exists() => _givenBack.Must().Be(_masuForm);
      [XF] public void creates_no_vocab() => NumberOfVocab.Must().Be(_numberOfVocabBefore);
      [XF] public void leaves_the_masu_form_unchanged() => _masuForm.CorpusAnswer.Value.Must().Be("eat (polite)");
   }

   public class cloning_it_to_a_form_that_another_vocab_has_as_its_question : When_a_vocab_would_get_the_question_of_another_vocab
   {
      readonly VocabNote _vocabOfTheForm;
      readonly VocabNote _givenBack;

      public cloning_it_to_a_form_that_another_vocab_has_as_its_question()
      {
         _vocabOfTheForm = CreateVocab("喰べる", "to-eat (rare kanji)", "たべる");
         _givenBack = _vocab.Cloner.CloneToForm("喰べる");
      }

      [XF] public void gives_back_the_vocab_that_has_the_form_as_its_question() => _givenBack.Must().Be(_vocabOfTheForm);
   }

   public class cloning_it_with_a_disambiguation : When_a_vocab_would_get_the_question_of_another_vocab
   {
      readonly VocabNote _clone;

      public cloning_it_with_a_disambiguation() => _clone = _vocab.Cloner.CloneWithDisambiguation("figurative");

      [XF] public void the_clone_has_the_vocabs_question_text() => _clone.Question.Text.Must().Be("食べる");
      [XF] public void the_clone_has_the_disambiguation() => _clone.Question.Disambiguation.Must().Be("figurative");
      [XF] public void the_clone_has_the_vocabs_answer() => _clone.GetAnswer().Must().Be("to-eat");
      [XF] public void the_vocab_keeps_its_question() => _vocab.Question.DisambiguationName.Must().Be("食べる");

      public class and_then_again_with_the_same_disambiguation : cloning_it_with_a_disambiguation
      {
         readonly VocabNote _givenBack;
         public and_then_again_with_the_same_disambiguation() => _givenBack = _vocab.Cloner.CloneWithDisambiguation("figurative");

         [XF] public void gives_back_the_first_clone() => _givenBack.Must().Be(_clone);
      }

      public class and_then_giving_the_clone_no_disambiguation : cloning_it_with_a_disambiguation
      {
         [XF] public void fails_as_the_vocab_has_that_question() =>
            Invoking(() => _clone.Question.SetDisambiguation("")).Must().Throw<ArgumentAssertionFailedException>();
      }
   }

   public class giving_another_vocab_its_question_text : When_a_vocab_would_get_the_question_of_another_vocab
   {
      readonly VocabNote _otherVocab;

      public giving_another_vocab_its_question_text() => _otherVocab = CreateVocab("飲む", "to-drink", "のむ");

      [XF] public void fails_as_the_vocab_has_that_question() => Invoking(() => _otherVocab.Question.SetText("食べる")).Must().Throw<ArgumentAssertionFailedException>();
   }
}
