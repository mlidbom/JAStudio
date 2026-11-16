namespace JAStudio.Core.Note.Vocabulary.Tevents;

public partial class VocabTevent
{
   public static class Question
   {
      public class TextChanged(string text) : VocabTevent, IVocabTevent.Question.TextChanged
      {
         public string Text { get; } = text;
      }

      public class DisambiguationChanged(string disambiguation) : VocabTevent, IVocabTevent.Question.DisambiguationChanged
      {
         public string Disambiguation { get; } = disambiguation;
      }
   }
}
