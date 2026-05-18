using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Sentences;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

public class When_working_with_a_SentenceNote : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   public class given_a_newly_created_sentence : When_working_with_a_SentenceNote
   {
      readonly SentenceNote _sentence;

      public given_a_newly_created_sentence() => _sentence = CreateTestSentence("これは本です。", "This is a book.");

      [XF] public void the_question_is_set() => _sentence.GetQuestion().Must().Be("これは本です。");
      [XF] public void the_answer_is_set() => _sentence.GetAnswer().Must().Be("This is a book.");
      [XF] public void it_has_an_id() => _sentence.GetId().Must().NotBeNull();
   }

   public class given_an_answer_with_html : When_working_with_a_SentenceNote
   {
      readonly SentenceNote _sentence;

      public given_an_answer_with_html()
      {
         _sentence = new SentenceNote(NoteServices);
         _sentence.SourceAnswer.Set("<b>This</b> is a <i>book</i>.");
      }

      [XF] public void GetAnswer_strips_bold_tags() => _sentence.GetAnswer().Must().NotContain("<b>");
      [XF] public void GetAnswer_strips_closing_bold_tags() => _sentence.GetAnswer().Must().NotContain("</b>");
      [XF] public void GetAnswer_returns_plain_text() => _sentence.GetAnswer().Must().Be("This is a book.");
   }

   public class given_a_new_sentence_with_default_configuration : When_working_with_a_SentenceNote
   {
      readonly SentenceNote _sentence;

      public given_a_new_sentence_with_default_configuration() => _sentence = new SentenceNote(NoteServices);

      [XF] public void HighlightedWords_is_empty() => _sentence.Configuration.HighlightedWords.Must().BeEmpty();
      [XF] public void IncorrectMatches_is_empty() => _sentence.Configuration.IncorrectMatches.IsEmpty().Must().BeTrue();
      [XF] public void HiddenMatches_is_empty() => _sentence.Configuration.HiddenMatches.IsEmpty().Must().BeTrue();
   }
}
