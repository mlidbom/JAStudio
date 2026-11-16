namespace JAStudio.Core.Note.Vocabulary.Tevents;

public partial class VocabTevent
{
   /// <summary>The base class of the classes of the <see cref="IVocabTevent.Question"/> tevents, which <see cref="VocabNoteQuestion"/> publishes.</summary>
   public abstract class Question : VocabTevent, IVocabTevent.Question
   {
      public class TextChanged(string text) : Question, IVocabTevent.Question.TextChanged
      {
         public string Text { get; } = text;
      }

      public class DisambiguationChanged(string disambiguation) : Question, IVocabTevent.Question.DisambiguationChanged
      {
         public string Disambiguation { get; } = disambiguation;
      }
   }
}
